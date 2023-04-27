// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices;

/// <summary>
/// Provides a mechanism for creating <see cref="IDeviceEndPoint"/> from the address respresented by <typeparamref name="TAddress"/>.
/// </summary>
/// <remarks>
/// <c>MacAddressDeviceEndPointFactory</c> class from the package <c>Smdn.TPSmartHomeDevices.MacAddressEndPoint</c> is one concrete implementation of this interface.
/// </remarks>
/// <typeparam name="TAddress">The type that represents an address of device endpoint.</typeparam>
/// <seealso cref="IDeviceEndPoint" />
public interface IDeviceEndPointFactory<TAddress> where TAddress : notnull {
  /// <summary>
  /// Creates an <see cref="IDeviceEndPoint"/> that is resolved by the address represented by type <typeparamref name="TAddress"/>.
  /// </summary>
  /// <param name="address">The address to identify the device endpoint.</param>
  IDeviceEndPoint Create(TAddress address);
}
