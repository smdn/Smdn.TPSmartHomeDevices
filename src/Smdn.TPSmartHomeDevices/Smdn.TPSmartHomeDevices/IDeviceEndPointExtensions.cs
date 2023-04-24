// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

public static class IDeviceEndPointExtensions {
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

        /* EndPoint */ _ => endPoint,
      };
    }
  }
}
