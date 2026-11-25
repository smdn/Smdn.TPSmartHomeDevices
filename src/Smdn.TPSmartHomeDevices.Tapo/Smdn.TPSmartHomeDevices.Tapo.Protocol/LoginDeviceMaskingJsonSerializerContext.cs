// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

#pragma warning disable CS3016
[JsonSourceGenerationOptions(
  DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
  Converters = [typeof(LoginDeviceRequest.TapoCredentialMaskingJsonConverter)]
)]
#pragma warning restore CS3016
[JsonSerializable(typeof(LoginDeviceRequest))]
internal partial class LoginDeviceMaskingJsonSerializerContext : JsonSerializerContext { }
