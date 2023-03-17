using System.Net.NetworkInformation;

namespace Smdn.TPSmartHomeDevices;

internal class NullMacAddressDeviceEndPointFactory : IDeviceEndPointFactory<PhysicalAddress> {
  public IDeviceEndPointProvider Create(PhysicalAddress address, int port = 0)
    => new UnresolvedDeviceEndPointProvider();
}
