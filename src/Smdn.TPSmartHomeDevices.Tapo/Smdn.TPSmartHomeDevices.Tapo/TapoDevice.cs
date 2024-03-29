// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
#if SYSTEM_DIAGNOSTICS_UNREACHABLEEXCEPTION
using System.Diagnostics;
#endif
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
using System.Diagnostics.CodeAnalysis;
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

/// <summary>
/// Provides abstract APIs to operate Tapo smarthome devices.
/// </summary>
/// <remarks>
/// This is an unofficial API that has no affiliation with TP-Link.
/// This API is released under the <see href="https://opensource.org/license/mit/">MIT License</see>, and as stated in the terms of the MIT License,
/// there is no warranty for the results of using this API and no responsibility is taken for those results.
/// </remarks>
public partial class TapoDevice : ITapoCredentialIdentity, ISmartDevice, IDisposable {
  private readonly struct LoggerScopeEndPointState {
    public EndPoint CurrentEndPoint { get; }
    public IDeviceEndPoint DeviceEndPoint { get; }

    public LoggerScopeEndPointState(EndPoint currentEndPoint, IDeviceEndPoint deviceEndPoint)
    {
      CurrentEndPoint = currentEndPoint;
      DeviceEndPoint = deviceEndPoint;
    }

    public override string? ToString()
      => DeviceEndPoint is StaticDeviceEndPoint
        ? $"{DeviceEndPoint}"
        : $"{CurrentEndPoint} ({DeviceEndPoint})";
  }

  private IDeviceEndPoint deviceEndPoint; // if null, it indicates a 'disposed' state.

  /// <summary>
  /// Gets the <see cref="IDeviceEndPoint"/> representing a endpoint of smart device,
  /// which will be used to resolve the actual endpoint used to communicate with smart devices.
  /// </summary>
  /// <exception cref="ObjectDisposedException">The device has been disposed.</exception>
  public IDeviceEndPoint EndPoint {
    get {
      ThrowIfDisposed();

      return deviceEndPoint;
    }
  }

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
  [MemberNotNullWhen(false, nameof(deviceEndPoint))]
#endif
  protected bool IsDisposed => deviceEndPoint is null;

  private readonly ITapoCredentialProvider credential;
  private readonly TapoDeviceExceptionHandler exceptionHandler;
  private readonly IServiceProvider? serviceProvider;
  public string TerminalUuidString { get; } // must be in the format of 00000000-0000-0000-0000-000000000000

  private TapoClient? client;

