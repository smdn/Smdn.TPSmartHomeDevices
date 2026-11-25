// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net.NetworkInformation;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Net;
using Smdn.TPSmartHomeDevices;
using Smdn.TPSmartHomeDevices.Tapo;

var services = new ServiceCollection();

services.AddDeviceEndPointFactory(
  new MacAddressDeviceEndPointFactory(
    // Select and use the network of the interface with ID 'wlan0'
    IPNetworkProfile.CreateFromNetworkInterface(id: "wlan0")
  )
);

var serviceProvider = services.BuildServiceProvider();

using var bulb = new L530(
  PhysicalAddress.Parse("00:00:5E:00:53:00"),
  "user@mail.test",
  "password",
  serviceProvider
);

try {
  await bulb.SetColorTemperatureAsync(colorTemperature: 5500, brightness: 80);

  await bulb.TurnOffAsync();
}
catch (DeviceEndPointResolutionException ex) {
  Console.Error.WriteLine(ex);
}
