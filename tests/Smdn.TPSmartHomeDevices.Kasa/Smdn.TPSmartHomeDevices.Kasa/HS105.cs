// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading.Tasks;

using NUnit.Framework;

using Smdn.TPSmartHomeDevices.Kasa.Protocol;

namespace Smdn.TPSmartHomeDevices.Kasa;

[TestFixture]
public class HS105Tests {
  [Test]
  public new void ToString()
    => ConcreteKasaDeviceCommonTests.TestToString<HS105>();

  [TestCaseSource(typeof(ConcreteKasaDeviceCommonTests), nameof(ConcreteKasaDeviceCommonTests.YiledTestCases_Ctor_ArgumentException))]
  public void Ctor_ArgumentException(Type[] ctorParameterTypes, object?[] ctorParameters, Type? expectedExceptionType, string expectedParamName)
    => ConcreteKasaDeviceCommonTests.TestCtor_ArgumentException<HS105>(ctorParameterTypes, ctorParameters, expectedExceptionType, expectedParamName);

  [Test]
  public async Task ISmartDevice_GetDeviceInfoAsync()
  {
    const string DeviceModelName = "X-PSEUDO-TAPO-DEVICE";
    const string DeviceFirmwareVersion = "1.2.3 Build 456789";
    const string DeviceHardwareVersion = "1.0";
    const string DeviceMacAddress = "00:00:5E:00:53:42";

    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, request) => JsonDocument.Parse(@$"{{
  ""system"":{{
    ""get_sysinfo"":{{
      ""sw_ver"":""{DeviceFirmwareVersion}"",
      ""hw_ver"":""{DeviceHardwareVersion}"",
      ""model"":""{DeviceModelName}"",
      ""mac"":""{DeviceMacAddress}"",
      ""deviceId"":""0123456789ABCDEF""
    }}
  }}
}}")
    };

    pseudoDevice.Start();

    using var device = new HS105(
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    var smartDevice = (ISmartDevice)device;
    var info = await smartDevice.GetDeviceInfoAsync();

    Assert.That(info.Id.ToArray(), Is.EqualTo(new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF }).AsCollection, nameof(info.Id));
    Assert.That(info.ModelName, Is.EqualTo(DeviceModelName), nameof(info.ModelName));
    Assert.That(info.FirmwareVersion, Is.EqualTo(DeviceFirmwareVersion), nameof(info.FirmwareVersion));
    Assert.That(info.HardwareVersion, Is.EqualTo(DeviceHardwareVersion), nameof(info.HardwareVersion));
    Assert.That(info.MacAddress, Is.EqualTo(PhysicalAddress.Parse(DeviceMacAddress)), nameof(info.MacAddress));
  }

  [Test]
  public async Task TurnOnAsync()
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, request) => {
        Assert.That(
          JsonSerializer.Serialize(request),
          Is.EqualTo(@"{""system"":{""set_relay_state"":{""state"":1}}}"),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""system"":{""set_relay_state"":{""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new HS105(
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    Assert.DoesNotThrowAsync(async () => await device.TurnOnAsync());
  }

  [Test]
  public async Task TurnOffAsync()
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, request) => {
        Assert.That(
          JsonSerializer.Serialize(request),
          Is.EqualTo(@"{""system"":{""set_relay_state"":{""state"":0}}}"),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""system"":{""set_relay_state"":{""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new HS105(
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    Assert.DoesNotThrowAsync(async () => await device.TurnOffAsync());
  }

  [TestCase(true)]
  [TestCase(false)]
  public async Task SetOnOffStateAsync(bool newState)
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, request) => {
        Assert.That(
          JsonSerializer.Serialize(request),
          Is.EqualTo(@"{""system"":{""set_relay_state"":{""state"":" + (newState ? "1" : "0") + "}}}"),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""system"":{""set_relay_state"":{""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new HS105(
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    Assert.DoesNotThrowAsync(async () => await device.SetOnOffStateAsync(newOnOffState: newState));
  }

  private static System.Collections.IEnumerable YieldTestCases_GetOnOffStateAsync()
  {
    yield return new object[] {
      @"{""system"":{""get_sysinfo"":{""sw_ver"":""x.y.z"",""model"":""HS105"",""relay_state"":1,""on_time"":0,""err_code"":0}}}",
      true,
    };
    yield return new object[] {
      @"{""system"":{""get_sysinfo"":{""sw_ver"":""x.y.z"",""model"":""HS105"",""relay_state"":0,""on_time"":0,""err_code"":0}}}",
      false,
    };
    yield return new object[] {
      @"{""system"":{""get_sysinfo"":{""err_code"":0}}}",
      false,
    };
  }

  [TestCaseSource(nameof(YieldTestCases_GetOnOffStateAsync))]
  public async Task GetOnOffStateAsync(string responseJson, bool expectedState)
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, request) => {
        Assert.That(
          JsonSerializer.Serialize(request),
          Is.EqualTo(@"{""system"":{""get_sysinfo"":{}}}"),
          nameof(request)
        );

        return JsonDocument.Parse(responseJson);
      }
    };

    pseudoDevice.Start();

    using var device = new HS105(
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    Assert.DoesNotThrowAsync(async () => {
      Assert.That(await device.GetOnOffStateAsync(), Is.EqualTo(expectedState), nameof(expectedState));
    });
  }
}
