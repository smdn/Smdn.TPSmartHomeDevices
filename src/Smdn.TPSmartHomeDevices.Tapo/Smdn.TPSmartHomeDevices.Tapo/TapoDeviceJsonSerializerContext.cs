// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

[JsonSourceGenerationOptions(
  DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(PassThroughResponse<None>))]
[JsonSerializable(typeof(PassThroughResponse<TapoDevice.GetDeviceInfoOnOffStateResult>))]
[JsonSerializable(typeof(PassThroughResponse<TapoDevice.GetDeviceUsageResult>))]
[JsonSerializable(typeof(PassThroughResponse<TapoDeviceInfo>))]
[JsonSerializable(typeof(PassThroughResponse<TapoPlugMonitoringData>))]
[JsonSerializable(typeof(SetDeviceInfoRequest<TapoDevice.TurnOffParameter>))]
[JsonSerializable(typeof(SetDeviceInfoRequest<TapoDevice.TurnOnParameter>))]
internal partial class TapoDeviceJsonSerializerContext : JsonSerializerContext { }
