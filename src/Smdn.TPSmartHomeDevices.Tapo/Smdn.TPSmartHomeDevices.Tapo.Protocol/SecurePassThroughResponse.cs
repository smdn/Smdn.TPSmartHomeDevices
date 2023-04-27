// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// The type that reflects <c>securePassthrough</c> JSON response.
/// </summary>
public readonly struct SecurePassThroughResponse<TPassThroughResponse> :
  ITapoResponse
  where TPassThroughResponse : ITapoPassThroughResponse
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
