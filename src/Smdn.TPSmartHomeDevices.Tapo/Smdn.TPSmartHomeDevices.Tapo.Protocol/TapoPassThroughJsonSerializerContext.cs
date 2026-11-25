// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

[JsonSourceGenerationOptions(
  DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(GetCurrentPowerRequest))]
[JsonSerializable(typeof(GetDeviceInfoRequest))]
[JsonSerializable(typeof(GetDeviceUsageRequest))]
[JsonSerializable(typeof(GetEnergyUsageRequest))]
public partial class TapoPassThroughJsonSerializerContext : JsonSerializerContext { }
