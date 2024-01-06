// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable IDE0045 // 'if' statement can be simplified

using System;

namespace Smdn.TPSmartHomeDevices.Tapo;

internal static partial class ColorModelUtils {
  /// <summary>
  /// Converts a color that is represented by color temperature to the equivalent color in the HSV model.
  /// </summary>
  public static (int Hue, int Saturation, byte Value) ConvertColorTemperatureToHsv(int colorTemperatureInKelvin)
  {
    var (r, g, b) = ConvertColorTemperatureToRgb(colorTemperatureInKelvin);

    var (h, s, v) = ConvertRgbToConicalObjectModelHsv(r / 255.0f, g / 255.0f, b / 255.0f);

    return (
      Math.Clamp((int)(h * 360.0f), 0, 359),
      Math.Clamp((int)(s * 100.0f), 0, 100),
      (byte)(v * byte.MaxValue)
    );
  }

  private static (
    float H,
    float S,
    float V
  )
  ConvertRgbToConicalObjectModelHsv(
    float r,
    float g,
    float b
  )
  {
    if (r < 0.0f || 1.0f < r)
      throw new ArgumentOutOfRangeException(nameof(r));
    if (g < 0.0f || 1.0f < g)
      throw new ArgumentOutOfRangeException(nameof(g));
    if (b < 0.0f || 1.0f < b)
      throw new ArgumentOutOfRangeException(nameof(b));

    var min = MathF.Min(r, MathF.Min(g, b));
    var max = MathF.Max(r, MathF.Max(g, b));
    var v = max;
    var d = max - min;
    // var s = d / max; // 円柱モデル
    var s = d; // 円錐モデル

    if (min == max)
      return (0.0f, s, v);

    float h;

    if (min == b) {
      h = (1.0f / 6.0f * (g - r) / d) + (1.0f / 6.0f);
    }
    else if (min == r) {
      h = (1.0f / 6.0f * (b - g) / d) + (3.0f / 6.0f);
    }
    else { // if (min == b)
      h = (1.0f / 6.0f * (r - b) / d) + (5.0f / 6.0f);
    }

    if (1.0f <= h)
      h -= 1.0f;
    else if (h < 0.0f)
      h += 1.0f;

    return (h, s, v);
  }
}
