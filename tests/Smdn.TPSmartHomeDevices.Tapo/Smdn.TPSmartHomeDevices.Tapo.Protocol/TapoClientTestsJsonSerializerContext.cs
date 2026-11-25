// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

[JsonSourceGenerationOptions(
  DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(PassThroughResponse<None>), TypeInfoPropertyName = $"{nameof(PassThroughResponse<>)}Of{nameof(None)}")]
public partial class TapoClientTestsJsonSerializerContext : JsonSerializerContext { }
