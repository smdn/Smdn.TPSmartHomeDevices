using System.Net.NetworkInformation;

namespace Smdn.TPSmartHomeDevices;

internal class NullMacAddressDeviceEndPointFactory : IDeviceEndPointFactory<PhysicalAddress> {
  public IDeviceEndPoint Create(PhysicalAddress address)
    => new UnresolvedDeviceEndPoint();
}
