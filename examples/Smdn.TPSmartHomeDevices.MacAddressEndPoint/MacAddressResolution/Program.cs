// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net.NetworkInformation;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Net;
using Smdn.TPSmartHomeDevices;
using Smdn.TPSmartHomeDevices.Tapo;

var services = new ServiceCollection();

services.AddDeviceEndPointFactory(
  new MacAddressDeviceEndPointFactory(
    networkProfile: IPNetworkProfile.Create()
  )
);

using var bulb = new L530(
  macAddress: PhysicalAddress.Parse("00:00:5E:00:53:00"),
  email: "user@mail.test",
  password: "password",
  serviceProvider: services.BuildServiceProvider()
);

Console.WriteLine("EndPoint: {0}", await bulb.ResolveEndPointAsync());

await bulb.TurnOnAsync();

await Task.Delay(2000);

await bulb.TurnOffAsync();
