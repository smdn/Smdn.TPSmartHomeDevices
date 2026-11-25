// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1873

using System;
using System.Buffers;
#if SYSTEM_DIAGNOSTICS_UNREACHABLEEXCEPTION
using System.Diagnostics;
#endif
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE || SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE || SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Smdn.TPSmartHomeDevices.Kasa.Protocol;

namespace Smdn.TPSmartHomeDevices.Kasa;

/// <summary>
/// Provides abstract APIs to operate Kasa smarthome devices.
/// </summary>
/// <remarks>
/// This is an unofficial API that has no affiliation with TP-Link.
/// This API is released under the <see href="https://opensource.org/license/mit/">MIT License</see>, and as stated in the terms of the MIT License,
/// there is no warranty for the results of using this API and no responsibility is taken for those results.
/// </remarks>
public partial class KasaDevice : IDisposable {
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

  protected internal readonly struct NullParameter { }

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

  private readonly KasaDeviceExceptionHandler exceptionHandler;
  private readonly ArrayBufferWriter<byte> buffer = new(initialCapacity: KasaClient.DefaultBufferCapacity);
  private readonly IServiceProvider? serviceProvider;

  private KasaClient? client;

  public bool IsConnected {
    get {
      ThrowIfDisposed();
      return client is not null && client.IsConnected;
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="KasaDevice"/> class with specifying the device endpoint by host name.
  /// </summary>
  /// <param name="host">
  /// A <see cref="string"/> that holds the host name or IP address string, representing the device endpoint.
  /// </param>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/>.
  /// </param>
  protected KasaDevice(
    string host,
    IServiceProvider? serviceProvider
  )
    : this(
      deviceEndPoint: DeviceEndPoint.Create(host),
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="KasaDevice"/> class with specifying the device endpoint by IP address.
  /// </summary>
  /// <param name="ipAddress">
  /// A <see cref="IPAddress"/> that holds the IP address representing the device end point.
  /// </param>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/>.
  /// </param>
  protected KasaDevice(
    IPAddress ipAddress,
    IServiceProvider? serviceProvider
  )
    : this(
      deviceEndPoint: DeviceEndPoint.Create(ipAddress),
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="KasaDevice"/> class with specifying the device endpoint by MAC address.
  /// </summary>
  /// <param name="macAddress">
  /// A <see cref="PhysicalAddress"/> that holds the MAC address representing the device end point.
  /// </param>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/>.
  /// <see cref="IDeviceEndPointFactory{PhysicalAddress}"/> must be registered to create an end point from the <paramref name="macAddress"/>.
  /// </param>
  /// <exception cref="InvalidOperationException">No service for type <see cref="IDeviceEndPointFactory{PhysicalAddress}"/> has been registered for <paramref name="serviceProvider"/>.</exception>
  protected KasaDevice(
    PhysicalAddress macAddress,
    IServiceProvider serviceProvider
  )
    : this(
      deviceEndPoint: DeviceEndPoint.Create(
        macAddress ?? throw new ArgumentNullException(nameof(macAddress)),
        serviceProvider.GetDeviceEndPointFactory<PhysicalAddress>()
      ),
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="KasaDevice"/> class.
  /// </summary>
  /// <param name="deviceEndPoint">
  /// A <see cref="IDeviceEndPoint"/> that provides the device end point.
  /// </param>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/>.
  /// </param>
  /// <exception cref="InvalidOperationException">No service for type <see cref="IDeviceEndPointFactory{PhysicalAddress}"/> has been registered for <paramref name="serviceProvider"/>.</exception>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="deviceEndPoint"/> is <see langword="null"/>.
  /// </exception>
  protected KasaDevice(
    IDeviceEndPoint deviceEndPoint,
    IServiceProvider? serviceProvider
  )
  {
    this.deviceEndPoint = deviceEndPoint ?? throw new ArgumentNullException(nameof(deviceEndPoint));
    this.serviceProvider = serviceProvider;

    exceptionHandler = serviceProvider?.GetService<KasaDeviceExceptionHandler>() ?? KasaDeviceExceptionHandler.Default;
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
      defaultPort: KasaClient.DefaultPort,
      cancellationToken: cancellationToken
    );
  }

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE
  [RequiresDynamicCode(FeatureRequirementMessages.RequiresDynamicCodeJsonSerializeInMethodOverload)]
  [RequiresUnreferencedCode(FeatureRequirementMessages.RequiresUnreferencedCodeJsonSerializeInMethodOverload)]
#endif
  [Obsolete(ObsoleteMessages.RequiresReflectionBasedJsonSerializeInMethodOverload)]
  protected ValueTask SendRequestAsync<TMethodParameter>(
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
#if SYSTEM_THREADING_TASKS_VALUETASK_FROMCANCELED
      ? ValueTask.FromCanceled(cancellationToken)
#else
      ? ValueTaskShim.FromCanceled(cancellationToken)
#endif
      : SendRequestAsyncCore(
          module: module,
          method: method,
          parameters: parameters,
          parametersJsonTypeInfo: null,
          cancellationToken: cancellationToken
        );
  }

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_UNCONDITIONALSUPPRESSMESSAGEATTRIBUTE
  [UnconditionalSuppressMessage(category: "AOT", checkId: "IL3050", Justification = UnconditionalSuppressMessages.IL3050JustificationAotCompatibleCodePathUsingJsonTypeInfo)]
  [UnconditionalSuppressMessage(category: "Trimming", checkId: "IL2026", Justification = UnconditionalSuppressMessages.IL2026JustificationReflectionFreeCodePathUsingJsonTypeInfo)]
#endif
  protected ValueTask SendRequestAsync<TMethodParameter>(
    JsonEncodedText module,
    JsonEncodedText method,
    TMethodParameter parameters,
    JsonTypeInfo<TMethodParameter> parametersJsonTypeInfo,
    CancellationToken cancellationToken
  )
  {
    if (parameters is null)
      throw new ArgumentNullException(nameof(parameters));
    if (parametersJsonTypeInfo is null)
      throw new ArgumentNullException(nameof(parametersJsonTypeInfo));

    ThrowIfDisposed();

    return cancellationToken.IsCancellationRequested
#if SYSTEM_THREADING_TASKS_VALUETASK_FROMCANCELED
      ? ValueTask.FromCanceled(cancellationToken)
#else
      ? ValueTaskShim.FromCanceled(cancellationToken)
#endif
      : SendRequestAsyncCore(
          module: module,
          method: method,
          parameters: parameters,
          parametersJsonTypeInfo: parametersJsonTypeInfo,
          cancellationToken: cancellationToken
        );
  }

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_UNCONDITIONALSUPPRESSMESSAGEATTRIBUTE
  [UnconditionalSuppressMessage(category: "AOT", checkId: "IL3050", Justification = UnconditionalSuppressMessages.IL3050JustificationAotCompatibleCodePathUsingJsonTypeInfo)]
  [UnconditionalSuppressMessage(category: "Trimming", checkId: "IL2026", Justification = UnconditionalSuppressMessages.IL2026JustificationReflectionFreeCodePathUsingJsonTypeInfo)]
#endif
  protected ValueTask<TMethodResult> SendRequestAsync<TMethodResult>(
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
#if SYSTEM_THREADING_TASKS_VALUETASK_FROMCANCELED
      ? ValueTask.FromCanceled<TMethodResult>(cancellationToken)
#else
      ? ValueTaskShim.FromCanceled<TMethodResult>(cancellationToken)
#endif
      : SendRequestAsyncCore(
          module: module,
          method: method,
          parameters: default,
          parametersJsonTypeInfo: KasaDeviceJsonSerializerContext.Default.KasaDeviceNullParameter,
          composeResult: composeResult,
          cancellationToken: cancellationToken
        );
  }

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE
  [RequiresDynamicCode(FeatureRequirementMessages.RequiresDynamicCodeJsonSerializeInMethodOverload)]
  [RequiresUnreferencedCode(FeatureRequirementMessages.RequiresUnreferencedCodeJsonSerializeInMethodOverload)]
#endif
  [Obsolete(ObsoleteMessages.RequiresReflectionBasedJsonSerializeInMethodOverload)]
  protected ValueTask<TMethodResult> SendRequestAsync<TMethodParameter, TMethodResult>(
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
#if SYSTEM_THREADING_TASKS_VALUETASK_FROMCANCELED
      ? ValueTask.FromCanceled<TMethodResult>(cancellationToken)
#else
      ? ValueTaskShim.FromCanceled<TMethodResult>(cancellationToken)
#endif
      : SendRequestAsyncCore(
          module: module,
          method: method,
          parameters: parameters,
          parametersJsonTypeInfo: null,
          composeResult: composeResult,
          cancellationToken: cancellationToken
        );
  }

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE
  [RequiresDynamicCode(FeatureRequirementMessages.RequiresDynamicCodeConditionalJsonSerializeInMethodOverload)]
  [RequiresUnreferencedCode(FeatureRequirementMessages.RequiresUnreferencedCodeConditionalJsonSerializeInMethodOverload)]
#endif
  private async ValueTask SendRequestAsyncCore<TMethodParameter>(
    JsonEncodedText module,
    JsonEncodedText method,
    TMethodParameter parameters,
    JsonTypeInfo<TMethodParameter>? parametersJsonTypeInfo,
    CancellationToken cancellationToken
  )
    => await SendRequestAsyncCore<TMethodParameter, None /* as an alternative to System.Void */>(
      module: module,
      method: method,
      parameters: parameters,
      parametersJsonTypeInfo: parametersJsonTypeInfo,
      composeResult: static _ => default,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
  [MemberNotNull(nameof(client))]
#endif
  private void EnsureClientCreated(
    EndPoint endPoint,
    CancellationToken cancellationToken
  )
  {
    if (client is not null)
      return;

    cancellationToken.ThrowIfCancellationRequested();

    var logger = serviceProvider?.GetService<ILoggerFactory>()?.CreateLogger<KasaClient>();

    using var loggerScope = logger?.BeginScope(new LoggerScopeEndPointState(endPoint, deviceEndPoint));

    client = new KasaClient(
      endPoint: endPoint,
      buffer: buffer,
      logger: logger
    );
  }

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE
  [RequiresDynamicCode(FeatureRequirementMessages.RequiresDynamicCodeConditionalJsonSerializeInMethodOverload)]
  [RequiresUnreferencedCode(FeatureRequirementMessages.RequiresUnreferencedCodeConditionalJsonSerializeInMethodOverload)]
#endif
  private async ValueTask<TMethodResult> SendRequestAsyncCore<TMethodParameter, TMethodResult>(
    JsonEncodedText module,
    JsonEncodedText method,
    TMethodParameter parameters,
    JsonTypeInfo<TMethodParameter>? parametersJsonTypeInfo,
    Func<JsonElement, TMethodResult> composeResult,
    CancellationToken cancellationToken
  )
  {
    void DisposeClientIfEndPointHasChanged(EndPoint newEndPoint)
    {
      if (client is null)
        return; // client not created yet
      if (client.EndPoint.Equals(newEndPoint))
        return; // end point has not changed

      // endpoint has changed, recreate client with new endpoint
      using var loggerScope = client.Logger?.BeginScope(new LoggerScopeEndPointState(client.EndPoint, deviceEndPoint));

      client.Logger?.LogInformation(
        "Endpoint has changed: {CurrentEndPoint} -> {NewEndPoint}",
        client.EndPoint,
        newEndPoint
      );

      client.Dispose();
      client = null;
    }

    const int MaxAttempts = 5;
    var delay = TimeSpan.Zero;
    EndPoint? endPoint = null;

    for (var attempt = 0; attempt < MaxAttempts; attempt++) {
      if (TimeSpan.Zero < delay)
        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

      if (endPoint is null) {
        endPoint = await ResolveEndPointAsync(cancellationToken).ConfigureAwait(false);

        DisposeClientIfEndPointHasChanged(endPoint);
      }

      EnsureClientCreated(
        endPoint: endPoint,
        cancellationToken: cancellationToken
      );

      using var loggerScope =
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
        client
#else
        client!
#endif
        .Logger
        ?.BeginScope(new LoggerScopeEndPointState(endPoint, deviceEndPoint));

      try {
        return parametersJsonTypeInfo is null
#pragma warning disable CS0618 // CS0618: SendAsync without JsonTypeInfo<T> is obsolete.
          ? await client.SendAsync(
              module: module,
              method: method,
              parameter: parameters,
              composeResult: composeResult,
              cancellationToken: cancellationToken
            ).ConfigureAwait(false)
#pragma warning restore CS0618
          : await client.SendAsync(
              module: module,
              method: method,
              parameter: parameters,
              parameterJsonTypeInfo: parametersJsonTypeInfo,
              composeResult: composeResult,
              cancellationToken: cancellationToken
            ).ConfigureAwait(false);
      }
      catch (Exception ex) {
#pragma warning disable SA1143
        // OperationCanceledException and TaskCanceledException due to a cancel
        // request triggered by a given CancellationToken must not be handled
        // by exception handler, just rethrow instead.
        var handling = (
          ex is OperationCanceledException exOperationCanceled &&
          exOperationCanceled.CancellationToken.Equals(cancellationToken)
        )
          ? KasaDeviceExceptionHandling.Throw
          : exceptionHandler.DetermineHandling(this, ex, attempt, client.Logger);
#pragma warning restore SA1143

        static void LogRequest(ILogger logger, JsonEncodedText mod, JsonEncodedText meth, TMethodParameter param, JsonTypeInfo<TMethodParameter>? paramJsonTypeInfo)
          => logger.LogError(
            "{{{Module}:{{{Method}:{{{Param}}}}}}}",
            mod,
            meth,
            paramJsonTypeInfo is null
              ? JsonSerializer.Serialize(param)
              : JsonSerializer.Serialize(param, jsonTypeInfo: paramJsonTypeInfo)
          );

        client.Logger?.LogTrace(
          "Exception handling for {TypeOfException}: {ExceptionHandling}",
          ex.GetType().FullName,
          handling
        );

        if (client.Logger is not null && !handling.ShouldRetry)
          LogRequest(client.Logger, module, method, parameters, parametersJsonTypeInfo);

        if (handling.ShouldInvalidateEndPoint) {
          if (deviceEndPoint is IDynamicDeviceEndPoint dynamicEndPoint) {
            // mark end point as invalid to have the end point refreshed or rescanned
            dynamicEndPoint.Invalidate();

            endPoint = null; // should resolve end point

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

          if (handling.ShouldReconnect) {
            client.Logger?.LogInformation("Closing the current connection and will retry after {DelayMilliseconds} ms.", delay.TotalMilliseconds);

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
        client.Dispose();
        client = null;

        throw;
      } // try
    } // for

#if SYSTEM_DIAGNOSTICS_UNREACHABLEEXCEPTION
    throw new UnreachableException();
#else
    throw new NotImplementedException("unreachable");
#endif
  }

  /// <summary>Returns a string that represents the current object.</summary>
  /// <returns>A string that represents the device type name and end point.</returns>
  public override string? ToString()
    => $"{GetType().Name} ({deviceEndPoint?.ToString() ?? "disposed"})";
}
