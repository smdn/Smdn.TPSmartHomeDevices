// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable SA1313
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

public readonly struct SetDeviceInfoResponse : ITapoPassThroughResponse {
  [JsonPropertyName("error_code")]
  public int ErrorCode { get; init; }

  [JsonPropertyName("result")]
  public ResponseResult Result { get; init; }

  public readonly struct ResponseResult {
    [JsonExtensionData]
    public IDictionary<string, object>? ExtraData { get; init; }
  }
}
