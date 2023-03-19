// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
#if SYSTEM_DIAGNOSTICS_UNREACHABLEEXCEPTION
using System.Diagnostics;
#endif
using System.Net;
using System.Net.NetworkInformation;
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
    IServiceProvider? serviceProvider = null
  )
    : this(
      deviceEndPointProvider: KasaDeviceEndPointProvider.Create(host),
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
    IServiceProvider? serviceProvider = null
  )
    : this(
      deviceEndPointProvider: KasaDeviceEndPointProvider.Create(ipAddress),
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
      deviceEndPointProvider: KasaDeviceEndPointProvider.Create(macAddress, serviceProvider),
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="KasaDevice"/> class.
  /// </summary>
  /// <param name="deviceEndPointProvider">
  /// A <see cref="IDeviceEndPointProvider"/> that provides the device end point.
  /// </param>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/>.
  /// </param>
  /// <exception cref="InvalidOperationException">No service for type <see cref="IDeviceEndPointFactory{PhysicalAddress}"/> has been registered for <paramref name="serviceProvider"/>.</exception>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="deviceEndPointProvider"/> is <see langword="null"/>.
  /// </exception>
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
      ?
#if SYSTEM_THREADING_TASKS_VALUETASK_FROMCANCELED
        ValueTask.FromCanceled(cancellationToken)
#else
        ValueTaskShim.FromCanceled(cancellationToken)
#endif
      : SendRequestAsyncCore(
        module: module,
        method: method,
        parameters: parameters,
        cancellationToken: cancellationToken
      );
  }

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
      ?
#if SYSTEM_THREADING_TASKS_VALUETASK_FROMCANCELED
        ValueTask.FromCanceled<TMethodResult>(cancellationToken)
#else
        ValueTaskShim.FromCanceled<TMethodResult>(cancellationToken)
#endif
      : SendRequestAsyncCore<NullParameter, TMethodResult>(
        module: module,
        method: method,
        parameters: default,
        composeResult: composeResult,
        cancellationToken: cancellationToken
      );
  }

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
      ?
#if SYSTEM_THREADING_TASKS_VALUETASK_FROMCANCELED
        ValueTask.FromCanceled<TMethodResult>(cancellationToken)
#else
        ValueTaskShim.FromCanceled<TMethodResult>(cancellationToken)
#endif
      : SendRequestAsyncCore(
        module: module,
        method: method,
        parameters: parameters,
        composeResult: composeResult,
        cancellationToken: cancellationToken
      );
  }

  private async ValueTask SendRequestAsyncCore<TMethodParameter>(
    JsonEncodedText module,
    JsonEncodedText method,
    TMethodParameter parameters,
    CancellationToken cancellationToken
  )
    => await SendRequestAsyncCore<TMethodParameter, None /* as an alternative to System.Void */>(
      module: module,
      method: method,
      parameters: parameters,
      composeResult: static _ => default,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

  private async ValueTask<TMethodResult> SendRequestAsyncCore<TMethodParameter, TMethodResult>(
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
        logger: serviceProvider?.GetService<ILoggerFactory>()?.CreateLogger($"{nameof(KasaClient)}({endPoint})") // TODO: logger category name
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
