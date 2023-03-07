// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;

namespace Smdn.TPSmartHomeDevices.Tapo;

#pragma warning disable IDE0040
partial class TapoDevice {
#pragma warning restore IDE0040
  public static TapoDevice Create(
    IPAddress deviceAddress,
    string email,
    string password,
    IServiceProvider? serviceProvider = null
  )
    => Create(
      deviceEndPointProvider: TapoDeviceEndPointProvider.Create(
        ipAddress: deviceAddress ?? throw new ArgumentNullException(nameof(deviceAddress))
      ),
      credentialProvider: new PlainTextCredentialProvider(
        userName: email ?? throw new ArgumentNullException(nameof(email)),
        password: password ?? throw new ArgumentNullException(nameof(password))
      ),
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
    IDeviceEndPointProvider deviceEndPointProvider,
    Guid? terminalUuid = null,
    ITapoCredentialProvider? credentialProvider = null,
    IServiceProvider? serviceProvider = null
  )
    => new(
      deviceEndPointProvider: deviceEndPointProvider,
      terminalUuid: terminalUuid,
      credentialProvider: credentialProvider,
      serviceProvider: serviceProvider
    );
}
