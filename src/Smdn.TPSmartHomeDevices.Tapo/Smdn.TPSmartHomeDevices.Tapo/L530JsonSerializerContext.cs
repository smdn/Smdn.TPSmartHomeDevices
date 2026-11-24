// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

[JsonSourceGenerationOptions(
  DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(SetDeviceInfoRequest<L530.SetBrightnessParameter>))]
[JsonSerializable(typeof(SetDeviceInfoRequest<L530.SetColorTemperatureParameter>))]
[JsonSerializable(typeof(SetDeviceInfoRequest<L530.SetColorParameter>))]
internal partial class L530JsonSerializerContext : JsonSerializerContext { }
