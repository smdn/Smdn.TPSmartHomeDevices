// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using System.Net.NetworkInformation;

namespace Smdn.TPSmartHomeDevices;

internal class StaticMacAddressDeviceEndPointFactory : IDeviceEndPointFactory<PhysicalAddress> {
  private IPAddress ResolvedIPAddress { get; }

  public StaticMacAddressDeviceEndPointFactory(IPAddress resolvedIPAddress)
  {
    ResolvedIPAddress = resolvedIPAddress;
  }

  public IDeviceEndPoint Create(PhysicalAddress address)
    => new DynamicDeviceEndPoint(new IPEndPoint(ResolvedIPAddress, 0));
}
