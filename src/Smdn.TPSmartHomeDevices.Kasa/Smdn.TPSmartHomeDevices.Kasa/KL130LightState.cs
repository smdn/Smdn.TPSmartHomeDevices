// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
using System.Text.Json.Serialization;

using Smdn.TPSmartHomeDevices.Kasa.Json;

namespace Smdn.TPSmartHomeDevices.Kasa;

public readonly struct KL130LightState {
  [JsonPropertyName("on_off")]
  [JsonConverter(typeof(KasaNumericalBooleanJsonConverter))]
  public bool IsOn { get; init; }

  /// <summary>Gets the current mode string of the light bulb.</summary>
  /// <value>If light is off, the value will be <see langword="null"/>.</value>
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
  [MemberNotNullWhenAttribute(true, nameof(IsOn))]
#endif
  [JsonPropertyName("mode")]
  public string? Mode { get; init; }

  /// <summary>Gets the current hue value of the light bulb.</summary>
  /// <value>If light is off, the value will be <see langword="null"/>.</value>
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
  [MemberNotNullWhenAttribute(true, nameof(IsOn))]
#endif
  [JsonPropertyName("hue")]
  public int? Hue { get; init; }

  /// <summary>Gets the current saturation value of the light bulb.</summary>
  /// <value>If light is off, the value will be <see langword="null"/>.</value>
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
  [MemberNotNullWhenAttribute(true, nameof(IsOn))]
#endif
  [JsonPropertyName("saturation")]
  public int? Saturation { get; init; }

  /// <summary>Gets the current color temperature value of the light bulb.</summary>
  /// <value>If light is off, the value will be <see langword="null"/>.</value>
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
  [MemberNotNullWhenAttribute(true, nameof(IsOn))]
#endif
  [JsonPropertyName("color_temp")]
  public int? ColorTemperature { get; init; }

  /// <summary>Gets the current brightness value of the light bulb.</summary>
  /// <value>If light is off, the value will be <see langword="null"/>.</value>
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
  [MemberNotNullWhenAttribute(true, nameof(IsOn))]
#endif
  [JsonPropertyName("brightness")]
  public int? Brightness { get; init; }
}
