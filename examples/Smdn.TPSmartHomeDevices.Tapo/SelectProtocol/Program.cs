// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Smdn.TPSmartHomeDevices.Tapo;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

// Tapo devices use different protocols depending on the version of
// firmware installed on the device.
//
// Smdn.TPSmartHomeDevices.Tapo automatically selects the protocol by default.
//
// The AddTapoProtocolSelector() method can be used to select the
// protocol to be used for each Tapo device explicitly.
var services = new ServiceCollection();

services.AddTapoProtocolSelector(
  // To specify the protocol to use, specify the TapoSessionProtocol enum or null.
  static device => TapoSessionProtocol.Klap

  // TapoSessionProtocol.Klap:
  //   Use this if new firmware is installed on the device.
  //
  // TapoSessionProtocol.SecurePassThrough:
  //   Use this if new firmware is not installed on the device.
  //
  // null:
  //   First try with the old protocol, and if that fails, use the new protocol.
  //   Use this if the version of firmware installed on a device is unknown.
);

using var bulb = new L530(
  "192.0.2.255",
  "user@mail.test",
  "password",
  services.BuildServiceProvider()
);

await bulb.TurnOnAsync();
