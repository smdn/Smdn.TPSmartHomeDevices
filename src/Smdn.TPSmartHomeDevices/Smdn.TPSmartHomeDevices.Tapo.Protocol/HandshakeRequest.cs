// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

public readonly struct HandshakeRequest : ITapoRequest {
  [JsonPropertyName("method")]
  [JsonPropertyOrder(0)]
  public string Method => "handshake";

  [JsonPropertyName("params")]
  public RequestParameters Parameters { get; }

  public HandshakeRequest(string key)
  {
    Parameters = new(key);
  }

  public readonly record struct RequestParameters(
#pragma warning disable SA1313
    [property: JsonPropertyName("key")] string Key
#pragma warning restore SA1313
  ) {
#pragma warning disable CA1822
    [JsonPropertyName("requestTimeMils")]
    public long RequestTimeMilliseconds => 0L; // DateTimeOffset.Now.ToUnixTimeMilliseconds();
#pragma warning restore CA1822
  }
}
