// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

/// <summary>
/// Provides extension methods for <see cref="IDeviceEndPoint"/>.
/// </summary>
/// <seealso cref="IDeviceEndPoint" />
public static class IDeviceEndPointExtensions {
  /// <summary>
  /// Resolves this endpoint to its corresponding <see cref="EndPoint"/>.
  /// If the endpoint could not resolve to the specific <see cref="EndPoint"/>, throw <see cref="DeviceEndPointResolutionException"/>.
  /// </summary>
  /// <remarks>
  /// This method calls the <see cref="IDynamicDeviceEndPoint.Invalidate" /> if the <paramref name="deviceEndPoint"/> is an <see cref="IDynamicDeviceEndPoint"/> and the endpoint cannot be resolved.
  /// </remarks>
  /// <param name="deviceEndPoint">The <see cref="IDeviceEndPoint" />.</param>
  /// <param name="defaultPort">The default port number. If the resolved <see cref="EndPoint"/> does not specify a specific port, set this port number to the resolved <see cref="EndPoint"/>.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
  /// <returns>
  /// A <see cref="ValueTask{EndPoint}"/> representing the result of endpoint resolution.
  /// </returns>
  /// <exception cref="DeviceEndPointResolutionException">The endpoint could not resolve to the specific <see cref="EndPoint"/>.</exception>
  /// <seealso cref="IDeviceEndPoint.ResolveAsync(CancellationToken)" />
  /// <seealso cref="IDynamicDeviceEndPoint.Invalidate" />
  public static ValueTask<EndPoint> ResolveOrThrowAsync(
    this IDeviceEndPoint deviceEndPoint,
    int defaultPort,
    CancellationToken cancellationToken = default
  )
  {
    if (deviceEndPoint is null)
      throw new ArgumentNullException(nameof(deviceEndPoint));
    if (defaultPort < 0)
      throw new ArgumentOutOfRangeException(message: "must be positive number", paramName: nameof(defaultPort));

    if (cancellationToken.IsCancellationRequested)
#if SYSTEM_THREADING_TASKS_VALUETASK_FROMCANCELED
      return ValueTask.FromCanceled<EndPoint>(cancellationToken);
#else
      return ValueTaskShim.FromCanceled<EndPoint>(cancellationToken);
#endif

    return ResolveOrThrowAsyncCore();

    async ValueTask<EndPoint> ResolveOrThrowAsyncCore()
    {
      var endPoint = await deviceEndPoint.ResolveAsync(cancellationToken).ConfigureAwait(false);

      if (endPoint is null) {
        if (deviceEndPoint is IDynamicDeviceEndPoint dynamicDeviceEndPoint)
          dynamicDeviceEndPoint.Invalidate();

        throw new DeviceEndPointResolutionException(deviceEndPoint);
      }

      return endPoint switch {
        IPEndPoint ipEndPoint => ipEndPoint.Port == 0
          ? new IPEndPoint(ipEndPoint.Address, defaultPort)
          : ipEndPoint,

        DnsEndPoint dnsEndPoint => dnsEndPoint.Port == 0
          ? new DnsEndPoint(dnsEndPoint.Host, defaultPort)
          : dnsEndPoint,

        _ => endPoint, // includes case for the type EndPoint
      };
    }
  }
}
