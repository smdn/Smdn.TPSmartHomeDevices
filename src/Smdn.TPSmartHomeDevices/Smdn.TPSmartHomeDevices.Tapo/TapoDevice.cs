// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
#if SYSTEM_DIAGNOSTICS_UNREACHABLEEXCEPTION
using System.Diagnostics;
#endif
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Smdn.TPSmartHomeDevices.Tapo.Credentials;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

public partial class TapoDevice : ITapoCredentialIdentity, IDisposable {
  private IDeviceEndPointProvider? deviceEndPointProvider; // if null, it indicates a 'disposed' state.
  protected bool IsDisposed => deviceEndPointProvider is null;

  private readonly ITapoCredentialProvider? credential;
  private readonly TapoClientExceptionHandler exceptionHandler;
  private readonly IServiceProvider? serviceProvider;
  public string TerminalUuidString { get; } // must be in the format of 00000000-0000-0000-0000-000000000000

  private TapoClient? client;

  /// <summary>
  /// Gets a current session information represented by <see cref="TapoSession" />.
  /// If session has not been established or been disposed, returns <see langword="null"/>.
  /// </summary>
  public TapoSession? Session => client?.Session;

  string ITapoCredentialIdentity.Name {
    get {
      ThrowIfDisposed();
      return $"{GetType().FullName} ({deviceEndPointProvider})";
    }
  }

  /// <summary>
  /// Gets or sets the timeout value for HTTP requests.
  /// </summary>
  /// <value>
  /// <para>If the value is <see langword="null"/>, the timeout value configured by the <see cref="IHttpClientFactory"/> is used. Otherwise, the specified <see cref="TimeSpan"/> is used for the timeout value for this instance.</para>
  /// <para>Therefore, even if the value is <see langword="null"/>, a timeout may still occur. If you do not want to timeout for this instance, specify <see cref="Timeout.InfiniteTimeSpan"/> explicitly.</para>
  /// </value>
  public TimeSpan? Timeout { get; set; }

