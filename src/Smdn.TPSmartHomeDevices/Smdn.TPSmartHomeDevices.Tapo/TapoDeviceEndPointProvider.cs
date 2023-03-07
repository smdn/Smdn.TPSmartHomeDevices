// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;

namespace Smdn.TPSmartHomeDevices.Tapo;

public static class TapoDeviceEndPointProvider {
  private const int DefaultPort = 80; // HTTP

  public static IDeviceEndPointProvider Create(string hostName)
    => DeviceEndPointProvider.Create(hostName, DefaultPort);

  public static IDeviceEndPointProvider Create(IPAddress ipAddress)
    => DeviceEndPointProvider.Create(ipAddress, DefaultPort);
}
