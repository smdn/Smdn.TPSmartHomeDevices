// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

public static class TapoDeviceEndPointProvider {
  public static IDeviceEndPointProvider Create(string host)
    => DeviceEndPointProvider.Create(host, TapoClient.DefaultPort);

  public static IDeviceEndPointProvider Create(IPAddress ipAddress)
    => DeviceEndPointProvider.Create(ipAddress, TapoClient.DefaultPort);

  public static IDeviceEndPointProvider Create(
    PhysicalAddress macAddress,
    IServiceProvider serviceProvider
  )
    => DeviceEndPointProvider.Create(macAddress, TapoClient.DefaultPort, serviceProvider);

  public static IDeviceEndPointProvider Create(
    PhysicalAddress macAddress,
    IDeviceEndPointFactory<PhysicalAddress> endPointFactory
  )
    => DeviceEndPointProvider.Create(macAddress, TapoClient.DefaultPort, endPointFactory);
}
