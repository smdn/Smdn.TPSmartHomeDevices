// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices;

/// <summary>
/// Provides a mechanism for creating <see cref="IDeviceEndPoint"/> from the address respresented by <typeparamref name="TAddress"/>.
/// </summary>
/// <typeparam name="TAddress">The type that represents an address of device endpoint.</typeparam>
/// <seealso cref="IDeviceEndPoint" />
/// <seealso cref="MacAddressDeviceEndPointFactory" />
public interface IDeviceEndPointFactory<TAddress> where TAddress : notnull {
  /// <summary>
  /// Creates an <see cref="IDeviceEndPoint"/> that is resolved by the address represented by type <typeparamref name="TAddress"/>.
  /// </summary>
  /// <param name="address">The address to identify the device endpoint.</param>
  IDeviceEndPoint Create(TAddress address);
}
