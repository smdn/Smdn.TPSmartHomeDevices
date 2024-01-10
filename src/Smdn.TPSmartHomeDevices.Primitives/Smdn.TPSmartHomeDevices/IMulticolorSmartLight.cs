// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

/// <summary>
/// Provides a mechanism for abstracting the multicolor smart light including smart bulb, smart strip, etc. that can be specified in any one color and its functionality.
/// </summary>
public interface IMulticolorSmartLight : ISmartLight {
  /// <summary>
  /// Turns the light on and sets the light color to the specified color temperature.
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
  /// <param name="colorTemperature">
  /// The color temperature in kelvin [K].
  /// Available color temperatures depend on the device model.
  /// </param>
  /// <param name="brightness">
  /// The brightness in percent value, in range of 1~100[%].
  /// If <see langword="null"/>, the current brightness will be kept.
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
  ///     <item><description><paramref name="colorTemperature"/> is out of the range that can be specified for the device.</description></item>
  ///     <item><description><paramref name="brightness"/> is less than 1 or greater than 100.</description></item>
  ///     <item><description><paramref name="transitionPeriod"/> is less than <see cref="TimeSpan.Zero"/>.</description></item>
  ///   </list>
  /// </exception>
  ValueTask SetColorTemperatureAsync(
    int colorTemperature,
    int? brightness,
    TimeSpan transitionPeriod,
    CancellationToken cancellationToken
  );

  /// <summary>
  /// Turns the light on and sets the light color to the specified color represented by <paramref name="hue"/> and <paramref name="saturation"/>.
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
  /// <param name="hue">
  /// The hue of the color in degree, in range of 0~360[°].
  /// </param>
  /// <param name="saturation">
  /// The saturation of the color in percent value, in range of 0~100[%].
  /// </param>
  /// <param name="brightness">
  /// The brightness in percent value, in range of 1~100[%].
  /// If <see langword="null"/>, the current brightness will be kept.
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
  ///     <item><description><paramref name="hue"/> is less than 0 or greater than 360.</description></item>
  ///     <item><description><paramref name="saturation"/> is less than 0 or greater than 360.</description></item>
  ///     <item><description><paramref name="brightness"/> is less than 1 or greater than 100.</description></item>
  ///     <item><description><paramref name="transitionPeriod"/> is less than <see cref="TimeSpan.Zero"/>.</description></item>
  ///   </list>
  /// </exception>
  ValueTask SetColorAsync(
    int hue,
    int saturation,
    int? brightness,
    TimeSpan transitionPeriod,
    CancellationToken cancellationToken
  );

#if NET6_0_OR_GREATER // default interface methods; .NET Core 3.x + C# 8.0
  /// <summary>
  /// Sets the on/off state of the light according to the parameter <paramref name="newOnOffState" />.
  /// </summary>
  /// <param name="newOnOffState">
  /// The value that indicates new on/off state to be set. <see langword="true"/> for on, otherwise off.
  /// </param>
  /// <param name="transitionPeriod">
  /// The <see cref="TimeSpan" /> value that indicates the time interval between completion of gradual state transition.
  /// If the value is <see cref="TimeSpan.Zero"/>, the state transition will be performed immediately rather than gradual change.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  ValueTask SetOnOffStateAsync(
    bool newOnOffState,
    TimeSpan transitionPeriod,
    CancellationToken cancellationToken
  )
    // ignores the value of transitionPeriod
    => SetOnOffStateAsync(
      newOnOffState: newOnOffState,
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Turns on the light.
  /// </summary>
  /// <param name="transitionPeriod">
  /// The <see cref="TimeSpan" /> value that indicates the time interval between completion of gradual state transition.
  /// If the value is <see cref="TimeSpan.Zero"/>, the state transition will be performed immediately rather than gradual change.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  ValueTask TurnOnAsync(
    TimeSpan transitionPeriod,
    CancellationToken cancellationToken
  )
    => SetOnOffStateAsync(
      newOnOffState: true,
      transitionPeriod: transitionPeriod,
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Turns off the light.
  /// </summary>
  /// <param name="transitionPeriod">
  /// The <see cref="TimeSpan" /> value that indicates the time interval between completion of gradual state transition.
  /// If the value is <see cref="TimeSpan.Zero"/>, the state transition will be performed immediately rather than gradual change.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  ValueTask TurnOffAsync(
    TimeSpan transitionPeriod,
    CancellationToken cancellationToken
  )
    => SetOnOffStateAsync(
      newOnOffState: false,
      transitionPeriod: transitionPeriod,
      cancellationToken: cancellationToken
    );
#endif
}
