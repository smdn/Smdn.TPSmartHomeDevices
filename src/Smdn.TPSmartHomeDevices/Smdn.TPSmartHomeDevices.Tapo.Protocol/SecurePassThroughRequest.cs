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

  public readonly record struct RequestParams(
#pragma warning disable SA1313
    [property: JsonPropertyName("request")]
    TPassThroughRequest PassThroughRequest
#pragma warning restore SA1313
  );

  public SecurePassThroughRequest(TPassThroughRequest passThroughRequest)
  {
    Params = new(passThroughRequest);
  }
}
