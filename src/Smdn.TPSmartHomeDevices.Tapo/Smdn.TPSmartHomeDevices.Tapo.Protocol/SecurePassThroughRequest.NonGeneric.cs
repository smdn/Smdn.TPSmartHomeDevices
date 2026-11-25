// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// The type that reflects <c>securePassthrough</c> JSON request.
/// </summary>
internal readonly struct SecurePassThroughRequest(PassThroughRequest passThroughRequest) : ITapoRequest {
  [JsonPropertyName("method")]
  [JsonPropertyOrder(0)]
  public string Method => "securePassthrough";

  [JsonPropertyName("params")]
  [JsonConverter(typeof(PassThroughRequestConverter))]
  public PassThroughRequest Params => passThroughRequest;
}
