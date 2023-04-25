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

  /// <summary>
  /// Initializes a new instance of the <see cref="KasaDevice"/> class.
  /// </summary>
  /// <typeparam name="TAddress">The type that represents an address of device endpoint.</typeparam>
  /// <param name="deviceAddress">
  /// A <typeparamref name="TAddress"/> that provides the device end point.
  /// </param>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/>.
  /// <see cref="IDeviceEndPointFactory{PhysicalAddress}"/> must be registered to create an end point from the <paramref name="deviceAddress"/>.
  /// </param>
  /// <exception cref="InvalidOperationException">
  /// No service for type <see cref="IDeviceEndPointFactory{TAddress}"/> has been registered for <paramref name="serviceProvider"/>.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="deviceAddress"/> is <see langword="null"/>.
  /// Or <paramref name="serviceProvider"/> is <see langword="null"/>.
  /// </exception>
  public static KasaDevice Create<TAddress>(
    TAddress deviceAddress,
    IServiceProvider serviceProvider
  ) where TAddress : notnull
    => new(
      deviceEndPoint: DeviceEndPoint.Create(
        address: deviceAddress,
        serviceProvider.GetDeviceEndPointFactory<TAddress>()
      ),
      serviceProvider: serviceProvider
    );
}
