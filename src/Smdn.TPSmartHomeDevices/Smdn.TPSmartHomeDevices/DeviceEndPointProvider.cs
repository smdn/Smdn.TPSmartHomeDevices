// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

internal static class DeviceEndPointProvider {
  private sealed class StaticDeviceEndPointProvider : IDeviceEndPointProvider {
    private readonly ValueTask<EndPoint?> staticEndPointValueTaskResult;

    public bool IsStaticEndPoint => true;

    public StaticDeviceEndPointProvider(EndPoint endPoint)
    {
      staticEndPointValueTaskResult = new ValueTask<EndPoint?>(endPoint);
    }

    public ValueTask<EndPoint?> GetEndPointAsync(CancellationToken cancellationToken)
      => cancellationToken.IsCancellationRequested
        ?
#if SYSTEM_THREADING_TASKS_VALUETASK_FROMCANCELED
          ValueTask.FromCanceled<EndPoint?>(cancellationToken)
#else
          ValueTaskShim.FromCanceled<EndPoint?>(cancellationToken)
#endif
        : staticEndPointValueTaskResult;
  }

  public static IDeviceEndPointProvider Create(string hostName, int port)
    => Create(
      new DnsEndPoint(
        host: hostName,
        port: port
      )
    );

  public static IDeviceEndPointProvider Create(IPAddress ipAddress, int port)
    => Create(
      new IPEndPoint(
        address: ipAddress,
        port: port
      )
    );

  public static IDeviceEndPointProvider Create(EndPoint endPoint)
    => new StaticDeviceEndPointProvider(
      endPoint ?? throw new ArgumentNullException(nameof(endPoint))
    );

  internal static async ValueTask<EndPoint> ResolveOrThrowAsync(
    this IDeviceEndPointProvider provider,
    int defaultPort,
    CancellationToken cancellationToken
  )
  {
    var endPoint = await provider.GetEndPointAsync(cancellationToken);

    return endPoint switch {
      IPEndPoint ipEndPoint => ipEndPoint.Port == 0
        ? new IPEndPoint(ipEndPoint.Address, defaultPort)
        : ipEndPoint,

      DnsEndPoint dnsEndPoint => dnsEndPoint.Port == 0
        ? new DnsEndPoint(dnsEndPoint.Host, defaultPort)
        : dnsEndPoint,

      EndPoint ep => ep,

      null => throw new DeviceEndPointResolutionException(provider),
    };
  }
}
