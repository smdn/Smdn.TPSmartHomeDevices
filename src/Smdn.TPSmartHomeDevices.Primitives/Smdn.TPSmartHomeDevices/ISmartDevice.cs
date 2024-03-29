// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

/// <summary>
/// Provides a mechanism for abstracting the smart device and its functionality.
/// </summary>
public interface ISmartDevice {
  /// <summary>
  /// Gets the <see cref="IDeviceEndPoint"/> representing a endpoint of smart device,
  /// which will be used to resolve the actual endpoint used to communicate with smart devices.
  /// </summary>
  IDeviceEndPoint EndPoint { get; }

  /// <summary>
  /// Sets the on/off state of the device according to the parameter <paramref name="newOnOffState" />.
  /// </summary>
  /// <param name="newOnOffState">
  /// The value that indicates new on/off state to be set. <see langword="true"/> for on, otherwise off.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  ValueTask SetOnOffStateAsync(
    bool newOnOffState,
    CancellationToken cancellationToken
  );

#if NET6_0_OR_GREATER // default interface methods; .NET Core 3.x + C# 8.0
  /// <summary>
  /// Turns on the device.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  ValueTask TurnOnAsync(CancellationToken cancellationToken = default)
    => SetOnOffStateAsync(newOnOffState: true, cancellationToken);

  /// <summary>
  /// Turns off the device.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  ValueTask TurnOffAsync(CancellationToken cancellationToken = default)
    => SetOnOffStateAsync(newOnOffState: false, cancellationToken);
#endif

  /// <summary>
  /// Gets the on/off state of the device.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  ValueTask<bool> GetOnOffStateAsync(
    CancellationToken cancellationToken
  );

  /// <summary>
  /// Gets the device information.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{IDeviceInfo}"/> representing the result of method.
  /// </returns>
  /// <seealso cref="IDeviceInfo"/>
  ValueTask<IDeviceInfo> GetDeviceInfoAsync(
    CancellationToken cancellationToken = default
  );
}
