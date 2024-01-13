// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json.Serialization;

using Smdn.TPSmartHomeDevices.Json;
using Smdn.TPSmartHomeDevices.Tapo.Json;

namespace Smdn.TPSmartHomeDevices.Tapo;

/// <summary>
/// Represents the usage monitoring data report for devices connected to the Tapo smart plug.
/// </summary>
/// <seealso cref="P110M.GetMonitoringDataAsync(System.Threading.CancellationToken)"/>
public class TapoPlugMonitoringData {
  /// <summary>
  /// Gets the <see cref="TimeSpan"/> value that represents today's total operating time.
  /// </summary>
  [JsonPropertyName("today_runtime")]
  [JsonConverter(typeof(TimeSpanInMinutesJsonConverter))]
  public TimeSpan? TotalOperatingTimeToday { get; init; }

  /// <summary>
  /// Gets the <see cref="TimeSpan"/> value that represents this month's total operating time.
  /// </summary>
  [JsonPropertyName("month_runtime")]
  [JsonConverter(typeof(TimeSpanInMinutesJsonConverter))]
  public TimeSpan? TotalOperatingTimeThisMonth { get; init; }

  /// <summary>
  /// Gets the <see cref="decimal"/> value that represents today's cumulative energy usage.
  /// </summary>
  /// <value>
  /// The cumulative usage amount of electric energy, in unit of watt-hours [Wh].
  /// <see langword="null"/> if the device does not return value for this property.
  /// </value>
  [JsonPropertyName("today_energy")]
  [JsonConverter(typeof(TapoElectricEnergyInWattHourJsonConverter))]
  public decimal? CumulativeEnergyUsageToday { get; init; }

  /// <summary>
  /// Gets the <see cref="decimal"/> value that represents this month's cumulative energy usage.
  /// </summary>
  /// <value>
  /// The cumulative usage amount of electric energy, in unit of watt-hours [Wh].
  /// <see langword="null"/> if the device does not return value for this property.
  /// </value>
  [JsonPropertyName("month_energy")]
  [JsonConverter(typeof(TapoElectricEnergyInWattHourJsonConverter))]
  public decimal? CumulativeEnergyUsageThisMonth { get; init; }

  /// <summary>
  /// Gets the <see cref="DateTime"/> that represents the date and time this monitoring data was measured.
  /// </summary>
  /// <value>
  /// The local date and time in the time zone set for the device.
  /// This property returns the <see cref="DateTime"/> for which <see cref="DateTime.Kind"/> is <see cref="DateTimeKind.Unspecified"/>.
  /// </value>
  [JsonPropertyName("local_time")]
  [JsonConverter(typeof(TapoLocalDateAndTimeJsonConverter))]
  public DateTime? TimeStamp { get; init; }

  // TODO: electricity_charge
  // [JsonPropertyName("electricity_charge")]
  // private ElectricityCharge? ElectricityCharge { get; init; }

  /// <summary>
  /// Gets the <see cref="decimal"/> value that represents the current power consumption.
  /// </summary>
  /// <value>
  /// The power consumption, in unit of watt [W].
  /// </value>
  [JsonPropertyName("current_power")]
  [JsonConverter(typeof(TapoElectricPowerInMilliWattJsonConverter))]
  public decimal? CurrentPowerConsumption { get; init; }
}
