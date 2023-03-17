// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;

namespace Smdn.TPSmartHomeDevices.Kasa;

#pragma warning disable IDE0040
partial class KasaDevice {
#pragma warning restore IDE0040
  public static KasaDevice Create(
    IPAddress ipAddress,
    IServiceProvider? serviceProvider = null
  )
    => new(
      ipAddress: ipAddress,
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
    PhysicalAddress macAddress,
    IDeviceEndPointFactory<PhysicalAddress> endPointFactory,
    IServiceProvider? serviceProvider = null
  )
    => new(
      macAddress: macAddress,
      endPointFactory: endPointFactory,
      serviceProvider: serviceProvider
    );

  /// <summary>
  /// Creates a new instance of the <see cref="KasaDevice"/> class with a MAC address.
  /// </summary>
  /// <param name="macAddress">
  /// A <see cref="PhysicalAddress"/> that holds the MAC address representing the device end point.
  /// </param>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/>.
  /// <see cref="IDeviceEndPointFactory&lt;PhysicalAddress&gt;"/> must be registered to create an end point from the <paramref name="macAddress"/>.
  /// </param>
  /// <exception cref="InvalidOperationException">No service for type <see cref="IDeviceEndPointFactory&lt;PhysicalAddress&gt;"/> has been registered for <see cref="serviceProvider"/>.</exception>
  public static KasaDevice Create(
    PhysicalAddress macAddress,
    IServiceProvider serviceProvider
  )
    => new(
      macAddress: macAddress,
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
