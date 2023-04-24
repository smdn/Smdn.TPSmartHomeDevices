// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;
using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo;

public class P105 : TapoDevice {
  /// <summary>
  /// Initializes a new instance of the <see cref="P105"/> class with specifying the device endpoint by host name.
  /// </summary>
  /// <inheritdoc cref="TapoDevice(string, string, string, IServiceProvider?)" />
  public P105(
    string host,
    string email,
    string password,
    IServiceProvider? serviceProvider = null
  )
    : base(
      host: host,
      email: email,
      password: password,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="P105"/> class with specifying the device endpoint by host name.
  /// </summary>
  /// <inheritdoc cref="TapoDevice(string, IServiceProvider)" />
  public P105(
    string host,
    IServiceProvider serviceProvider
  )
    : base(
      host: host,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="P105"/> class with specifying the device endpoint by IP address.
  /// </summary>
  /// <inheritdoc cref="TapoDevice(string, string, string, IServiceProvider?)" />
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

  /// <summary>
  /// Initializes a new instance of the <see cref="P105"/> class with specifying the device endpoint by IP address.
  /// </summary>
  /// <inheritdoc cref="TapoDevice(IPAddress, IServiceProvider)" />
  public P105(
    IPAddress ipAddress,
    IServiceProvider serviceProvider
  )
    : base(
      ipAddress: ipAddress,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="P105"/> class with specifying the device endpoint by MAC address.
  /// </summary>
  /// <inheritdoc cref="TapoDevice(PhysicalAddress, string, string, IServiceProvider?)" />
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

  /// <summary>
  /// Initializes a new instance of the <see cref="P105"/> class with specifying the device endpoint by MAC address.
  /// </summary>
  /// <inheritdoc cref="TapoDevice(PhysicalAddress, IServiceProvider)" />
  public P105(
    PhysicalAddress macAddress,
    IServiceProvider serviceProvider
  )
    : base(
      macAddress: macAddress,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="P105"/> class.
  /// </summary>
  /// <inheritdoc
  ///   cref="TapoDevice(IDeviceEndPoint, ITapoCredentialProvider?, Protocol.TapoClientExceptionHandler?, IServiceProvider?)"
  ///   path="/exception | /param[@name='deviceEndPoint' or @name='credential' or @name='serviceProvider']"
  /// />
  public P105(
    IDeviceEndPoint deviceEndPoint,
    ITapoCredentialProvider? credential = null,
    IServiceProvider? serviceProvider = null
  )
    : base(
      deviceEndPoint: deviceEndPoint,
      credential: credential,
      serviceProvider: serviceProvider
    )
  {
  }

  public static P105 Create<TAddress>(
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
