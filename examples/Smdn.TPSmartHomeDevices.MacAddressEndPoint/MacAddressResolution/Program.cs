// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using System.Net.NetworkInformation;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Net;
using Smdn.TPSmartHomeDevices;
using Smdn.TPSmartHomeDevices.Tapo;

var services = new ServiceCollection();

// By adding the 'Smdn.TPSmartHomeDevices.MacAddressEndPoint' to the PackageReference,
// you can specify devices by MAC address instead of IP address or host name.
//
// Adding a MacAddressDeviceEndPointFactory with the AddDeviceEndPointFactory method
// enables MAC address endpoint resolution.
services.AddDeviceEndPointFactory(
  new MacAddressDeviceEndPointFactory(
    networkProfile: IPNetworkProfile.Create()
  )
);

// The IPNetworkProfile class can be used to specify the network
// to be used for address resolution.

// You can create an IPNetworkProfile that uses a specific network interface as follows
var networkProfileUnix = IPNetworkProfile.CreateFromNetworkInterface("wlan0"); // for Unix-like OS
var networkProfileWin = IPNetworkProfile.CreateFromNetworkInterface(Guid.Parse("00000000-0000-0000-0000-000000000000")); // for Windows OS

// Or you can create an IPNetworkProfile that targets the specific address range.
var dhcpNetworkProfile = IPNetworkProfile.Create(
  // Use the address range of 192.168.2.100-192.168.2.119
  addressRangeGenerator: () => Enumerable.Range(100, 20).Select(b => new IPAddress(new byte[] { 192, 168, 2, (byte)b }))
);

// For more information and example about the IPNetworkProfile,
// see https://github.com/smdn/Smdn.Net.AddressResolution/

var servicesForDhcpNetwork = new ServiceCollection();

servicesForDhcpNetwork.AddDeviceEndPointFactory(
  // When a MacAddressDeviceEndPointFactory is created with an IPNetworkProfile,
  // address resolution will be performed for the network that IPNetworkProfile describes.
  new MacAddressDeviceEndPointFactory(dhcpNetworkProfile)
);

using var bulb = new L530(
  // Specify the device's MAC address using the PhysicalAddress class.
  // MAC addresses can be used for Tapo devices as well as Kasa devices.
  macAddress: PhysicalAddress.Parse("00:00:5E:00:53:00"),
  email: "user@mail.test",
  password: "password",
  serviceProvider: servicesForDhcpNetwork.BuildServiceProvider()
);

// Subsequent authentication and sending of requests to the device will
// target the endpoint resolved from the specified MAC address.
try {
  await bulb.TurnOnAsync();
}
catch (DeviceEndPointResolutionException) {
  // If the endpoint cannot be resolved, a DeviceEndPointResolutionException will be thrown.
  Console.Error.WriteLine("Could not resolve the device endpoint");
  return;
}

// If you want to get the resolved endpoint, you can use the ResolveEndPointAsync method.
Console.WriteLine("EndPoint: {0}", await bulb.ResolveEndPointAsync());
