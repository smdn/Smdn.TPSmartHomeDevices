// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices;

/// <summary>
/// Provides a mechanism for abstracting device endpoints and resolving to specific <seealso cref="System.Net.EndPoint" />.
/// This interface represents a 'dynamic' endpoint, such as when the actual IP address is assigned by DHCP.
/// </summary>
/// <remarks>
/// <c>MacAddressDeviceEndPointFactory+MacAddressDeviceEndPoint</c> class from the package <c>Smdn.TPSmartHomeDevices.MacAddressEndPoint</c> is one concrete implementation of this interface.
/// </remarks>
public interface IDynamicDeviceEndPoint : IDeviceEndPoint {
  /// <summary>
  /// Marks the device endpoint as 'invalidated', for example,
  /// if the resolved <seealso cref="System.Net.EndPoint" /> is unreachable or expired.
  /// </summary>
  void Invalidate();
}
