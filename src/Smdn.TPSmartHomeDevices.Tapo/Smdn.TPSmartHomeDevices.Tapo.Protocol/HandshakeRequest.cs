// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// The type that reflects <c>handshake</c> JSON request.
/// </summary>
public readonly struct HandshakeRequest : ITapoRequest {
  [JsonPropertyName("method")]
  [JsonPropertyOrder(0)]
  public string Method => "handshake";

  [JsonPropertyName("params")]
  public RequestParameters Parameters { get; }

  public HandshakeRequest(string key)
  {
    Parameters = new() { Key = key ?? throw new ArgumentNullException(nameof(key)) };
  }

  public readonly struct RequestParameters {
    [JsonPropertyName("key")]
    public string Key { get; init; }

#pragma warning disable CA1822
    [JsonPropertyName("requestTimeMils")]
    public long RequestTimeMilliseconds => 0L; // DateTimeOffset.Now.ToUnixTimeMilliseconds();
#pragma warning restore CA1822
  }
}
