// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// The type that reflects <c>securePassthrough</c> JSON response.
/// </summary>
/// <typeparam name="TPassThroughResponse">A type that will be deserialized from the value of the encapsulated <c>response</c> JSON property.</typeparam>
#pragma warning disable IDE0055
public readonly struct SecurePassThroughResponse<TPassThroughResponse> :
  ITapoResponse
  where TPassThroughResponse : ITapoPassThroughResponse
#pragma warning restore IDE0055
{
  [JsonPropertyName("error_code")]
  public int ErrorCode { get; init; }

  [JsonPropertyName("result")]
  public ResponseResult Result { get; init; }

  public SecurePassThroughResponse(
    int errorCode,
    TPassThroughResponse passThroughResponse
  )
  {
    ErrorCode = errorCode;
    Result = new() { PassThroughResponse = passThroughResponse };
  }

  public readonly struct ResponseResult {
    [JsonPropertyName("response")]
    public TPassThroughResponse PassThroughResponse { get; init; }
  }
}
