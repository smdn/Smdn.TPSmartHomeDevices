// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;

namespace Smdn.TPSmartHomeDevices;

public static class DeviceEndPoint {
  public static IDeviceEndPoint Create(string host)
    => new StaticDeviceEndPoint(
      new DnsEndPoint(
        host: host ?? throw new ArgumentNullException(nameof(host)),
        port: 0
      )
    );

  public static IDeviceEndPoint Create(IPAddress ipAddress)
    => new StaticDeviceEndPoint(
      new IPEndPoint(
        address: ipAddress ?? throw new ArgumentNullException(nameof(ipAddress)),
        port: 0
      )
    );

  public static IDeviceEndPoint Create(EndPoint endPoint)
    => new StaticDeviceEndPoint(
      endPoint ?? throw new ArgumentNullException(nameof(endPoint))
    );

  public static IDeviceEndPoint Create<TAddress>(
    TAddress address,
    IDeviceEndPointFactory<TAddress> endPointFactory
  ) where TAddress : notnull
    => (endPointFactory ?? throw new ArgumentNullException(nameof(endPointFactory)))
      .Create(address: address ?? throw new ArgumentNullException(nameof(address)));
}
