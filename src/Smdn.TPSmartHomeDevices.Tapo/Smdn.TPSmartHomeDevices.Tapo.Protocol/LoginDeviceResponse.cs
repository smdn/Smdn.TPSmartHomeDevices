// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1815 // TODO: implement equality comparison

using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// The type that reflects <c>login_device</c> JSON response.
/// </summary>
public readonly struct LoginDeviceResponse : ITapoPassThroughResponse {
  [JsonPropertyName("error_code")]
  public int ErrorCode { get; init; }

  [JsonPropertyName("result")]
  public ResponseResult Result { get; init; }

#pragma warning disable CA1034
  public readonly struct ResponseResult {
    [JsonPropertyName("token")]
    public string Token { get; init; }
  }
#pragma warning restore CA1034
}
