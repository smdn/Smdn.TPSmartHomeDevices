using System.Net.NetworkInformation;
using Smdn.Net.AddressResolution;
using Smdn.TPSmartHomeDevices;
using Smdn.TPSmartHomeDevices.Tapo;

var macAddressEndPointFactory = new MacAddressDeviceEndPointFactory(
  new MacAddressResolverOptions() {
    NmapTargetSpecification = "192.0.2.0-255"
  }
);

var deviceMacAddress = PhysicalAddress.Parse("00:00:5E:00:53:00");

var bulb = new L530(macAddressEndPointFactory.Create(deviceMacAddress));

Console.WriteLine("EndPoint: {0}", await bulb.ResolveEndPointAsync());

await bulb.TurnOnAsync();

await Task.Delay(2000);

await bulb.TurnOffAsync();
