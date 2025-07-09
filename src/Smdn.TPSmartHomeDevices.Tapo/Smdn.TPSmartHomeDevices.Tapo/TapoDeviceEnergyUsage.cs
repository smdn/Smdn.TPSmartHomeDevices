// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1815 // TODO: implement equality comparison

using System.Text.Json.Serialization;

using Smdn.TPSmartHomeDevices.Tapo.Json;

namespace Smdn.TPSmartHomeDevices.Tapo;

/// <summary>
/// Represents the cumulative electric energy usage of Tapo device as retrieved from the device.
/// </summary>
public readonly struct TapoDeviceEnergyUsage {
  /// <summary>
  /// Gets the <see cref="decimal"/> value that represents the cumulative amount of electric energy used today.
  /// </summary>
  /// <value>
  /// The cumulative usage amount of electric energy, in unit of watt-hours [Wh].
  /// <see langword="null"/> if the device does not return value for this property.
  /// </value>
  [JsonPropertyName("today")]
  [JsonConverter(typeof(TapoElectricEnergyInWattHourJsonConverter))]
  public decimal? Today { get; init; }

  /// <summary>
  /// Gets the <see cref="decimal"/> value that represents the cumulative amount of electric energy used past 7 days.
  /// </summary>
  /// <value>
  /// The cumulative usage amount of electric energy, in unit of watt-hours [Wh].
  /// <see langword="null"/> if the device does not return value for this property.
  /// </value>
  [JsonPropertyName("past7")]
  [JsonConverter(typeof(TapoElectricEnergyInWattHourJsonConverter))]
  public decimal? Past7Days { get; init; }

  /// <summary>
  /// Gets the <see cref="decimal"/> value that represents the cumulative amount of electric energy used past 30 days.
  /// </summary>
  /// <value>
  /// The cumulative usage amount of electric energy, in unit of watt-hours [Wh].
  /// <see langword="null"/> if the device does not return value for this property.
  /// </value>
  [JsonPropertyName("past30")]
  [JsonConverter(typeof(TapoElectricEnergyInWattHourJsonConverter))]
  public decimal? Past30Days { get; init; }
}
