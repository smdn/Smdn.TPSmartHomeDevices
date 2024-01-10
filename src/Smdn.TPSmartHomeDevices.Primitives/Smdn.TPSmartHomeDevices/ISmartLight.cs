// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

/// <summary>
/// Provides a mechanism for abstracting the smart light without the color toning capability, including smart bulb, smart strip, etc. and its functionality.
/// </summary>
public interface ISmartLight : ISmartDevice {
  /// <summary>
  /// Turns the light on and sets the light brightness.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///   If the device does not support gradual state transitions, the parameter <paramref name="transitionPeriod"/> is ignored and the new state is set immediately.
  ///   </para>
  ///   <para>
  ///   The device cannot be turned off by setting the <paramref name="brightness"/> to <c>0</c>.
  ///   Use <see cref="ISmartDevice.SetOnOffStateAsync(bool, CancellationToken)"/> to turn off the device.
  ///   </para>
  /// </remarks>
  /// <param name="brightness">
  /// The brightness in percent value, in range of 1~100[%].
  /// </param>
  /// <param name="transitionPeriod">
  /// The <see cref="TimeSpan" /> value that indicates the time interval between completion of gradual state transition.
  /// If the value is <see cref="TimeSpan.Zero"/>, the state transition will be performed immediately rather than gradual change.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  /// <exception cref="ArgumentOutOfRangeException">
  ///   <list type="bullet">
  ///     <item><description><paramref name="brightness"/> is less than 1 or greater than 100.</description></item>
  ///     <item><description><paramref name="transitionPeriod"/> is less than <see cref="TimeSpan.Zero"/>.</description></item>
  ///   </list>
  /// </exception>
  ValueTask SetBrightnessAsync(
    int brightness,
    TimeSpan transitionPeriod,
    CancellationToken cancellationToken
  );
}
