// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Kasa;

[JsonSourceGenerationOptions(
  DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(
  typeof(KasaDevice.NullParameter),
  TypeInfoPropertyName = nameof(KasaDevice) + nameof(KasaDevice.NullParameter)
)]
[JsonSerializable(typeof(KasaDeviceInfo))]
[JsonSerializable(
  typeof(HS105.GetSysInfoRelayStateParameter),
  TypeInfoPropertyName = nameof(HS105) + nameof(HS105.GetSysInfoRelayStateParameter)
)]
[JsonSerializable(
  typeof(HS105.SetRelayStateParameter),
  TypeInfoPropertyName = nameof(HS105) + nameof(HS105.SetRelayStateParameter)
)]
[JsonSerializable(
  typeof(KL130.GetSysInfoLightStateParameter),
  TypeInfoPropertyName = nameof(KL130) + nameof(KL130.GetSysInfoLightStateParameter)
)]
[JsonSerializable(
  typeof(KL130.SetColorTemperatureParameter),
  TypeInfoPropertyName = nameof(KL130) + nameof(KL130.SetColorTemperatureParameter)
)]
[JsonSerializable(
  typeof(KL130.SetColorParameter),
  TypeInfoPropertyName = nameof(KL130) + nameof(KL130.SetColorParameter)
)]
[JsonSerializable(
  typeof(KL130.SetOnOffStateParameter),
  TypeInfoPropertyName = nameof(KL130) + nameof(KL130.SetOnOffStateParameter)
)]
internal partial class KasaDeviceJsonSerializerContext : JsonSerializerContext { }
