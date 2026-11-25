// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

[JsonSourceGenerationOptions(
  DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(SetDeviceInfoRequest<TapoDeviceTests.NullParameters>), TypeInfoPropertyName = $"{nameof(SetDeviceInfoRequest<>)}Of{nameof(TapoDeviceTests)}{nameof(TapoDeviceTests.NullParameters)}")]
[JsonSerializable(typeof(SetDeviceInfoRequest<TapoDeviceTests.SetDeviceInfoOnOffStateParameter>), TypeInfoPropertyName = $"{nameof(SetDeviceInfoRequest<>)}Of{nameof(TapoDeviceTests)}{nameof(TapoDeviceTests.SetDeviceInfoOnOffStateParameter)}")]
[JsonSerializable(typeof(PassThroughResponse<TapoDeviceTests.NullResult>), TypeInfoPropertyName = $"{nameof(PassThroughResponse<>)}Of{nameof(TapoDeviceTests)}{nameof(TapoDeviceTests.NullResult)}")]
[JsonSerializable(typeof(PassThroughResponse<TapoDeviceTests.GetDeviceInfoResult>), TypeInfoPropertyName = $"{nameof(PassThroughResponse<>)}Of{nameof(TapoDeviceTests)}{nameof(TapoDeviceTests.GetDeviceInfoResult)}")]
[JsonSerializable(typeof(PassThroughResponse<TapoDeviceTests.GetDeviceInfoOnOffStateResult>), TypeInfoPropertyName = $"{nameof(PassThroughResponse<>)}Of{nameof(TapoDeviceTests)}{nameof(TapoDeviceTests.GetDeviceInfoOnOffStateResult)}")]
internal partial class TapoDeviceTestsJsonSerializerContext : JsonSerializerContext { }
