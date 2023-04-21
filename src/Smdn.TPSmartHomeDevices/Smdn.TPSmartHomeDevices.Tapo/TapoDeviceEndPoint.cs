// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

public static class TapoDeviceEndPoint {
  public static IDeviceEndPoint Create(string host)
    => DeviceEndPoint.Create(host, TapoClient.DefaultPort);

  public static IDeviceEndPoint Create(IPAddress ipAddress)
    => DeviceEndPoint.Create(ipAddress, TapoClient.DefaultPort);

  public static IDeviceEndPoint Create(
    PhysicalAddress macAddress,
    IServiceProvider serviceProvider
  )
    => DeviceEndPoint.Create(macAddress, TapoClient.DefaultPort, serviceProvider);

  public static IDeviceEndPoint Create(
    PhysicalAddress macAddress,
    IDeviceEndPointFactory<PhysicalAddress> endPointFactory
  )
    => DeviceEndPoint.Create(macAddress, TapoClient.DefaultPort, endPointFactory);
}
