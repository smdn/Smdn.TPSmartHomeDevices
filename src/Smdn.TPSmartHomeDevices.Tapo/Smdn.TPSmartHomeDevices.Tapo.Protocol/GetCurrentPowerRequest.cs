// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// The type that reflects <c>get_current_power</c> JSON request.
/// </summary>
public readonly struct GetCurrentPowerRequest : ITapoPassThroughRequest {
  [JsonPropertyName("method")]
  [JsonPropertyOrder(0)]
  public string Method => "get_current_power";

#pragma warning disable CA1822
  [JsonPropertyName("requestTimeMils")]
  public long RequestTimeMilliseconds => 0L; // DateTimeOffset.Now.ToUnixTimeMilliseconds();
#pragma warning restore CA1822
}
