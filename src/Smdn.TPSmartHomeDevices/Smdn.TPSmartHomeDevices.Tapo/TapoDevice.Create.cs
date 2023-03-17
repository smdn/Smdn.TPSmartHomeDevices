// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;

namespace Smdn.TPSmartHomeDevices.Tapo;

#pragma warning disable IDE0040
partial class TapoDevice {
#pragma warning restore IDE0040
  public static TapoDevice Create(
    IPAddress ipAddress,
    string email,
    string password,
    IServiceProvider? serviceProvider = null
  )
    => new(
      ipAddress: ipAddress,
      email: email,
      password: password,
      serviceProvider: serviceProvider
    );

  public static TapoDevice Create(
    string hostName,
    string email,
    string password,
    IServiceProvider? serviceProvider = null
  )
    => new(
      hostName: hostName,
      email: email,
      password: password,
      serviceProvider: serviceProvider
    );

  public static TapoDevice Create(
    PhysicalAddress macAddress,
    string email,
    string password,
    IDeviceEndPointFactory<PhysicalAddress> endPointFactory,
    IServiceProvider? serviceProvider = null
  )
    => new(
      macAddress: macAddress,
      email: email,
      password: password,
      endPointFactory: endPointFactory,
      serviceProvider: serviceProvider
    );

  /// <summary>
  /// Creates a new instance of the <see cref="TapoDevice"/> class with a MAC address.
  /// </summary>
  /// <param name="macAddress">
  /// A <see cref="PhysicalAddress"/> that holds the MAC address representing the device end point.
  /// </param>
  /// <param name="email">
  /// A <see cref="string"/> that holds the e-mail address of the Tapo account used for authentication.
  /// </param>
  /// <param name="password">
  /// A <see cref="string"/> that holds the password of the Tapo account used for authentication.
  /// </param>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/>.
  /// <see cref="IDeviceEndPointFactory&lt;PhysicalAddress&gt;"/> must be registered to create an end point from the <paramref name="macAddress"/>.
  /// </param>
  /// <exception cref="InvalidOperationException">No service for type <see cref="IDeviceEndPointFactory&lt;PhysicalAddress&gt;"/> has been registered for <see cref="serviceProvider"/>.</exception>
  public static TapoDevice Create(
    PhysicalAddress macAddress,
    string email,
    string password,
    IServiceProvider serviceProvider
  )
    => new(
      macAddress: macAddress,
      email: email,
      password: password,
      serviceProvider: serviceProvider
    );

  public static TapoDevice Create(
    IDeviceEndPointProvider deviceEndPointProvider,
    ITapoCredentialProvider? credentialProvider = null,
    IServiceProvider? serviceProvider = null
  )
    => new(
      deviceEndPointProvider: deviceEndPointProvider,
      credentialProvider: credentialProvider,
      serviceProvider: serviceProvider
    );
}
