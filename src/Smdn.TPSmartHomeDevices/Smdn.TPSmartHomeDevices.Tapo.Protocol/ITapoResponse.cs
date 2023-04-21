// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

public interface ITapoResponse {
/*
  [JsonPropertyName("error_code")]
*/
  public int ErrorCode { get; init; }
}
