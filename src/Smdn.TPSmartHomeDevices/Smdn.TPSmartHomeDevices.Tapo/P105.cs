// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;

namespace Smdn.TPSmartHomeDevices.Tapo;

public class P105 : TapoDevice {
  public P105(
    string hostName,
    string email,
    string password,
    IServiceProvider? serviceProvider = null
  )
    : base(
      hostName: hostName,
      email: email,
      password: password,
      serviceProvider: serviceProvider
    )
  {
  }

  public P105(
    string hostName,
    IServiceProvider? serviceProvider = null
  )
    : base(
      hostName: hostName,
      serviceProvider: serviceProvider
    )
  {
  }

  public P105(
    IPAddress ipAddress,
    string email,
    string password,
    IServiceProvider? serviceProvider = null
  )
    : base(
      ipAddress: ipAddress,
      email: email,
      password: password,
      serviceProvider: serviceProvider
    )
  {
  }

  public P105(
    PhysicalAddress macAddress,
    string email,
    string password,
    IDeviceEndPointFactory<PhysicalAddress> endPointFactory,
    IServiceProvider? serviceProvider = null
  )
    : base(
      macAddress: macAddress,
      email: email,
      password: password,
      endPointFactory: endPointFactory,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="P105"/> class with a MAC address.
  /// </summary>
  /// <inheritdoc cref="TapoDevice" />
  public P105(
    PhysicalAddress macAddress,
    string email,
    string password,
    IServiceProvider serviceProvider
  )
    : base(
      macAddress: macAddress,
      email: email,
      password: password,
      serviceProvider: serviceProvider
    )
  {
  }

  public P105(
    IDeviceEndPointProvider deviceEndPointProvider,
    ITapoCredentialProvider? credentialProvider = null,
    IServiceProvider? serviceProvider = null
  )
    : base(
      deviceEndPointProvider: deviceEndPointProvider,
      credentialProvider: credentialProvider,
      serviceProvider: serviceProvider
    )
  {
  }
}
