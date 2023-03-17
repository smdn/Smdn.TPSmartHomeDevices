// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
#if SYSTEM_DIAGNOSTICS_UNREACHABLEEXCEPTION
using System.Diagnostics;
#endif
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

public partial class TapoDevice : IDisposable {
  private IDeviceEndPointProvider? deviceEndPointProvider; // if null, it indicates a 'disposed' state.
  protected bool IsDisposed => deviceEndPointProvider is null;

  private readonly ITapoCredentialProvider? credentialProvider;
  private readonly TapoClientExceptionHandler exceptionHandler;
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
      deviceEndPointProvider: TapoDeviceEndPointProvider.Create(hostName),
      terminalUuid: terminalUuid,
      credentialProvider: PlainTextCredentialProvider.Create(email, password),
      exceptionHandler: null,
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
      deviceEndPointProvider: TapoDeviceEndPointProvider.Create(ipAddress),
      terminalUuid: terminalUuid,
      credentialProvider: PlainTextCredentialProvider.Create(email, password),
      exceptionHandler: null,
      serviceProvider: serviceProvider
    )
  {
  }

  protected TapoDevice(
    PhysicalAddress macAddress,
    string email,
    string password,
    IDeviceEndPointFactory<PhysicalAddress> endPointFactory,
    IServiceProvider? serviceProvider = null
  )
    : this(
      deviceEndPointProvider: TapoDeviceEndPointProvider.Create(macAddress, endPointFactory),
      terminalUuid: null,
      credentialProvider: PlainTextCredentialProvider.Create(email, password),
      exceptionHandler: null,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="TapoDevice"/> class with a MAC address.
  /// </summary>
  /// <param name="macAddress">
  /// A <see cref="PhysicalAddress"/> that holds the MAC address representing the device end point.
  /// </param>
  /// <param name="email">
  /// A <see cref="string"/> that holds the e-mail address of the Tapo account used for authentication.
  /// </param>
  /// <param name="password">
  /// A <see cref="string"/> that holds the password of the Tapo account used for authentication.
  /// </param>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/>.
  /// <see cref="IDeviceEndPointFactory&lt;PhysicalAddress&gt;"/> must be registered to create an end point from the <paramref name="macAddress"/>.
  /// </param>
  /// <exception cref="InvalidOperationException">No service for type <see cref="IDeviceEndPointFactory&lt;PhysicalAddress&gt;"/> has been registered for <see cref="serviceProvider"/>.</exception>
  protected TapoDevice(
    PhysicalAddress macAddress,
    string email,
    string password,
    IServiceProvider serviceProvider
  )
    : this(
      deviceEndPointProvider: TapoDeviceEndPointProvider.Create(macAddress, serviceProvider),
      terminalUuid: null,
      credentialProvider: PlainTextCredentialProvider.Create(email, password),
      exceptionHandler: null,
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
      deviceEndPointProvider: TapoDeviceEndPointProvider.Create(hostName),
      terminalUuid: terminalUuid,
      credentialProvider: null,
      exceptionHandler: null,
      serviceProvider: serviceProvider
    )
  {
  }

  protected TapoDevice(
    IDeviceEndPointProvider deviceEndPointProvider,
    Guid? terminalUuid = null,
    ITapoCredentialProvider? credentialProvider = null,
    TapoClientExceptionHandler? exceptionHandler = null,
    IServiceProvider? serviceProvider = null
  )
  {
    this.deviceEndPointProvider = deviceEndPointProvider ?? throw new ArgumentNullException(nameof(deviceEndPointProvider));
    this.credentialProvider = credentialProvider
      ?? serviceProvider?.GetRequiredService<ITapoCredentialProvider>()
      ?? throw new ArgumentNullException(nameof(credentialProvider));
    this.exceptionHandler = exceptionHandler
      ?? serviceProvider?.GetService<TapoClientExceptionHandler>()
      ?? TapoClientExceptionHandler.Default;
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
      client.Logger?.LogInformation($"Endpoint has changed: {client.EndPoint} -> {endPoint}");
      client.Dispose();
      client = null;
    }

    client ??= new TapoClient(
      endPoint: endPoint,
      credentialProvider: credentialProvider,
      httpClientFactory: serviceProvider?.GetService<IHttpClientFactory>(),
      logger: serviceProvider?.GetService<ILoggerFactory>()?.CreateLogger($"{nameof(TapoClient)}({endPoint})") // TODO: logger category name
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
    const int maxAttempts = 5;

    for (var attempt = 0; attempt < maxAttempts; attempt++) {
      await EnsureSessionEstablishedAsync(cancellationToken).ConfigureAwait(false);

      cancellationToken.ThrowIfCancellationRequested();

      try {
        var response = await client.SendRequestAsync<TRequest, TResponse>(
          request: request,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        return composeResult(response);
      }
      catch (Exception ex) {
        var endPointUri = client.EndPointUri;
        var handling = exceptionHandler.DetermineHandling(ex, attempt, client.Logger);

        switch (handling) {
          case TapoClientExceptionHandling.Throw:
          default:
            client.Dispose();
            client = null;
            throw;

          case TapoClientExceptionHandling.ThrowWrapTapoProtocolException:
            client.Dispose();
            client = null;
            throw new TapoProtocolException(
              message: "Unhandled exception",
              endPoint: endPointUri,
              innerException: ex
            );

          case TapoClientExceptionHandling.Retry:
            continue;

          case TapoClientExceptionHandling.RetryAfterReconnect:
            client.Dispose();
            client = null;
            continue;

          case TapoClientExceptionHandling.RetryAfterReestablishSession:
            client.CloseSession();
            continue;

          case TapoClientExceptionHandling.RetryAfterResolveEndPoint:
            if (deviceEndPointProvider is not IDynamicDeviceEndPointProvider dynamicEndPoint)
              goto case TapoClientExceptionHandling.Throw;

            // mark end point as invalid to have the end point refreshed or rescanned
            dynamicEndPoint.InvalidateEndPoint();

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
