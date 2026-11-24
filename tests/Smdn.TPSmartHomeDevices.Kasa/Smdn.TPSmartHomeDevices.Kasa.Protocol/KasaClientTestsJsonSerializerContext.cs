// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Kasa.Protocol;

[JsonSourceGenerationOptions(
  DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(KasaClientTests.NullParameter), TypeInfoPropertyName = $"{nameof(KasaClientTests)}{nameof(KasaClientTests.NullParameter)}")]
internal partial class KasaClientTestsJsonSerializerContext : JsonSerializerContext { }
