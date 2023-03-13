// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
#if SYSTEM_DIAGNOSTICS_UNREACHABLEEXCEPTION
using System.Diagnostics;
#endif
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

public partial class TapoDevice : IDisposable {
  private IDeviceEndPointProvider? deviceEndPointProvider; // if null, it indicates a 'disposed' state.
  protected bool IsDisposed => deviceEndPointProvider is null;

  private readonly ITapoCredentialProvider? credentialProvider;
  private readonly IServiceProvider? serviceProvider;
  public string TerminalUuidString { get; } // must be in the format of 00000000-0000-0000-0000-000000000000

  private TapoClient? client;

  /// <summary>
  /// Gets a current session information represented by <see cref="TapoSession" />.
  /// If session has not been established or been disposed, returns <see langword="null"/>.
  /// </summary>
  public TapoSession? Session => client?.Session;

  protected TapoDevice(
    string hostName,
    string email,
    string password,
    Guid? terminalUuid = null,
    IServiceProvider? serviceProvider = null
  )
    : this(
      deviceEndPointProvider: TapoDeviceEndPointProvider.Create(
        hostName ?? throw new ArgumentNullException(nameof(hostName))
      ),
      terminalUuid: terminalUuid,
      credentialProvider: new PlainTextCredentialProvider(
        userName: email ?? throw new ArgumentNullException(nameof(email)),
        password: password ?? throw new ArgumentNullException(nameof(password))
      ),
      serviceProvider: serviceProvider
    )
  {
  }

  protected TapoDevice(
    IPAddress ipAddress,
    string email,
    string password,
    Guid? terminalUuid = null,
    IServiceProvider? serviceProvider = null
  )
    : this(
      deviceEndPointProvider: TapoDeviceEndPointProvider.Create(
        ipAddress ?? throw new ArgumentNullException(nameof(ipAddress))
      ),
      terminalUuid: terminalUuid,
      credentialProvider: new PlainTextCredentialProvider(
        userName: email ?? throw new ArgumentNullException(nameof(email)),
        password: password ?? throw new ArgumentNullException(nameof(password))
      ),
      serviceProvider: serviceProvider
    )
  {
  }

  protected TapoDevice(
    string hostName,
    Guid? terminalUuid = null,
    IServiceProvider? serviceProvider = null
  )
    : this(
      deviceEndPointProvider: TapoDeviceEndPointProvider.Create(
        hostName ?? throw new ArgumentNullException(nameof(hostName))
      ),
      terminalUuid: terminalUuid,
      credentialProvider: null,
      serviceProvider: serviceProvider
    )
  {
  }

