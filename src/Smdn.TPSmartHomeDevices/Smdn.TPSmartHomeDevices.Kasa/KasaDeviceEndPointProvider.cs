// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;
using Smdn.TPSmartHomeDevices.Kasa.Protocol;

namespace Smdn.TPSmartHomeDevices.Kasa;

public static class KasaDeviceEndPointProvider {
  public static IDeviceEndPointProvider Create(string hostName)
    => DeviceEndPointProvider.Create(hostName, KasaClient.DefaultPort);

  public static IDeviceEndPointProvider Create(IPAddress ipAddress)
    => DeviceEndPointProvider.Create(ipAddress, KasaClient.DefaultPort);

  public static IDeviceEndPointProvider Create(
    PhysicalAddress macAddress,
    IServiceProvider serviceProvider
  )
    => DeviceEndPointProvider.Create(macAddress, KasaClient.DefaultPort, serviceProvider);

  public static IDeviceEndPointProvider Create(
    PhysicalAddress macAddress,
    IDeviceEndPointFactory<PhysicalAddress> endPointFactory
  )
    => DeviceEndPointProvider.Create(macAddress, KasaClient.DefaultPort, endPointFactory);
}
