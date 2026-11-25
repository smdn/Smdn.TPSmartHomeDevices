// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

[JsonSourceGenerationOptions(
  DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(SetDeviceInfoRequest<L900.LightingEffectParameter>))]
[JsonSerializable(typeof(SetDeviceInfoRequest<L900.SetBrightnessParameter>))]
[JsonSerializable(typeof(SetDeviceInfoRequest<L900.SetColorParameter>))]
internal partial class L900JsonSerializerContext : JsonSerializerContext { }
