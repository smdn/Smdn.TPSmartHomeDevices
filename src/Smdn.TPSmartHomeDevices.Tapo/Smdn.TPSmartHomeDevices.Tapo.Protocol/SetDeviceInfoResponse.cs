// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable SA1313
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// The type that reflects <c>set_device_info</c> JSON response.
/// </summary>
/// <typeparam name="TResult">A type that will be deserialized from the value of the <c>result</c> JSON property.</typeparam>
public readonly struct SetDeviceInfoResponse<TResult> : ITapoPassThroughResponse {
  [JsonPropertyName("error_code")]
  public int ErrorCode { get; init; }

  [JsonPropertyName("result")]
  public TResult Result { get; init; }
}
