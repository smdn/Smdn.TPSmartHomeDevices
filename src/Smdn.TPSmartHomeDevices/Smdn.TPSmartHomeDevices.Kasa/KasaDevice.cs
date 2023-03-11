// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
#if SYSTEM_DIAGNOSTICS_UNREACHABLEEXCEPTION
using System.Diagnostics;
#endif
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Smdn.TPSmartHomeDevices.Kasa.Protocol;

namespace Smdn.TPSmartHomeDevices.Kasa;

public partial class KasaDevice : IDisposable {
  protected static readonly JsonEncodedText ModuleTextSystem = JsonEncodedText.Encode(
#if LANG_VERSION_11_OR_GREATER
    "system"u8
#else
    "system"
#endif
  );
  protected static readonly JsonEncodedText MethodTextGetSysInfo = JsonEncodedText.Encode(
#if LANG_VERSION_11_OR_GREATER
    "get_sysinfo"u8
#else
    "get_sysinfo"
#endif
  );

  protected readonly struct NullParameter { }

  private IDeviceEndPointProvider? deviceEndPointProvider; // if null, it indicates a 'disposed' state.
  protected bool IsDisposed => deviceEndPointProvider is null;

  private readonly IServiceProvider? serviceProvider;

  private KasaClient? client;

  public bool IsConnected {
    get {
      ThrowIfDisposed();
      return client is not null && client.IsConnected;
    }
  }

  protected KasaDevice(
    string hostName,
    IServiceProvider? serviceProvider = null
  )
    : this(
      deviceEndPointProvider: KasaDeviceEndPointProvider.Create(
        hostName ?? throw new ArgumentNullException(nameof(hostName))
      ),
      serviceProvider: serviceProvider
    )
  {
  }

  protected KasaDevice(
    IPAddress ipAddress,
    IServiceProvider? serviceProvider = null
  )
    : this(
      deviceEndPointProvider: KasaDeviceEndPointProvider.Create(
        ipAddress ?? throw new ArgumentNullException(nameof(ipAddress))
      ),
      serviceProvider: serviceProvider
    )
  {
  }

  protected KasaDevice(
    IDeviceEndPointProvider deviceEndPointProvider,
    IServiceProvider? serviceProvider = null
  )
  {
    this.deviceEndPointProvider = deviceEndPointProvider ?? throw new ArgumentNullException(nameof(deviceEndPointProvider));
    this.serviceProvider = serviceProvider;
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!disposing)
      return;

    deviceEndPointProvider = null; // mark as disposed

