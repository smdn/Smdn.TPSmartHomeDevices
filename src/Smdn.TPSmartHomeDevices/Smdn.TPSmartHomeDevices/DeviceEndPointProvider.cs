// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Smdn.TPSmartHomeDevices;

internal static class DeviceEndPointProvider {
  private sealed class StaticDeviceEndPointProvider : IDeviceEndPointProvider {
    private readonly ValueTask<EndPoint?> staticEndPointValueTaskResult;

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
        host: hostName ?? throw new ArgumentNullException(nameof(hostName)),
        port: port
      )
    );

  public static IDeviceEndPointProvider Create(IPAddress ipAddress, int port)
    => Create(
      new IPEndPoint(
        address: ipAddress ?? throw new ArgumentNullException(nameof(ipAddress)),
        port: port
      )
    );

  public static IDeviceEndPointProvider Create(EndPoint endPoint)
    => new StaticDeviceEndPointProvider(
      endPoint ?? throw new ArgumentNullException(nameof(endPoint))
    );

  public static IDeviceEndPointProvider Create(
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

  public static IDeviceEndPointProvider Create(
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
