// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Smdn.TPSmartHomeDevices;

internal static class DeviceEndPoint {
  private sealed class StaticDeviceEndPoint : IDeviceEndPoint {
    private readonly EndPoint endPoint;

    public StaticDeviceEndPoint(EndPoint endPoint)
    {
      this.endPoint = endPoint;
    }

    public ValueTask<EndPoint?> ResolveAsync(CancellationToken cancellationToken)
      => cancellationToken.IsCancellationRequested
        ?
#if SYSTEM_THREADING_TASKS_VALUETASK_FROMCANCELED
          ValueTask.FromCanceled<EndPoint?>(cancellationToken)
#else
          ValueTaskShim.FromCanceled<EndPoint?>(cancellationToken)
#endif
        : new(endPoint);

    public override string? ToString()
      => endPoint.ToString();
  }

  public static IDeviceEndPoint Create(string host, int port)
    => Create(
      new DnsEndPoint(
        host: host ?? throw new ArgumentNullException(nameof(host)),
        port: port
      )
    );

  public static IDeviceEndPoint Create(IPAddress ipAddress, int port)
    => Create(
      new IPEndPoint(
        address: ipAddress ?? throw new ArgumentNullException(nameof(ipAddress)),
        port: port
      )
    );

  public static IDeviceEndPoint Create(EndPoint endPoint)
    => new StaticDeviceEndPoint(
      endPoint ?? throw new ArgumentNullException(nameof(endPoint))
    );

  public static IDeviceEndPoint Create(
    PhysicalAddress macAddress,
    int port,
    IServiceProvider serviceProvider
  )
    => Create(
      macAddress: macAddress ?? throw new ArgumentNullException(nameof(macAddress)),
      port: port,
      endPointFactory: (serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider)))
        .GetRequiredService<IDeviceEndPointFactory<PhysicalAddress>>()
    );

  public static IDeviceEndPoint Create(
    PhysicalAddress macAddress,
    int port,
    IDeviceEndPointFactory<PhysicalAddress> endPointFactory
  )
    => (endPointFactory ?? throw new ArgumentNullException(nameof(endPointFactory)))
      .Create(
        address: macAddress ?? throw new ArgumentNullException(nameof(macAddress)),
        port: port
      );

  internal static async ValueTask<EndPoint> ResolveOrThrowAsync(
    this IDeviceEndPoint deviceEndPoint,
    int defaultPort,
    CancellationToken cancellationToken
  )
  {
    var endPoint = await deviceEndPoint.ResolveAsync(cancellationToken);

    if (endPoint is null && deviceEndPoint is IDynamicDeviceEndPoint dynamicDeviceEndPoint)
      dynamicDeviceEndPoint.Invalidate();

    return endPoint switch {
      IPEndPoint ipEndPoint => ipEndPoint.Port == 0
        ? new IPEndPoint(ipEndPoint.Address, defaultPort)
        : ipEndPoint,

      DnsEndPoint dnsEndPoint => dnsEndPoint.Port == 0
        ? new DnsEndPoint(dnsEndPoint.Host, defaultPort)
        : dnsEndPoint,

      EndPoint ep => ep,

      null => throw new DeviceEndPointResolutionException(deviceEndPoint),
    };
  }
}
