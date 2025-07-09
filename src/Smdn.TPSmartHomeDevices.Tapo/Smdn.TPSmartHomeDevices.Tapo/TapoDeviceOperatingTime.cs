// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1815 // TODO: implement equality comparison

using System;
using System.Text.Json.Serialization;

using Smdn.TPSmartHomeDevices.Json;

namespace Smdn.TPSmartHomeDevices.Tapo;

/// <summary>
/// Represents the total operating time of Tapo device as retrieved from the device.
/// </summary>
public readonly struct TapoDeviceOperatingTime {
  /// <summary>Gets the <see cref="TimeSpan"/> value that represents the cumulative usage time for today.</summary>
  /// <value>The <see cref="TimeSpan"/>. <see langword="null"/> if the device does not return value for this property.</value>
  [JsonPropertyName("today")]
  [JsonConverter(typeof(TimeSpanInMinutesJsonConverter))]
  public TimeSpan? Today { get; init; }

  /// <summary>Gets the <see cref="TimeSpan"/> value that represents the cumulative usage time for past 7 days.</summary>
  /// <value>The <see cref="TimeSpan"/>. <see langword="null"/> if the device does not return value for this property.</value>
  [JsonPropertyName("past7")]
  [JsonConverter(typeof(TimeSpanInMinutesJsonConverter))]
  public TimeSpan? Past7Days { get; init; }

  /// <summary>Gets the <see cref="TimeSpan"/> value that represents the cumulative usage time for past 30 days.</summary>
  /// <value>The <see cref="TimeSpan"/>. <see langword="null"/> if the device does not return value for this property.</value>
  [JsonPropertyName("past30")]
  [JsonConverter(typeof(TimeSpanInMinutesJsonConverter))]
  public TimeSpan? Past30Days { get; init; }
}
