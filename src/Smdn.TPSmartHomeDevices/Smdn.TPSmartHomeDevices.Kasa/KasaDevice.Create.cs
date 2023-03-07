// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;

namespace Smdn.TPSmartHomeDevices.Kasa;

#pragma warning disable IDE0040
partial class KasaDevice {
#pragma warning restore IDE0040
  public static KasaDevice Create(
    IPAddress deviceAddress,
    IServiceProvider? serviceProvider = null
  )
    => new(
      ipAddress: deviceAddress ?? throw new ArgumentNullException(nameof(deviceAddress)),
      serviceProvider: serviceProvider
    );

  public static KasaDevice Create(
    string hostName,
    IServiceProvider? serviceProvider = null
  )
    => new(
      hostName: hostName ?? throw new ArgumentNullException(nameof(hostName)),
      serviceProvider: serviceProvider
    );

  public static KasaDevice Create(
    IDeviceEndPointProvider deviceEndPointProvider,
    IServiceProvider? serviceProvider = null
  )
    => new(
      deviceEndPointProvider: deviceEndPointProvider ?? throw new ArgumentNullException(nameof(deviceEndPointProvider)),
      serviceProvider: serviceProvider
    );
}
