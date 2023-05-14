// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo;

#pragma warning disable IDE0040
partial class TapoDevice {
#pragma warning restore IDE0040
  /// <inheritdoc cref="TapoDevice(string, string, string, IServiceProvider?)"/>
  public static TapoDevice Create(
    string host,
    string email,
    string password,
    IServiceProvider? serviceProvider = null
  )
    => new(
      host: host,
      email: email,
      password: password,
      serviceProvider: serviceProvider
    );

  /// <inheritdoc cref="TapoDevice(string, IServiceProvider)"/>
  public static TapoDevice Create(
    string host,
    IServiceProvider serviceProvider
  )
    => new(
      host: host,
      serviceProvider: serviceProvider
    );

  /// <inheritdoc cref="TapoDevice(IPAddress, string, string, IServiceProvider?)"/>
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

  /// <inheritdoc cref="TapoDevice(IPAddress, IServiceProvider)"/>
  public static TapoDevice Create(
    IPAddress ipAddress,
    IServiceProvider serviceProvider
  )
    => new(
      ipAddress: ipAddress,
      serviceProvider: serviceProvider
    );

  /// <inheritdoc cref="TapoDevice(PhysicalAddress, string, string, IServiceProvider)"/>
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

  /// <inheritdoc cref="TapoDevice(PhysicalAddress, IServiceProvider)"/>
  public static TapoDevice Create(
    PhysicalAddress macAddress,
    IServiceProvider serviceProvider
  )
    => new(
      macAddress: macAddress,
      serviceProvider: serviceProvider
    );

  /// <inheritdoc
  ///   cref="TapoDevice(IDeviceEndPoint, ITapoCredentialProvider?, TapoDeviceExceptionHandler?, IServiceProvider?)"
  ///   path="/summary | /exception | /param[@name='deviceEndPoint' or @name='credential' or @name='serviceProvider']"
  /// />
  public static TapoDevice Create(
    IDeviceEndPoint deviceEndPoint,
    ITapoCredentialProvider? credential = null,
    IServiceProvider? serviceProvider = null
  )
    => new(
      deviceEndPoint: deviceEndPoint,
      credential: credential,
      serviceProvider: serviceProvider
    );

  /// <inheritdoc
  ///   cref="TapoDevice(IDeviceEndPoint, ITapoCredentialProvider?, TapoDeviceExceptionHandler?, IServiceProvider?)"
  ///   path="/summary"
  /// />
  /// <typeparam name="TAddress">The type that represents an address of device endpoint.</typeparam>
  /// <param name="deviceAddress">
  /// A <typeparamref name="TAddress"/> that provides the device end point.
  /// </param>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/>.
  /// <see cref="IDeviceEndPointFactory{PhysicalAddress}"/> must be registered to create an end point from the <paramref name="deviceAddress"/>.
  /// Also <see cref="ITapoCredentialProvider"/> must be registered if the <paramref name="credential"/> is <see langword="null"/>.
  /// </param>
  /// <param name="credential">
  /// A <see cref="ITapoCredentialProvider"/> that provides the credentials required for authentication.
  /// </param>
  /// <exception cref="InvalidOperationException">
  /// No service for type <see cref="IDeviceEndPointFactory{TAddress}"/> has been registered for <paramref name="serviceProvider"/>.
  /// <paramref name="credential"/> is <see langword="null"/> and no service for type <see cref="ITapoCredentialProvider"/> has been registered for <paramref name="serviceProvider"/>.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="deviceAddress"/> is <see langword="null"/>.
  /// Or <paramref name="serviceProvider"/> is <see langword="null"/>.
  /// </exception>
  public static TapoDevice Create<TAddress>(
    TAddress deviceAddress,
    IServiceProvider serviceProvider,
    ITapoCredentialProvider? credential = null
  ) where TAddress : notnull
    => new(
      deviceEndPoint: DeviceEndPoint.Create(
        address: deviceAddress,
        serviceProvider.GetDeviceEndPointFactory<TAddress>()
      ),
      credential: credential,
      serviceProvider: serviceProvider
    );
}
