// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: CC-BY-SA-4.0
#pragma warning disable IDE0045 // 'if' statement can be simplified

using System;

namespace Smdn.TPSmartHomeDevices.Tapo;

#pragma warning disable IDE0040
partial class ColorModelUtils {
#pragma warning restore IDE0040
  /// <summary>
  /// Converts a color that is represented by color temperature to the equivalent color in the RGB model.
  /// </summary>
  /// <remarks>
  /// This implementation is based on and ported from the pseudo code described in the following document by <see href="https://tannerhelland.com/about.html">Tanner Helland</see>:
  /// <see href="https://tannerhelland.com/2012/09/18/convert-temperature-rgb-algorithm-code.html">How to Convert Temperature (K) to RGB: Algorithm and Sample Code</see>, published under the <see href="https://creativecommons.org/licenses/by-sa/4.0/">CC BY-SA 4.0</see>.
  /// </remarks>
  private static (byte R, byte G, byte B) ConvertColorTemperatureToRgb(int colorTemperatureInKelvin)
  {
    const int MinKelvin = 1000;
    const int MaxKelvin = 40000;

    if (colorTemperatureInKelvin is < MinKelvin or > MaxKelvin) {
      throw new ArgumentOutOfRangeException(
        paramName: nameof(colorTemperatureInKelvin),
        actualValue: colorTemperatureInKelvin,
        message: $"The value for color temperature must be in range of {MinKelvin}~{MaxKelvin}"
      );
    }

    byte red, green, blue;

    var temperature = colorTemperatureInKelvin / 100;

    // Calculate Red
    if (temperature <= 66) {
      red = 255;
    }
    else {
      red = (byte)Math.Clamp(
        329.698727446 * Math.Pow(temperature - 60, -0.1332047592),
        0.0,
        255.0
      );
    }

    // Calculate Green
    if (temperature <= 66) {
      green = (byte)Math.Clamp(
        (99.4708025861 * Math.Log(temperature)) - 161.1195681661,
        0.0,
        255.0
      );
    }
    else {
      green = (byte)Math.Clamp(
        288.1221695283 * Math.Pow(temperature - 60, -0.0755148492),
        0.0,
        255.0
      );
    }

    // Calculate Blue
    if (66 <= temperature) {
      blue = 255;
    }
    else if (temperature <= 19) {
      blue = 0;
    }
    else {
      blue = (byte)Math.Clamp(
        (138.5177312231 * Math.Log(temperature - 10)) - 305.0447927307,
        0.0,
        255.0
      );
    }

    return (red, green, blue);
  }
}
