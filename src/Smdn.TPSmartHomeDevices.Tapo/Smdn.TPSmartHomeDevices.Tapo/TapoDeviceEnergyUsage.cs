// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

using Smdn.TPSmartHomeDevices.Tapo.Json;

namespace Smdn.TPSmartHomeDevices.Tapo;

/// <summary>
/// Represents the cumulative electric energy usage of Tapo device as retrieved from the device.
/// </summary>
public readonly struct TapoDeviceEnergyUsage {
  /// <summary>Gets the <see cref="ElectricEnergyAmount"/> value that represents the amount of cumulative electric energy used today.</summary>
  /// <value>The <see cref="ElectricEnergyAmount"/>. <see langword="null"/> if the device does not return value for this property.</value>
  [JsonPropertyName("today")]
  [JsonConverter(typeof(TapoElectricEnergyAmountInWattHourJsonConverter))]
  public ElectricEnergyAmount? Today { get; init; }

  /// <summary>Gets the <see cref="ElectricEnergyAmount"/> value that represents the amount of cumulative electric energy used past 7 days.</summary>
  /// <value>The <see cref="ElectricEnergyAmount"/>. <see langword="null"/> if the device does not return value for this property.</value>
  [JsonPropertyName("past7")]
  [JsonConverter(typeof(TapoElectricEnergyAmountInWattHourJsonConverter))]
  public ElectricEnergyAmount? Past7Days { get; init; }

  /// <summary>Gets the <see cref="ElectricEnergyAmount"/> value that represents the amount of cumulative electric energy used past 30 days.</summary>
  /// <value>The <see cref="ElectricEnergyAmount"/>. <see langword="null"/> if the device does not return value for this property.</value>
  [JsonPropertyName("past30")]
  [JsonConverter(typeof(TapoElectricEnergyAmountInWattHourJsonConverter))]
  public ElectricEnergyAmount? Past30Days { get; init; }
}
