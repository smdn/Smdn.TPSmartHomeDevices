// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using Smdn.TPSmartHomeDevices.Kasa.Protocol;

namespace Smdn.TPSmartHomeDevices.Kasa;

public static class KasaDeviceEndPointProvider {
  public static IDeviceEndPointProvider Create(string hostName)
    => DeviceEndPointProvider.Create(hostName, KasaClient.DefaultPort);

  public static IDeviceEndPointProvider Create(IPAddress ipAddress)
    => DeviceEndPointProvider.Create(ipAddress, KasaClient.DefaultPort);
}
