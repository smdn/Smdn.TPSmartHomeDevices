// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

public readonly struct SecurePassThroughResponse<TPassThroughResponse> :
  ITapoResponse
  where TPassThroughResponse : ITapoPassThroughResponse
{
  [JsonPropertyName("error_code")]
  public ErrorCode ErrorCode { get; init; }

  [JsonPropertyName("result")]
  public ResponseResult Result { get; init; }

  public SecurePassThroughResponse(
    ErrorCode errorCode,
    TPassThroughResponse passThroughResponse
  )
  {
    ErrorCode = errorCode;
    Result = new(passThroughResponse);
  }

  public readonly record struct ResponseResult(
#pragma warning disable SA1313
    [property: JsonPropertyName("response")]
    TPassThroughResponse PassThroughResponse
#pragma warning restore SA1313
  );
}
