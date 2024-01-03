// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

partial class TapoDeviceTests {

  private readonly struct TapoDeviceInfoResult {
    [JsonPropertyName("model")]
    public string? ModelName { get; init; }

    [JsonPropertyName("mac")]
    public string? MacAddress { get; init; }

    [JsonPropertyName("time_diff")]
    public int? TimeZoneOffset { get; init; }

    [JsonPropertyName("ssid")]
    public string? NetworkSsid { get; init; }

    [JsonPropertyName("longitude")]
    public int? GeolocationLongitude { get; init; }
  }

  [Test]
  public async Task GetDeviceInfoAsync_OfTapoDeviceInfo()
  {
    const string DeviceModelName = "X-PSEUDO-TAPO-DEVICE";
    const string DeviceMacAddress = "00:00:5E:00:53:42";
    const int DeviceTimeZoneOffsetInMinutes = +9 /*hours*/ * 60;
    const int DeviceGeolocationScaledLongitude = 1397666;

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        return (
          KnownErrorCodes.Success,
          new GetDeviceInfoResponse<TapoDeviceInfoResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new() {
              ModelName = DeviceModelName,
              MacAddress = DeviceMacAddress,
              TimeZoneOffset = DeviceTimeZoneOffsetInMinutes,
              NetworkSsid = "44Oe44K144Op44K/44Km44Oz44Gr44GV44KI44Gq44KJV2ktRmk=",
              GeolocationLongitude = DeviceGeolocationScaledLongitude,
            },
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    var info = await device.GetDeviceInfoAsync();

    Assert.That(info.ModelName, Is.EqualTo(DeviceModelName), nameof(info.ModelName));
    Assert.That(info.MacAddress, Is.EqualTo(PhysicalAddress.Parse(DeviceMacAddress)), nameof(info.MacAddress));
    Assert.That(info.TimeZoneOffset, Is.EqualTo(TimeSpan.FromMinutes(DeviceTimeZoneOffsetInMinutes)), nameof(info.TimeZoneOffset));
    Assert.That(info.NetworkSsid, Is.EqualTo("マサラタウンにさよならWi-Fi"), nameof(info.NetworkSsid));
    Assert.That(info.GeolocationLongitude, Is.EqualTo(DeviceGeolocationScaledLongitude / 10000.0m), nameof(info.GeolocationLongitude));
    Assert.That(info.Id, Is.Null, nameof(info.Id));
    Assert.That(info.IPAddress, Is.Null, nameof(info.IPAddress));
  }

  private readonly struct GetDeviceInfoResponseGetOnOffStateResult {
    [JsonPropertyName("device_on")]
    public bool DeviceOn { get; init; }
  }

  [TestCase(true)]
  [TestCase(false)]
  public async Task GetOnOffStateAsync(bool currentState)
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        return (
          KnownErrorCodes.Success,
          new GetDeviceInfoResponse<GetDeviceInfoResponseGetOnOffStateResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new() { DeviceOn = currentState },
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.That(
      await device.GetOnOffStateAsync(),
      Is.EqualTo(currentState)
    );
  }
}
