// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

public readonly struct GetDeviceInfoResponse : ITapoPassThroughResponse {
  [JsonPropertyName("error_code")]
  public int ErrorCode { get; init; }

  [JsonPropertyName("result")]
  public TapoDeviceInfo Result { get; init; }
}
