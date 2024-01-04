// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// The type that reflects <c>securePassthrough</c> JSON request.
/// </summary>
/// <typeparam name="TPassThroughRequest">A type that will be serialized to the value of the encapsulated <c>request</c> JSON property.</typeparam>
#pragma warning disable IDE0055
public readonly struct SecurePassThroughRequest<TPassThroughRequest> :
  ITapoRequest
  where TPassThroughRequest : notnull, ITapoPassThroughRequest
#pragma warning restore IDE0055
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