    client?.Dispose();
    client = null;
  }

  private void ThrowIfDisposed()
  {
    if (IsDisposed)
      throw new ObjectDisposedException(GetType().FullName);
  }

  public ValueTask<EndPoint> ResolveEndPointAsync(
    CancellationToken cancellationToken = default
  )
  {
    ThrowIfDisposed();

    return cancellationToken.IsCancellationRequested
      ?
#if SYSTEM_THREADING_TASKS_VALUETASK_FROMCANCELED
        ValueTask.FromCanceled<EndPoint>(cancellationToken)
#else
        ValueTaskShim.FromCanceled<EndPoint>(cancellationToken)
#endif
      : deviceEndPointProvider.ResolveOrThrowAsync(
        defaultPort: KasaClient.DefaultPort,
        cancellationToken: cancellationToken
      );
  }

  protected Task SendRequestAsync<TMethodParameter>(
    JsonEncodedText module,
    JsonEncodedText method,
    TMethodParameter parameters,
    CancellationToken cancellationToken
  )
  {
    if (parameters is null)
      throw new ArgumentNullException(nameof(parameters));

    ThrowIfDisposed();

    return cancellationToken.IsCancellationRequested
      ? Task.FromCanceled(cancellationToken)
      : SendRequestAsyncCore(
        module: module,
        method: method,
        parameters: parameters,
        composeResult: NoneConverter,
        cancellationToken: cancellationToken
      );

    static None NoneConverter(JsonElement result) => default;
  }

  protected Task<TMethodResult> SendRequestAsync<TMethodResult>(
    JsonEncodedText module,
    JsonEncodedText method,
    Func<JsonElement, TMethodResult> composeResult,
    CancellationToken cancellationToken
  )
  {
    if (composeResult is null)
      throw new ArgumentNullException(nameof(composeResult));

    ThrowIfDisposed();

    return cancellationToken.IsCancellationRequested
      ? Task.FromCanceled<TMethodResult>(cancellationToken)
      : SendRequestAsyncCore<NullParameter, TMethodResult>(
        module: module,
        method: method,
        parameters: default,
        composeResult: composeResult,
        cancellationToken: cancellationToken
      );
  }

  protected Task<TMethodResult> SendRequestAsync<TMethodParameter, TMethodResult>(
    JsonEncodedText module,
    JsonEncodedText method,
    TMethodParameter parameters,
    Func<JsonElement, TMethodResult> composeResult,
    CancellationToken cancellationToken
  )
  {
    if (parameters is null)
      throw new ArgumentNullException(nameof(parameters));
    if (composeResult is null)
      throw new ArgumentNullException(nameof(composeResult));

    ThrowIfDisposed();

    return cancellationToken.IsCancellationRequested
      ? Task.FromCanceled<TMethodResult>(cancellationToken)
      : SendRequestAsyncCore(
        module: module,
        method: method,
        parameters: parameters,
        composeResult: composeResult,
        cancellationToken: cancellationToken
      );
  }

  private async Task<TMethodResult> SendRequestAsyncCore<TMethodParameter, TMethodResult>(
    JsonEncodedText module,
    JsonEncodedText method,
    TMethodParameter parameters,
    Func<JsonElement, TMethodResult> composeResult,
    CancellationToken cancellationToken
  )
  {
    var endPoint = await ResolveEndPointAsync(cancellationToken).ConfigureAwait(false);

    if (client is not null && !client.EndPoint.Equals(endPoint)) {
      // endpoint has changed, recreate client with new endpoint
      client.DisposeWithLog(LogLevel.Information, $"Endpoint has changed: {client.EndPoint} -> {endPoint}");
      client = null;
    }

    for (var attempt = 0; attempt <= 1; attempt++) {
      cancellationToken.ThrowIfCancellationRequested();

      client ??= new KasaClient(
        endPoint: endPoint,
        serviceProvider: serviceProvider
      );

      try {
        return await client.SendAsync(
          module: module,
          method: method,
          parameter: parameters,
          composeResult: composeResult,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
      }
      catch (SocketException ex) when (
        attempt == 0 &&
        !deviceEndPointProvider.IsStaticEndPoint &&
        ex.SocketErrorCode switch {
          SocketError.ConnectionRefused => true, // ECONNREFUSED
          SocketError.HostUnreachable => true, // EHOSTUNREACH
          SocketError.NetworkUnreachable => true, // ENETUNREACH
          _ => false,
        }
      ) {
        // The end point may have changed.
        // Dispose the current client in order to recreate the client and try again.
        client.DisposeWithLog(LogLevel.Information, $"Endpoint may have changed ({nameof(ex.SocketErrorCode)}: {(int)ex.SocketErrorCode})");
        client = null;

        continue;
      }
      catch (SocketException ex) {
        // The client may have been invalid due to an exception at the transport layer.
        // Dispose the current client and rethrow exception.
        client.DisposeWithLog(LogLevel.Error, $"Unexpected socket exception ({nameof(ex.SocketErrorCode)}: {(int)ex.SocketErrorCode})");
        client = null;

        throw;
      }
      catch (KasaDisconnectedException ex) when (attempt == 0) {
        // The peer device disconnected the connection, or may have dropped the connection.
        // Dispose the current client in order to recreate the client and try again.
        if (ex.InnerException is SocketException exSocket)
          client.DisposeWithLog(LogLevel.Debug, $"Disconnected ({nameof(exSocket.SocketErrorCode)}: {(int)exSocket.SocketErrorCode})");
        else
          client.DisposeWithLog(LogLevel.Debug, $"Disconnected ({ex.Message})");

        client = null;

        continue;
      }
      catch (Exception ex) {
        // Dispose the current client and rethrow exception.
        client.DisposeWithLog(LogLevel.Error, $"Unexpected exception ({ex.GetType().FullName})");
        client = null;

        throw;
      }
    }

#if SYSTEM_DIAGNOSTICS_UNREACHABLEEXCEPTION
    throw new UnreachableException();
#else
    throw new NotImplementedException("unreachable");
#endif
  }
}