  /// <summary>
  /// Initializes a new instance of the <see cref="TapoDevice"/> class with specifying the device endpoint by host name.
  /// </summary>
  /// <param name="host">
  /// A <see cref="string"/> that holds the host name or IP address string, representing the device endpoint.
  /// </param>
  /// <param name="email">
  /// A <see cref="string"/> that holds the e-mail address of the Tapo account used for authentication.
  /// </param>
  /// <param name="password">
  /// A <see cref="string"/> that holds the password of the Tapo account used for authentication.
  /// </param>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/>.
  /// </param>
  protected TapoDevice(
    string host,
    string email,
    string password,
    IServiceProvider? serviceProvider = null
  )
    : this(
      deviceEndPointProvider: TapoDeviceEndPointProvider.Create(host),
      credential: TapoCredentialProviderFactory.CreateFromPlainText(email, password),
      exceptionHandler: null,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <param name="host">
  /// A <see cref="string"/> that holds the host name or IP address string, representing the device endpoint.
  /// </param>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/>.
  /// <see cref="ITapoCredentialProvider"/> must be registered in order to retrieve the credentials required for authentication.
  /// </param>
  /// <inheritdoc cref="TapoDevice(string, string, string, IServiceProvider?)" path="/summary"/>
  protected TapoDevice(
    string host,
    IServiceProvider serviceProvider
  )
    : this(
      deviceEndPointProvider: TapoDeviceEndPointProvider.Create(host),
      credential: null,
      exceptionHandler: null,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="TapoDevice"/> class with specifying the device endpoint by IP address.
  /// </summary>
  /// <param name="ipAddress">
  /// A <see cref="IPAddress"/> that holds the IP address representing the device end point.
  /// </param>
  /// <inheritdoc cref="TapoDevice(string, string, string, IServiceProvider?)" path="/param[@name='email' or @name='password' or @name='serviceProvider']"/>
  protected TapoDevice(
    IPAddress ipAddress,
    string email,
    string password,
    IServiceProvider? serviceProvider = null
  )
    : this(
      deviceEndPointProvider: TapoDeviceEndPointProvider.Create(ipAddress),
      credential: TapoCredentialProviderFactory.CreateFromPlainText(email, password),
      exceptionHandler: null,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="TapoDevice"/> class with specifying the device endpoint by IP address.
  /// </summary>
  /// <inheritdoc cref="TapoDevice(IPAddress, string, string, IServiceProvider?)" path="/summary | /param[@name='ipAddress']"/>
  /// <inheritdoc cref="TapoDevice(string, IServiceProvider)" path="/param[@name='serviceProvider']"/>
  protected TapoDevice(
    IPAddress ipAddress,
    IServiceProvider serviceProvider
  )
    : this(
      deviceEndPointProvider: TapoDeviceEndPointProvider.Create(ipAddress),
      credential: null,
      exceptionHandler: null,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="TapoDevice"/> class with specifying the device endpoint by MAC address.
  /// </summary>
  /// <param name="macAddress">
  /// A <see cref="PhysicalAddress"/> that holds the MAC address representing the device end point.
  /// </param>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/>.
  /// <see cref="IDeviceEndPointFactory{PhysicalAddress}"/> must be registered to create an end point from the <paramref name="macAddress"/>.
  /// </param>
  /// <exception cref="InvalidOperationException">No service for type <see cref="IDeviceEndPointFactory{PhysicalAddress}"/> has been registered for <paramref name="serviceProvider"/>.</exception>
  /// <inheritdoc cref="TapoDevice(string, string, string, IServiceProvider?)" path="/param[@name='email' or @name='password']"/>
  protected TapoDevice(
    PhysicalAddress macAddress,
    string email,
    string password,
    IServiceProvider serviceProvider
  )
    : this(
      deviceEndPointProvider: TapoDeviceEndPointProvider.Create(macAddress, serviceProvider),
      credential: TapoCredentialProviderFactory.CreateFromPlainText(email, password),
      exceptionHandler: null,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/>.
  /// <see cref="ITapoCredentialProvider"/> must be registered in order to retrieve the credentials required for authentication.
  /// <see cref="IDeviceEndPointFactory&lt;PhysicalAddress&gt;"/> must also be registered to create an <see cref="IDeviceEndPointProvider" />, corresponding to the <paramref name="macAddress"/>.
  /// </param>
  /// <exception cref="InvalidOperationException">No service for type <see cref="ITapoCredentialProvider"/> and/or <see cref="IDeviceEndPointFactory{PhysicalAddress}"/> has been registered for <paramref name="serviceProvider"/>.</exception>
  /// <inheritdoc cref="TapoDevice(PhysicalAddress, string, string, IServiceProvider?)" path="/summary | /param[@name='macAddress']"/>
  protected TapoDevice(
    PhysicalAddress macAddress,
    IServiceProvider serviceProvider
  )
    : this(
      deviceEndPointProvider: TapoDeviceEndPointProvider.Create(macAddress, serviceProvider),
      credential: null,
      exceptionHandler: null,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="TapoDevice"/> class.
  /// </summary>
  /// <param name="deviceEndPointProvider">
  /// A <see cref="IDeviceEndPointProvider"/> that provides the device end point.
  /// </param>
  /// <param name="credential">
  /// A <see cref="ITapoCredentialProvider"/> that provides the credentials required for authentication.
  /// </param>
  /// <param name="exceptionHandler">
  /// A <see cref="TapoClientExceptionHandler"/> that determines the handling of the exception thrown by the <see cref="TapoClient"/>.
  /// </param>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/>.
  /// </param>
  /// <exception cref="InvalidOperationException">No service for type <see cref="ITapoCredentialProvider"/> and/or <see cref="IDeviceEndPointFactory{PhysicalAddress}"/> has been registered for <paramref name="serviceProvider"/>.</exception>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="deviceEndPointProvider"/> is <see langword="null"/>, or both <paramref name="credential"/> and <paramref name="serviceProvider"/> are <see langword="null"/>.
  /// </exception>
  protected TapoDevice(
    IDeviceEndPointProvider deviceEndPointProvider,
    ITapoCredentialProvider? credential = null,
    TapoClientExceptionHandler? exceptionHandler = null,
    IServiceProvider? serviceProvider = null
  )
  {
    this.deviceEndPointProvider = deviceEndPointProvider ?? throw new ArgumentNullException(nameof(deviceEndPointProvider));
    this.credential = credential
      ?? (serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider))).GetRequiredService<ITapoCredentialProvider>();
    this.exceptionHandler = exceptionHandler
      ?? serviceProvider?.GetService<TapoClientExceptionHandler>()
      ?? TapoClientExceptionHandler.Default;
    this.serviceProvider = serviceProvider;

    TerminalUuidString = Guid.NewGuid().ToString("D", provider: null); // TODO: support DI
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
      httpClientFactory: serviceProvider?.GetService<IHttpClientFactory>(),
      logger: serviceProvider?.GetService<ILoggerFactory>()?.CreateLogger(GenerateLoggerCategoryName())
    );

    string GenerateLoggerCategoryName()
      => deviceEndPointProvider is IDynamicDeviceEndPointProvider
        ? $"{nameof(TapoClient)}({endPoint}, {deviceEndPointProvider})"
        : $"{nameof(TapoClient)}({endPoint})";

    if (client.Session is not null)
      return;

    cancellationToken.ThrowIfCancellationRequested();

    try {
      await client.AuthenticateAsync(
        identity: this,
        credential: credential,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);
    }
    catch {
      client.Dispose();
      client = null;

      throw;
    }
  }

  protected ValueTask SendRequestAsync<TRequest, TResponse>(
    TRequest request,
    CancellationToken cancellationToken = default
  )
    where TRequest : ITapoPassThroughRequest
    where TResponse : ITapoPassThroughResponse
  {
    if (request is null)
      throw new ArgumentNullException(nameof(request));

    ThrowIfDisposed();

    return SendRequestAsyncCore(request, cancellationToken);

    async ValueTask SendRequestAsyncCore(TRequest req, CancellationToken ct)
      => await SendRequestAsync<TRequest, TResponse, None /* as an alternative to System.Void */>(
        request: req,
        composeResult: static _ => default,
        cancellationToken: ct
      ).ConfigureAwait(false);
  }

  protected ValueTask<TResult> SendRequestAsync<TRequest, TResponse, TResult>(
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

  private async ValueTask<TResult> SendRequestAsyncCore<TRequest, TResponse, TResult>(
    TRequest request,
    Func<TResponse, TResult> composeResult,
    CancellationToken cancellationToken = default
  )
    where TRequest : ITapoPassThroughRequest
    where TResponse : ITapoPassThroughResponse
  {
    const int maxAttempts = 5;
    var delay = TimeSpan.Zero;

    for (var attempt = 0; attempt < maxAttempts; attempt++) {
      if (TimeSpan.Zero < delay)
        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

      await EnsureSessionEstablishedAsync(cancellationToken).ConfigureAwait(false);

      cancellationToken.ThrowIfCancellationRequested();

      client.Timeout = Timeout;

      try {
        var response = await client.SendRequestAsync<TRequest, TResponse>(
          request: request,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        return composeResult(response);
      }
      catch (Exception ex) {
        // OperationCanceledException and TaskCanceledException due to a cancel
        // request triggered by a given CancellationToken must not be handled
        // by exception handler, just rethrow instead.
        var handling = (
          ex is OperationCanceledException exOperationCanceled &&
          exOperationCanceled.CancellationToken.Equals(cancellationToken)
        )
          ? TapoClientExceptionHandling.Throw
          : exceptionHandler.DetermineHandling(this, ex, attempt, client.Logger);

        static void LogRequest(ILogger logger, TRequest req)
          => logger.LogError(JsonSerializer.Serialize(req));

        client.Logger?.LogTrace(
          "Exception handling for {TypeOfException}: {ExceptionHandling}",
          ex.GetType().FullName,
          handling
        );

        if (client.Logger is not null && !handling.ShouldRetry)
          LogRequest(client.Logger, request);

        if (handling.ShouldInvalidateEndPoint) {
          if (deviceEndPointProvider is IDynamicDeviceEndPointProvider dynamicEndPoint) {
            // mark end point as invalid to have the end point refreshed or rescanned
            dynamicEndPoint.InvalidateEndPoint();
          }
          else {
            // disallow retry
            handling = handling with { ShouldRetry = false };
          }
        }

        if (handling.ShouldRetry) {
          /*
           * retry
           */
          delay = handling.RetryAfter;

          if (handling.ShouldReconnect) {
            client.Dispose();
            client = null;
          }

          continue;
        }

        /*
         * rethrow
         */
        var endPointUri = client.EndPointUri;

        client.Dispose();
        client = null;

        if (handling.ShouldWrapIntoTapoProtocolException) {
          if (
            ex is TaskCanceledException exTaskCanceled &&
            exTaskCanceled.InnerException is TimeoutException exInnerTimeout
          ) {
            throw new TapoProtocolException(
              message: $"Request timed out; {ex.Message}",
              endPoint: endPointUri,
              innerException: exInnerTimeout
            );
          }
          else {
            throw new TapoProtocolException(
              message: "Unhandled exception",
              endPoint: endPointUri,
              innerException: ex
            );
          }
        }

        throw;
      } // try
    } // for

#if SYSTEM_DIAGNOSTICS_UNREACHABLEEXCEPTION
    throw new UnreachableException();
#else
    throw new NotImplementedException("unreachable");
#endif
  }

  public ValueTask<TapoDeviceInfo> GetDeviceInfoAsync(
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync<GetDeviceInfoRequest, GetDeviceInfoResponse, TapoDeviceInfo>(
      request: default,
      composeResult: static resp => resp.Result,
      cancellationToken: cancellationToken
    );

  public ValueTask SetDeviceInfoAsync<TParameters>(
    TParameters parameters,
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync<SetDeviceInfoRequest<TParameters>, SetDeviceInfoResponse>(
      request: new(
        terminalUuid: TerminalUuidString,
        parameters: parameters
      ),
      cancellationToken: cancellationToken
    );
}
