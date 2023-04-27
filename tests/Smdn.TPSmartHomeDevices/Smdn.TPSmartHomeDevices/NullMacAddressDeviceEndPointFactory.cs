// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net.NetworkInformation;

namespace Smdn.TPSmartHomeDevices;

internal class NullMacAddressDeviceEndPointFactory : IDeviceEndPointFactory<PhysicalAddress> {
  public IDeviceEndPoint Create(PhysicalAddress address)
    => new UnresolvedDeviceEndPoint();
}
