// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;

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
    IDeviceEndPointProvider deviceEndPointProvider,
    Guid? terminalUuid = null,
    ITapoCredentialProvider? credentialProvider = null,
    IServiceProvider? serviceProvider = null
  )
    : base(
      deviceEndPointProvider: deviceEndPointProvider,
      terminalUuid: terminalUuid,
      credentialProvider: credentialProvider,
      serviceProvider: serviceProvider
    )
  {
  }
}
