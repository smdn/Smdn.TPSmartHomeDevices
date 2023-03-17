using System.Net;
using System.Net.NetworkInformation;

namespace Smdn.TPSmartHomeDevices;

internal class StaticMacAddressDeviceEndPointFactory : IDeviceEndPointFactory<PhysicalAddress> {
  private IPAddress ResolvedIPAddress { get; }

  public StaticMacAddressDeviceEndPointFactory(IPAddress resolvedIPAddress)
  {
    ResolvedIPAddress = resolvedIPAddress;
  }

  public IDeviceEndPointProvider Create(PhysicalAddress address, int port = 0)
    => new DynamicDeviceEndPointProvider(new IPEndPoint(ResolvedIPAddress, port));
}
