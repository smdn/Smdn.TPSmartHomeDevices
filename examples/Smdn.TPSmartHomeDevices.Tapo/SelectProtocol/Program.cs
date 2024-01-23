// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Smdn.TPSmartHomeDevices.Tapo;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

var services = new ServiceCollection();

// Tapo devices use different protocols depending on the version of
// firmware installed on the device.
//
// Smdn.TPSmartHomeDevices.Tapo automatically selects the protocol by default.
//
// The AddTapoProtocolSelector() method can be used to explicitly specify a protocol.
// This method takes one of the following values:
//
//   TapoSessionProtocol.Klap:
//     Use this if new firmware is installed on the device.
//
//   TapoSessionProtocol.SecurePassThrough:
//     Use this if new firmware is not installed on the device.
//
//   null:
//     First try with the old protocol, and if that fails, use the new protocol.
//     Use this if the version of firmware installed on a device is unknown.
//
services.AddTapoProtocolSelector(TapoSessionProtocol.Klap);

// If you want to select which protocol to use for each Tapo device,
// you can also use the following overload.
services.AddTapoProtocolSelector(
  static device => TapoSessionProtocol.Klap
);

using var bulb = new L530(
  "192.0.2.1",
  "user@mail.test",
  "password",
  services.BuildServiceProvider()
);

// When making a request, the protocol will be selected as specified
// in the AddTapoProtocolSelector method.
await bulb.TurnOnAsync();
