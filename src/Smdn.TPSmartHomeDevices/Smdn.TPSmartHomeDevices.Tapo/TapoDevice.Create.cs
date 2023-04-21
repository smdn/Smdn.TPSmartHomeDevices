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
  ///   cref="TapoDevice(IDeviceEndPoint, ITapoCredentialProvider?, Protocol.TapoClientExceptionHandler?, IServiceProvider?)"
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
}
