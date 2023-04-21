// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;

namespace Smdn.TPSmartHomeDevices.Kasa;

#pragma warning disable IDE0040
partial class KasaDevice {
#pragma warning restore IDE0040
  /// <inheritdoc cref="KasaDevice(string, IServiceProvider?)" />
  public static KasaDevice Create(
    string host,
    IServiceProvider? serviceProvider = null
  )
    => new(
      host: host,
      serviceProvider: serviceProvider
    );

  /// <inheritdoc cref="KasaDevice(IPAddress, IServiceProvider?)" />
  public static KasaDevice Create(
    IPAddress ipAddress,
    IServiceProvider? serviceProvider = null
  )
    => new(
      ipAddress: ipAddress,
      serviceProvider: serviceProvider
    );

  /// <inheritdoc cref="KasaDevice(PhysicalAddress, IServiceProvider)" />
  public static KasaDevice Create(
    PhysicalAddress macAddress,
    IServiceProvider serviceProvider
  )
    => new(
      macAddress: macAddress,
      serviceProvider: serviceProvider
    );

  /// <inheritdoc cref="KasaDevice(IDeviceEndPoint, IServiceProvider?)" />
  public static KasaDevice Create(
    IDeviceEndPoint deviceEndPoint,
    IServiceProvider? serviceProvider = null
  )
    => new(
      deviceEndPoint: deviceEndPoint,
      serviceProvider: serviceProvider
    );
}
