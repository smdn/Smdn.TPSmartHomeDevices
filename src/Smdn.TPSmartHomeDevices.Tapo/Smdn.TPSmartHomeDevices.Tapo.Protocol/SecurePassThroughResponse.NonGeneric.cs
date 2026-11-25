// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// The type that reflects <c>securePassthrough</c> JSON response.
/// </summary>
internal readonly struct SecurePassThroughResponse : ITapoResponse {
  [JsonPropertyName("error_code")]
  public int ErrorCode { get; init; }

  [JsonPropertyName("result")]
  [JsonConverter(typeof(PassThroughResponseConverter))]
  public PassThroughResponse Result { get; init; }
}
