// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

public readonly struct SecurePassThroughRequest<TPassThroughRequest> :
  ITapoRequest
  where TPassThroughRequest : ITapoPassThroughRequest
{
  [JsonPropertyName("method")]
  [JsonPropertyOrder(0)]
  public string Method => "securePassthrough";

  [JsonPropertyName("params")]
  public RequestParams Params { get; }

  public readonly struct RequestParams {
    [JsonPropertyName("request")]
    public TPassThroughRequest PassThroughRequest { get; init; }
  }

  public SecurePassThroughRequest(TPassThroughRequest passThroughRequest)
  {
    Params = new() { PassThroughRequest = passThroughRequest };
  }
}
