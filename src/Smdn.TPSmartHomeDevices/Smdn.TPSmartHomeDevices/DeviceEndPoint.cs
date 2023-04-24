// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;

namespace Smdn.TPSmartHomeDevices;

internal static class DeviceEndPoint {
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
    IDeviceEndPointFactory<PhysicalAddress> endPointFactory
  )
    => Create(
      address: macAddress ?? throw new ArgumentNullException(nameof(macAddress)),
      port: port,
      endPointFactory: endPointFactory ?? throw new ArgumentNullException(nameof(endPointFactory))
    );

  public static IDeviceEndPoint Create<TAddress>(
    TAddress address,
    int port,
    IDeviceEndPointFactory<TAddress> endPointFactory
  ) where TAddress : notnull
    => (endPointFactory ?? throw new ArgumentNullException(nameof(endPointFactory)))
      .Create(
        address: address ?? throw new ArgumentNullException(nameof(address)),
        port: port
      );
}
