// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

[JsonSourceGenerationOptions(
  DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(HandshakeRequest))]
[JsonSerializable(typeof(HandshakeRequest.RequestParameters))]
[JsonSerializable(typeof(HandshakeResponse))]
[JsonSerializable(typeof(HandshakeResponse.ResponseResult))]
[JsonSerializable(typeof(PassThroughRequest))]
[JsonSerializable(typeof(PassThroughResponse))]
[JsonSerializable(typeof(SecurePassThroughRequest))]
[JsonSerializable(typeof(SecurePassThroughResponse))]
internal partial class TapoPlainTextJsonSerializerContext : JsonSerializerContext { }
