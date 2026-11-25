// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Kasa;

[JsonSourceGenerationOptions(
  DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(KasaDeviceTests.NullParameter), TypeInfoPropertyName = $"{nameof(KasaDeviceTests)}{nameof(KasaDeviceTests.NullParameter)}")]
internal partial class KasaDeviceTestsJsonSerializerContext : JsonSerializerContext { }
