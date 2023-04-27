// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

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
    const string deviceModelName = "X-PSEUDO-TAPO-DEVICE";
    const string deviceMacAddress = "00:00:5E:00:53:42";
    const int deviceTimeZoneOffsetInMinutes = +9 /*hours*/ * 60;
    const int deviceGeolocationScaledLongitude = 1397666;

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        return (
          KnownErrorCodes.Success,
          new GetDeviceInfoResponse<TapoDeviceInfoResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new() {
              ModelName = deviceModelName,
              MacAddress = deviceMacAddress,
              TimeZoneOffset = deviceTimeZoneOffsetInMinutes,
              NetworkSsid = "44Oe44K144Op44K/44Km44Oz44Gr44GV44KI44Gq44KJV2ktRmk=",
              GeolocationLongitude = deviceGeolocationScaledLongitude,
            },
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services.BuildServiceProvider()
    );

    var info = await device.GetDeviceInfoAsync();

    Assert.AreEqual(deviceModelName, info.ModelName, nameof(info.ModelName));
    Assert.AreEqual(PhysicalAddress.Parse(deviceMacAddress), info.MacAddress, nameof(info.MacAddress));
    Assert.AreEqual(TimeSpan.FromMinutes(deviceTimeZoneOffsetInMinutes), info.TimeZoneOffset, nameof(info.TimeZoneOffset));
    Assert.AreEqual("マサラタウンにさよならWi-Fi", info.NetworkSsid, nameof(info.NetworkSsid));
    Assert.AreEqual(deviceGeolocationScaledLongitude / 10000.0m, info.GeolocationLongitude, nameof(info.GeolocationLongitude));
    Assert.IsNull(info.Id, nameof(info.Id));
    Assert.IsNull(info.IPAddress, nameof(info.IPAddress));
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
      serviceProvider: services.BuildServiceProvider()
    );

    Assert.AreEqual(
      currentState,
      await device.GetOnOffStateAsync()
    );
  }
}