  /// <summary>
  /// Gets a current session information represented by <see cref="TapoSession" />.
  /// </summary>
  /// <value>
  /// The <see cref="TapoSession" />. <see langword="null"/> if the session for this instance has not been established or been disposed.
  /// </value>
  public TapoSession? Session => client?.Session;

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
    IServiceProvider? serviceProvider
  )
    : this(
      deviceEndPoint: DeviceEndPoint.Create(host),
      credential: TapoCredentials.CreateProviderFromPlainText(email, password),
      exceptionHandler: null,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="TapoDevice"/> class with specifying the device endpoint by host name.
  /// </summary>
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
      deviceEndPoint: DeviceEndPoint.Create(host),
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
  /// <param name="email">
  /// A <see cref="string"/> representing the e-mail address used for authentication to control the device.
  /// </param>
  /// <param name="password">
  /// A <see cref="string"/> representing the password used for authentication to control the device.
  /// </param>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/>.
  /// </param>
  protected TapoDevice(
    IPAddress ipAddress,
    string email,
    string password,
    IServiceProvider? serviceProvider
  )
    : this(
      deviceEndPoint: DeviceEndPoint.Create(ipAddress),
      credential: TapoCredentials.CreateProviderFromPlainText(email, password),
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
      deviceEndPoint: DeviceEndPoint.Create(ipAddress),
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
  /// <param name="email">
  /// A <see cref="string"/> representing the e-mail address used for authentication to control the device.
  /// </param>
  /// <param name="password">
  /// A <see cref="string"/> representing the password used for authentication to control the device.
  /// </param>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/>.
  /// <see cref="IDeviceEndPointFactory{PhysicalAddress}"/> must be registered to create an end point from the <paramref name="macAddress"/>.
  /// </param>
  /// <exception cref="InvalidOperationException">No service for type <see cref="IDeviceEndPointFactory{PhysicalAddress}"/> has been registered for <paramref name="serviceProvider"/>.</exception>
  protected TapoDevice(
    PhysicalAddress macAddress,
    string email,
    string password,
    IServiceProvider serviceProvider
  )
    : this(
      deviceEndPoint: DeviceEndPoint.Create(
        macAddress ?? throw new ArgumentNullException(nameof(macAddress)),
        serviceProvider.GetDeviceEndPointFactory<PhysicalAddress>()
      ),
      credential: TapoCredentials.CreateProviderFromPlainText(email, password),
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
  /// <see cref="ITapoCredentialProvider"/> must be registered in order to retrieve the credentials required for authentication.
  /// <see cref="IDeviceEndPointFactory&lt;PhysicalAddress&gt;"/> must also be registered to create an <see cref="IDeviceEndPoint" />, corresponding to the <paramref name="macAddress"/>.
  /// </param>
  /// <exception cref="InvalidOperationException">No service for type <see cref="ITapoCredentialProvider"/> and/or <see cref="IDeviceEndPointFactory{PhysicalAddress}"/> has been registered for <paramref name="serviceProvider"/>.</exception>
  protected TapoDevice(
    PhysicalAddress macAddress,
    IServiceProvider serviceProvider
  )
    : this(
      deviceEndPoint: DeviceEndPoint.Create(
        macAddress ?? throw new ArgumentNullException(nameof(macAddress)),
        serviceProvider.GetDeviceEndPointFactory<PhysicalAddress>()
      ),
      credential: null,
      exceptionHandler: null,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="TapoDevice"/> class.
  /// </summary>
  /// <param name="deviceEndPoint">
  /// A <see cref="IDeviceEndPoint"/> that provides the device end point.
  /// </param>
  /// <param name="credential">
  /// A <see cref="ITapoCredentialProvider"/> that provides the credentials required for authentication.
  /// </param>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/>.
  /// </param>
  /// <exception cref="InvalidOperationException">No service for type <see cref="ITapoCredentialProvider"/> has been registered for <paramref name="serviceProvider"/>.</exception>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="deviceEndPoint"/> is <see langword="null"/>, or both <paramref name="credential"/> and <paramref name="serviceProvider"/> are <see langword="null"/>.
  /// </exception>
  protected TapoDevice(
    IDeviceEndPoint deviceEndPoint,
    ITapoCredentialProvider? credential,
    IServiceProvider? serviceProvider
  )
    : this(
      deviceEndPoint: deviceEndPoint,
      credential: credential,
      exceptionHandler: null,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="TapoDevice"/> class.
  /// </summary>
  /// <param name="deviceEndPoint">
  /// A <see cref="IDeviceEndPoint"/> that provides the device end point.
  /// </param>
  /// <param name="credential">
  /// A <see cref="ITapoCredentialProvider"/> that provides the credentials required for authentication.
  /// </param>
  /// <param name="exceptionHandler">
  /// A <see cref="TapoDeviceExceptionHandler"/> that determines the handling of the exception thrown by the <see cref="TapoClient"/>.
  /// </param>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/>.
  /// </param>
  /// <exception cref="InvalidOperationException">No service for type <see cref="ITapoCredentialProvider"/> has been registered for <paramref name="serviceProvider"/>.</exception>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="deviceEndPoint"/> is <see langword="null"/>, or both <paramref name="credential"/> and <paramref name="serviceProvider"/> are <see langword="null"/>.
  /// </exception>
  protected TapoDevice(
    IDeviceEndPoint deviceEndPoint,
    ITapoCredentialProvider? credential,
    TapoDeviceExceptionHandler? exceptionHandler,
    IServiceProvider? serviceProvider
  )
  {
    this.deviceEndPoint = deviceEndPoint ?? throw new ArgumentNullException(nameof(deviceEndPoint));
    this.credential = credential
      ?? (serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider))).GetRequiredService<ITapoCredentialProvider>();
    this.exceptionHandler = exceptionHandler
      ?? serviceProvider?.GetService<TapoDeviceExceptionHandler>()
      ?? TapoDeviceExceptionHandler.Default;
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

    deviceEndPoint = null!; // mark as disposed

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

    return deviceEndPoint.ResolveOrThrowAsync(
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
      using var loggerScopeCurrentEndPoint = client.Logger?.BeginScope(new LoggerScopeEndPointState(client.EndPoint, deviceEndPoint));

      client.Logger?.LogInformation(
        "Endpoint has changed: {CurrentEndPoint} -> {NewEndPoint}",
        client.EndPoint,
        endPoint
      );

      client.Dispose();
      client = null;
    }

    if (client is null) {
      var logger = serviceProvider?.GetService<ILoggerFactory>()?.CreateLogger<TapoClient>();

      using var loggerScopeNewClient = logger?.BeginScope(new LoggerScopeEndPointState(endPoint, deviceEndPoint));

      client = new TapoClient(
        endPoint: endPoint,
        httpClientFactory: serviceProvider?.GetService<IHttpClientFactory>(),
        logger: logger
      );
    }

    if (client.Session is not null)
      return;

    // select TapoSessionProtocol for this device
    var protocol = (
      serviceProvider?.GetService<TapoSessionProtocolSelector>() ??
      TapoSessionProtocolSelector.Default
    ).SelectProtocol(this);

    cancellationToken.ThrowIfCancellationRequested();

    using var loggerScopeAuthentication = client.Logger?.BeginScope(new LoggerScopeEndPointState(client.EndPoint, deviceEndPoint));

    try {
      client.Logger?.LogTrace(
        "Initiate authentication with protocol '{Protocol}'",
        protocol
      );

      if (protocol == null) {
        await client.AuthenticateAsync(
          identity: this,
          credential: credential,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
      }
      else {
        await client.AuthenticateAsync(
          protocol: protocol.Value,
          identity: this,
          credential: credential,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
      }
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
    where TRequest : notnull, ITapoPassThroughRequest
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
    where TRequest : notnull, ITapoPassThroughRequest
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
    where TRequest : notnull, ITapoPassThroughRequest
    where TResponse : ITapoPassThroughResponse
  {
    const int MaxAttempts = 5;
    var delay = TimeSpan.Zero;

    for (var attempt = 0; attempt < MaxAttempts; attempt++) {
      if (TimeSpan.Zero < delay)
        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

      await EnsureSessionEstablishedAsync(cancellationToken).ConfigureAwait(false);

#if DEBUG
      if (client is null)
        throw new InvalidOperationException($"{nameof(client)} is null");
#endif

      cancellationToken.ThrowIfCancellationRequested();

      client!.Timeout = Timeout;

      using var loggerScopeSendRequest = client.Logger?.BeginScope(new LoggerScopeEndPointState(client.EndPoint, deviceEndPoint));

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
          ? TapoDeviceExceptionHandling.Throw
          : exceptionHandler.DetermineHandling(this, ex, attempt, client.Logger);

        static void LogRequest(ILogger logger, TRequest req)
          => logger.LogError("{Request}", JsonSerializer.Serialize(req));

        client.Logger?.LogTrace(
          "Exception handling for {TypeOfException}: {ExceptionHandling}",
          ex.GetType().FullName,
          handling
        );

        if (client.Logger is not null && !handling.ShouldRetry)
          LogRequest(client.Logger, request);

        if (handling.ShouldInvalidateEndPoint) {
          if (deviceEndPoint is IDynamicDeviceEndPoint dynamicEndPoint) {
            // mark end point as invalid to have the end point refreshed or rescanned
            dynamicEndPoint.Invalidate();

            client.Logger?.LogInformation("Marked end point {EndPoint} as invalid.", dynamicEndPoint);
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

          if (handling.ShouldReestablishSession) {
            client.Logger?.LogInformation("Closing the current session and will retry after {DelayMilliseconds} ms.", delay.TotalMilliseconds);

            client.Dispose();
            client = null;
          }
          else {
            client.Logger?.LogInformation("Will retry after {DelayMilliseconds} ms.", delay.TotalMilliseconds);
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

  /// <summary>
  /// Gets the Tapo device information.
  /// </summary>
  /// <remarks>
  /// This overload deserializes the obtained JSON response of device information into the type <typeparamref name="TDeviceInfo"/>.
  /// See <see cref="TapoDeviceInfo"/> for the information about the property names included in the JSON response.
  /// </remarks>
  /// <typeparam name="TDeviceInfo">A type to be deserialized from the obtained JSON response.</typeparam>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{TResult}"/> representing the result of method.
  /// </returns>
  /// <seealso cref="TapoDeviceInfo"/>
  public ValueTask<TDeviceInfo> GetDeviceInfoAsync<TDeviceInfo>(
    CancellationToken cancellationToken = default
  )
    => GetDeviceInfoAsync<TDeviceInfo, TDeviceInfo>(
      composeResult: static resp => resp,
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Gets the Tapo device information.
  /// </summary>
  /// <remarks>
  /// This overload deserializes the obtained JSON response of device information into the type <typeparamref name="TDeviceInfo"/>.
  /// This overload also composes or converts the value of deserialized <typeparamref name="TDeviceInfo"/>
  /// into <typeparamref name="TResult"/> using the specified <see cref="Func{TDeviceInfo, TResult}"/> delegate.
  /// See <see cref="TapoDeviceInfo"/> for the information about the property names included in the JSON response.
  /// </remarks>
  /// <typeparam name="TDeviceInfo">A type to be deserialized from the obtained JSON response.</typeparam>
  /// <typeparam name="TResult">A type to be composed or converted from the value of <typeparamref name="TDeviceInfo"/>.</typeparam>
  /// <param name="composeResult">
  /// The <see cref="Func{TDeviceInfo, TResult}"/> delegate that composes or converts a deserialized <typeparamref name="TDeviceInfo"/> value into the type <typeparamref name="TResult"/>.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{TResult}"/> representing the result of method.
  /// </returns>
  /// <seealso cref="TapoDeviceInfo"/>
  public ValueTask<TResult> GetDeviceInfoAsync<TDeviceInfo, TResult>(
    Func<TDeviceInfo, TResult> composeResult,
    CancellationToken cancellationToken = default
  )
  {
    if (composeResult is null)
      throw new ArgumentNullException(nameof(composeResult));

    return SendRequestAsync<GetDeviceInfoRequest, PassThroughResponse<TDeviceInfo>, TResult>(
      request: default,
      composeResult: result => composeResult(result.Result),
      cancellationToken: cancellationToken
    );
  }

  /// <summary>
  /// Sets the Tapo device information.
  /// </summary>
  /// <remarks>
  /// This method serializes the values represented by <typeparamref name="TDeviceInfo"/> and specifies
  /// them as parameters in a JSON request that sets the device information.
  /// See <see cref="TapoDeviceInfo"/> for the information about the property names included in the JSON request.
  /// </remarks>
  /// <typeparam name="TDeviceInfo">A type to be serialized to the parameters of JSON request.</typeparam>
  /// <param name="deviceInfo">
  /// The values to be set to the device.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  /// <seealso cref="TapoDeviceInfo"/>
  public ValueTask SetDeviceInfoAsync<TDeviceInfo>(
    TDeviceInfo deviceInfo,
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync<SetDeviceInfoRequest<TDeviceInfo>, PassThroughResponse<None>>(
      request: new(
        terminalUuid: TerminalUuidString,
        parameters: deviceInfo
      ),
      cancellationToken: cancellationToken
    );

  /// <summary>Returns a string that represents the current object.</summary>
  /// <returns>A string that represents the device type name and end point.</returns>
  public override string? ToString()
    => $"{GetType().Name} ({deviceEndPoint?.ToString() ?? "disposed"})";
}
