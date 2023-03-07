// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

public interface ITapoRequest {
/*
  [JsonPropertyName("method")]
  [JsonPropertyOrder(int.MinValue)]
*/
  string Method { get; }
}
