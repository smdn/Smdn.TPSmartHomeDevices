using System.Net.NetworkInformation;
using Microsoft.Extensions.DependencyInjection;
using Smdn.Net;
using Smdn.TPSmartHomeDevices;
using Smdn.TPSmartHomeDevices.Tapo;

var services = new ServiceCollection();

// By creating a MacAddressDeviceEndPointFactory and adding it with the
// AddDeviceEndPointFactory method, you can enable MAC address endpoint resolution.
// This can be applied to Kasa devices as well as Tapo devices.
services.AddDeviceEndPointFactory(
  new MacAddressDeviceEndPointFactory(
    // Select and use the network of the interface with ID 'wlan0'
    IPNetworkProfile.CreateFromNetworkInterface(id: "wlan0")
  )
);

var serviceProvider = services.BuildServiceProvider();

// Creates device controller for Tapo L530 multicolor light bulb.
using var bulb = new L530(
  PhysicalAddress.Parse("00:00:5E:00:53:00"), // MAC address assigned to L530
  "user@mail.test", // E-mail address for your Tapo account
  "password",       // Password for your Tapo account
  serviceProvider   // IServiceProvider that has a MacAddressDeviceEndPointFactory added
);

try {
  // Turns on the bulb, and set the color temperature and brightness.
  await bulb.SetColorTemperatureAsync(colorTemperature: 5500, brightness: 80);

  // Turns off the bulb
  await bulb.TurnOffAsync();
}
catch (DeviceEndPointResolutionException ex) {
  // If the endpoint cannot be resolved, a DeviceEndPointResolutionException is thrown.
  Console.Error.WriteLine(ex);

  // In this example, address resolution is based on information cached in the
  // system's address table (ARP table).
  // If there is no IP address corresponding to the MAC address in the cache,
  // address resolution will fail.

  // The MacAddressResolver that MacAddressDeviceEndPointFactory uses internally
  // supports network scanning to perform address table updates prior to address resolution.

  // Documents for enabling network scanning with the MacAddressDeviceEndPointFactory
  // will be added in the future.

  // In MacAddressResolver, network scanning can be enabled by setting the
  // properties NetworkScanInterval and NetworkScanMinInterval.
  // See the MacAddressResolver documentation for detail.
  // https://github.com/smdn/Smdn.Net.AddressResolution/blob/main/examples/network-scanning/Program.cs
}
