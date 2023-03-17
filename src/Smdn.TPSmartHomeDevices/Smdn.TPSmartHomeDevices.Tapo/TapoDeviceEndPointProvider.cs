// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

public static class TapoDeviceEndPointProvider {
  public static IDeviceEndPointProvider Create(string hostName)
    => DeviceEndPointProvider.Create(hostName, TapoClient.DefaultPort);

  public static IDeviceEndPointProvider Create(IPAddress ipAddress)
    => DeviceEndPointProvider.Create(ipAddress, TapoClient.DefaultPort);
}
