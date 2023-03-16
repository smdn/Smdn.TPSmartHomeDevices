// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
#if SYSTEM_DIAGNOSTICS_UNREACHABLEEXCEPTION
using System.Diagnostics;
#endif
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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

  private readonly KasaClientExceptionHandler exceptionHandler;
  private readonly ArrayBufferWriter<byte> buffer = new(initialCapacity: KasaClient.DefaultBufferCapacity);
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

    exceptionHandler = serviceProvider?.GetService<KasaClientExceptionHandler>() ?? KasaClientExceptionHandler.Default;
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
    const int maxAttempts = 5;
    EndPoint? endPoint = null;

    for (var attempt = 0; attempt < maxAttempts; attempt++) {
      if (endPoint is null) {
        endPoint = await ResolveEndPointAsync(cancellationToken).ConfigureAwait(false);

        if (client is not null && !client.EndPoint.Equals(endPoint)) {
          // endpoint has changed, recreate client with new endpoint
          client.Logger?.LogInformation($"Endpoint has changed: {client.EndPoint} -> {endPoint}");
          client.Dispose();
          client = null;
        }
      }

      cancellationToken.ThrowIfCancellationRequested();

      client ??= new KasaClient(
        endPoint: endPoint,
        buffer: buffer,
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
      catch (Exception ex) {
        var handling = exceptionHandler.DetermineHandling(ex, attempt, client.Logger);

        switch (handling) {
          case KasaClientExceptionHandling.Throw:
          default:
            client.Dispose();
            client = null;
            throw;

          case KasaClientExceptionHandling.Retry:
            continue;

          case KasaClientExceptionHandling.RetryAfterReconnect:
            client.Dispose();
            client = null;
            continue;

          case KasaClientExceptionHandling.RetryAfterResolveEndPoint:
            if (deviceEndPointProvider is not IDynamicDeviceEndPointProvider dynamicEndPoint)
              goto case KasaClientExceptionHandling.Throw;

            // mark end point as invalid to have the end point refreshed or rescanned
            dynamicEndPoint.InvalidateEndPoint();

            endPoint = null; // should resolve end point

            continue;
        } // switch (handling)
      } // try
    } // for

#if SYSTEM_DIAGNOSTICS_UNREACHABLEEXCEPTION
    throw new UnreachableException();
#else
    throw new NotImplementedException("unreachable");
#endif
  }
}