  protected TapoDevice(
    IDeviceEndPointProvider deviceEndPointProvider,
    Guid? terminalUuid = null,
    ITapoCredentialProvider? credentialProvider = null,
    IServiceProvider? serviceProvider = null
  )
  {
    this.deviceEndPointProvider = deviceEndPointProvider ?? throw new ArgumentNullException(nameof(deviceEndPointProvider));
    this.credentialProvider = credentialProvider;
    this.serviceProvider = serviceProvider;

    TerminalUuidString = GetOrGenerateTerminalUuidString(terminalUuid);

    static string GetOrGenerateTerminalUuidString(Guid? terminalUuid)
    {
      var uuid = terminalUuid ?? Guid.NewGuid();

      return uuid.ToString("D", provider: null);
    }
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
        defaultPort: TapoClient.DefaultPort,
        cancellationToken: cancellationToken
      );
  }

  protected ValueTask EnsureSessionEstablishedAsync(
    CancellationToken cancellationToken = default
  )
  {
    ThrowIfDisposed();

    return EnsureSessionEstablishedAsyncCore(
      cancellationToken: cancellationToken
    );
  }

  private async ValueTask EnsureSessionEstablishedAsyncCore(
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();

    var endPoint = await ResolveEndPointAsync(cancellationToken).ConfigureAwait(false);

    if (client is not null && !client.EndPoint.Equals(endPoint)) {
      // endpoint has changed, recreate client with new endpoint
      client.DisposeWithLog(LogLevel.Information, exception: null, $"Endpoint has changed: {client.EndPoint} -> {endPoint}");
      client = null;
    }

    client ??= new TapoClient(
      endPoint: endPoint,
      credentialProvider: credentialProvider,
      serviceProvider: serviceProvider
    );

    if (client.Session is not null)
      return;

    cancellationToken.ThrowIfCancellationRequested();

    await client.AuthenticateAsync(
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);
  }

  protected Task<TResult> SendRequestAsync<TRequest, TResponse, TResult>(
    TRequest request,
    Func<TResponse, TResult> composeResult,
    CancellationToken cancellationToken = default
  )
    where TRequest : ITapoPassThroughRequest
    where TResponse : ITapoPassThroughResponse
  {
    if (request is null)
      throw new ArgumentNullException(nameof(request));
    if (composeResult is null)
      throw new ArgumentNullException(nameof(composeResult));

    ThrowIfDisposed();

    return SendRequestAsyncCore(
      request: request,
      composeResult: composeResult,
      cancellationToken: cancellationToken
    );
  }

  private async Task<TResult> SendRequestAsyncCore<TRequest, TResponse, TResult>(
    TRequest request,
    Func<TResponse, TResult> composeResult,
    CancellationToken cancellationToken = default
  )
    where TRequest : ITapoPassThroughRequest
    where TResponse : ITapoPassThroughResponse
  {
    for (var attempt = 0; attempt <= 1; attempt++) {
      await EnsureSessionEstablishedAsync(cancellationToken).ConfigureAwait(false);

      cancellationToken.ThrowIfCancellationRequested();

      try {
        var response = await client.SendRequestAsync<TRequest, TResponse>(
          request: request,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        return composeResult(response);
      }
      catch (HttpRequestException ex) when (
        attempt == 0 &&
        !deviceEndPointProvider.IsStaticEndPoint &&
        ex.InnerException is SocketException exSocket &&
        exSocket.SocketErrorCode switch {
          SocketError.ConnectionRefused => true, // ECONNREFUSED
          SocketError.HostUnreachable => true, // EHOSTUNREACH
          SocketError.NetworkUnreachable => true, // ENETUNREACH
          _ => false,
        }
      ) {
        // The end point may have changed.
        // Dispose the current HTTP client in order to recreate the client and try again.
        client.DisposeWithLog(LogLevel.Information, exception: null, $"Endpoint may have changed: ({nameof(exSocket.SocketErrorCode)}: {(int)exSocket.SocketErrorCode})");
        client = null;

        continue;
      }
      catch (HttpRequestException ex) when (ex.InnerException is SocketException exSocket) {
        // The HTTP client may have been invalid due to an exception at the transport layer.
        // Dispose the current HTTP client and rethrow exception.
        client.DisposeWithLog(LogLevel.Error, ex, $"Unexpected socket exception ({nameof(exSocket.SocketErrorCode)}: {(int)exSocket.SocketErrorCode})");
        client = null;

        throw;
      }
      catch (SecurePassThroughInvalidPaddingException ex) {
        // The session might have been in invalid state(?)
        // Dispose the current HTTP client in order to recreate the client and try again from establishing session.
        client.DisposeWithLog(LogLevel.Warning, ex, "Invalid padding in secure pass through");
        client = null;

        continue;
      }
      catch (TapoErrorResponseException ex) when (
        // request failed with error code -1301
        attempt == 0 &&
        ex.ErrorCode == (ErrorCode)(-1301)
      ) {
        // The session might have been in invalid state(?)
        // Dispose the current HTTP client in order to recreate the client and try again from establishing session.
        client.DisposeWithLog(LogLevel.Warning, ex, "Error code -1301");
        client = null;

        continue;
      }
      catch (TapoErrorResponseException ex) when (attempt == 0) {
        // The session may have been invalid.
        // Dispose the current session in order to re-establish the session and try again.
        client.CloseSessionWithLog(LogLevel.Warning, ex, $"Unexpected error response ({nameof(ex.ErrorCode)}: {(int)ex.ErrorCode})");

        continue;
      }
      catch (Exception ex) {
        // Dispose the current client and rethrow exception.
        client.DisposeWithLog(LogLevel.Error, ex, $"Unexpected exception ({ex.GetType().FullName})");
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

  public Task<TapoDeviceInfo> GetDeviceInfoAsync(
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync<GetDeviceInfoRequest, GetDeviceInfoResponse, TapoDeviceInfo>(
      request: default,
      composeResult: static resp => resp.Result,
      cancellationToken: cancellationToken
    );

  public Task SetDeviceInfoAsync<TParameters>(
    TParameters parameters,
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync<SetDeviceInfoRequest<TParameters>, SetDeviceInfoResponse, None>(
      request: new(
        terminalUuid: TerminalUuidString,
        parameters: parameters
      ),
      composeResult: static resp => default,
      cancellationToken: cancellationToken
    );
}
