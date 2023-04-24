// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using System.Net.NetworkInformation;
using Smdn.TPSmartHomeDevices.Kasa.Protocol;

namespace Smdn.TPSmartHomeDevices.Kasa;

public static class KasaDeviceEndPoint {
  public static IDeviceEndPoint Create(string host)
    => DeviceEndPoint.Create(host, KasaClient.DefaultPort);

  public static IDeviceEndPoint Create(IPAddress ipAddress)
    => DeviceEndPoint.Create(ipAddress, KasaClient.DefaultPort);

  public static IDeviceEndPoint Create(
    PhysicalAddress macAddress,
    IDeviceEndPointFactory<PhysicalAddress> endPointFactory
  )
    => DeviceEndPoint.Create(macAddress, endPointFactory);

  public static IDeviceEndPoint Create<TAddress>(
    TAddress address,
    IDeviceEndPointFactory<TAddress> endPointFactory
  ) where TAddress : notnull
    => DeviceEndPoint.Create(address, endPointFactory);
}
