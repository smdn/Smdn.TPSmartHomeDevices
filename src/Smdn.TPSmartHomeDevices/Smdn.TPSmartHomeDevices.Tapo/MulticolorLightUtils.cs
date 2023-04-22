// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
#if LANG_VERSION_11_OR_GREATER && NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
using System.Diagnostics.CodeAnalysis;
#endif

namespace Smdn.TPSmartHomeDevices.Tapo;

internal static class MulticolorLightUtils {
  private const int BrightnessMinValue = 1;
  private const int BrightnessMaxValue = 100;

  private const int HueMinValue = 0;
  private const int HueMaxValue = 360;

  private const int SaturationMinValue = 0;
  private const int SaturationMaxValue = 100;

  private static readonly string BrightnessVallueOutOfRangeExceptionMessage =
    $"The value for brightness must be in range of {BrightnessMinValue}~{BrightnessMaxValue}";

  private static readonly string HueValueOutOfRangeExceptionMessage =
    $"The value for hue must be in range of {HueMinValue}~{HueMaxValue}";

  private static readonly string SaturationValueOutOfRangeExceptionMessage =
    $"The value for saturation must be in range of {SaturationMinValue}~{SaturationMaxValue}";

#if false // TODO
  private static readonly string ColorTemperatureValueOutOfRangeExceptionMessage =
    "The value for saturation must be in range of {ColorTemperatureMinValue}~{ColorTemperatureMaxValue}";
#endif

#if LANG_VERSION_11_OR_GREATER && NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
  [return: NotNullIfNotNull(nameof(newValue))]
#endif
  public static int? ValidateBrightnessValue(int? newValue, string paramName)
  {
    if (newValue == null)
      return null;

    if (newValue.Value is < BrightnessMinValue or > BrightnessMaxValue) {
      throw new ArgumentOutOfRangeException(
        paramName: paramName,
        actualValue: newValue.Value,
        message: BrightnessVallueOutOfRangeExceptionMessage
      );
    }

    return newValue;
  }

#if LANG_VERSION_11_OR_GREATER && NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
  [return: NotNullIfNotNull(nameof(newValue))]
#endif
  public static int? ValidateHueValue(int? newValue, string paramName)
  {
    if (newValue == null)
      return null;

    if (newValue.Value is < HueMinValue or > HueMaxValue) {
      throw new ArgumentOutOfRangeException(
        paramName: paramName,
        actualValue: newValue.Value,
        message: HueValueOutOfRangeExceptionMessage
      );
    }

    return newValue;
  }

#if LANG_VERSION_11_OR_GREATER && NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
  [return: NotNullIfNotNull(nameof(newValue))]
#endif
  public static int? ValidateSaturationValue(int? newValue, string paramName)
  {
    if (newValue == null)
      return null;

    if (newValue.Value is < SaturationMinValue or > SaturationMaxValue) {
      throw new ArgumentOutOfRangeException(
        paramName: paramName,
        actualValue: newValue.Value,
        message: SaturationValueOutOfRangeExceptionMessage
      );
    }

    return newValue;
  }

#if LANG_VERSION_11_OR_GREATER && NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
  [return: NotNullIfNotNull(nameof(newValue))]
#endif
  public static int ValidateColorTemperatureValue(int newValue, string paramName)
  {
#if false // TODO
    if (newValue is < ColorTemperatureMinValue or > ColorTemperatureMaxValue) {
      throw new ArgumentOutOfRangeException(
        paramName: paramName,
        actualValue: newValue,
        message: ColorTemperatureValueOutOfRangeExceptionMessage
      );
    }
#endif

    return newValue;
  }
}
