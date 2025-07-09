// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
// cSpell:ignore smartlife,smartbulb,lightingservice,sysinfo
using System;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading.Tasks;

using NUnit.Framework;

using Smdn.TPSmartHomeDevices.Kasa.Protocol;

namespace Smdn.TPSmartHomeDevices.Kasa;

[TestFixture]
public class KL130Tests {
  [Test]
  public new void ToString()
    => ConcreteKasaDeviceCommonTests.TestToString<KL130>();

  [TestCaseSource(typeof(ConcreteKasaDeviceCommonTests), nameof(ConcreteKasaDeviceCommonTests.YieldTestCases_Ctor_ArgumentException))]
  public void Ctor_ArgumentException(Type[] ctorParameterTypes, object?[] ctorParameters, Type? expectedExceptionType, string expectedParamName)
    => ConcreteKasaDeviceCommonTests.TestCtor_ArgumentException<KL130>(ctorParameterTypes, ctorParameters, expectedExceptionType, expectedParamName);

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
      ""mic_mac"":""{DeviceMacAddress}"",
      ""deviceId"":""0123456789ABCDEF""
    }}
  }}
}}")
    };

    pseudoDevice.Start();

    using var device = new KL130(
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

  private static System.Collections.IEnumerable YieldTestCases_TransitionPeriod()
  {
    yield return new object?[] { TimeSpan.Zero, "0" };
    yield return new object?[] { TimeSpan.FromMilliseconds(1), "1" };
    yield return new object?[] { TimeSpan.FromSeconds(1), "1000" };
  }

  [Test]
  public async Task TurnOnAsync()
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, request) => {
        Assert.That(
          JsonSerializer.Serialize(request),
          Is.EqualTo(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":1,""transition_period"":0,""ignore_default"":0}}}"),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":1,""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":0,""brightness"":100,""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new KL130(
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    Assert.DoesNotThrowAsync(async () => await device.TurnOnAsync());
  }

  [TestCaseSource(nameof(YieldTestCases_TransitionPeriod))]
  public async Task TurnOnAsync_WithTransitionPeriod(
    TimeSpan transitionPeriod,
    string expectedTransitionPeriod
  )
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, request) => {
        Assert.That(
          JsonSerializer.Serialize(request),
          Is.EqualTo(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":1,""transition_period"":" + expectedTransitionPeriod + @",""ignore_default"":0}}}"),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":1,""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":0,""brightness"":100,""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new KL130(
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    Assert.DoesNotThrowAsync(async () => await device.TurnOnAsync(transitionPeriod: transitionPeriod));
  }

  [Test]
  public async Task TurnOffAsync()
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, request) => {
        Assert.That(
          JsonSerializer.Serialize(request),
          Is.EqualTo(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":0,""transition_period"":0,""ignore_default"":0}}}"),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":0,""dft_on_state"":{""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":0,""brightness"":100},""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new KL130(
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    Assert.DoesNotThrowAsync(async () => await device.TurnOffAsync());
  }

  [TestCaseSource(nameof(YieldTestCases_TransitionPeriod))]
  public async Task TurnOffAsync_WithTransitionPeriod(
    TimeSpan transitionPeriod,
    string expectedTransitionPeriod
  )
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, request) => {
        Assert.That(
          JsonSerializer.Serialize(request),
          Is.EqualTo(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":0,""transition_period"":" + expectedTransitionPeriod + @",""ignore_default"":0}}}"),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":0,""dft_on_state"":{""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":0,""brightness"":100},""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new KL130(
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    Assert.DoesNotThrowAsync(async () => await device.TurnOffAsync(transitionPeriod: transitionPeriod));
  }

  [TestCase(true)]
  [TestCase(false)]
  public async Task SetOnOffStateAsync(bool newState)
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, request) => {
        Assert.That(
          JsonSerializer.Serialize(request),
          Is.EqualTo(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":" + (newState ? "1" : "0") + @",""transition_period"":0,""ignore_default"":0}}}"),
          nameof(request)
        );

        return newState
          ? JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":1,""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":0,""brightness"":100,""err_code"":0}}}")
          : JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":0,""dft_on_state"":{""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":0,""brightness"":100},""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new KL130(
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    Assert.DoesNotThrowAsync(async () => await device.SetOnOffStateAsync(newOnOffState: newState));
  }

  private static System.Collections.IEnumerable YieldTestCases_SetOnOffStateAsync_WithTransitionPeriod()
  {
    foreach (object?[] args in YieldTestCases_TransitionPeriod()) {
      yield return new object?[] { true, args[0], args[1] };
      yield return new object?[] { false, args[0], args[1] };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_SetOnOffStateAsync_WithTransitionPeriod))]
  public async Task SetOnOffStateAsync_WithTransitionPeriod(
    bool newState,
    TimeSpan transitionPeriod,
    string expectedTransitionPeriod
  )
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, request) => {
        Assert.That(
          JsonSerializer.Serialize(request),
          Is.EqualTo(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":" + (newState ? "1" : "0") + @",""transition_period"":"+ expectedTransitionPeriod + @",""ignore_default"":0}}}"),
          nameof(request)
        );

        return newState
          ? JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":1,""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":0,""brightness"":100,""err_code"":0}}}")
          : JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":0,""dft_on_state"":{""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":0,""brightness"":100},""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new KL130(
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    Assert.DoesNotThrowAsync(async () => await device.SetOnOffStateAsync(newOnOffState: newState, transitionPeriod: transitionPeriod));
  }

  private static System.Collections.IEnumerable YieldTestCases_SetColorTemperatureAsync()
  {
    yield return new object?[] { 3000, null, TimeSpan.Zero, null, "0" };
    yield return new object?[] { 5000, 1, TimeSpan.Zero, "1", "0" };
    yield return new object?[] { 5000, 100, TimeSpan.Zero, "100", "0" };
    yield return new object?[] { 8000, 100, TimeSpan.FromMilliseconds(1), "100", "1" };
    yield return new object?[] { 8000, 100, TimeSpan.FromSeconds(1), "100", "1000" };
  }

  [TestCaseSource(nameof(YieldTestCases_SetColorTemperatureAsync))]
  public async Task SetColorTemperatureAsync(
    int colorTemperature,
    int? brightness,
    TimeSpan transitionPeriod,
    string expectedBrightness,
    string expectedTransitionPeriod
  )
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, request) => {
        Assert.That(
          JsonSerializer.Serialize(request),
          Is.EqualTo(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""color_temp"":" + colorTemperature.ToString(provider: null) + (expectedBrightness is null ? string.Empty : @",""brightness"":" + expectedBrightness) + @",""transition_period"":"+ expectedTransitionPeriod + @",""on_off"":1,""ignore_default"":1}}}"),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":1,""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":" + colorTemperature.ToString(provider: null) + (expectedBrightness is null ? string.Empty : @",""brightness"":" + expectedBrightness) + @",""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new KL130(
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    Assert.DoesNotThrowAsync(
      async () => await device.SetColorTemperatureAsync(
        colorTemperature: colorTemperature,
        brightness: brightness,
        transitionPeriod: transitionPeriod
      )
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_SetColorAsync()
  {
    yield return new object?[] { 0, 0, null, TimeSpan.Zero, null, "0" };
    yield return new object?[] { 360, 100, null, TimeSpan.Zero, null, "0" };
    yield return new object?[] { 180, 50, 1, TimeSpan.Zero, "1", "0" };
    yield return new object?[] { 360, 100, 100, TimeSpan.Zero, "100", "0" };
    yield return new object?[] { 360, 100, 100, TimeSpan.FromMilliseconds(1), "100", "1" };
    yield return new object?[] { 360, 100, 100, TimeSpan.FromSeconds(1), "100", "1000" };
  }

  [TestCaseSource(nameof(YieldTestCases_SetColorAsync))]
  public async Task SetColorAsync(
    int hue,
    int saturation,
    int? brightness,
    TimeSpan transitionPeriod,
    string expectedBrightness,
    string expectedTransitionPeriod
  )
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, request) => {
        Assert.That(
          JsonSerializer.Serialize(request),
          Is.EqualTo(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""hue"":" + hue.ToString(provider: null) + @",""saturation"":" + saturation.ToString(provider: null) + (expectedBrightness is null ? string.Empty : @",""brightness"":" + expectedBrightness) + @",""transition_period"":"+ expectedTransitionPeriod + @",""color_temp"":0,""on_off"":1,""ignore_default"":1}}}"),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":1,""mode"":""normal"",""hue"":" + hue.ToString(provider: null) + @",""saturation"":" + saturation.ToString(provider: null) + @",""color_temp"":0" + (expectedBrightness is null ? string.Empty : @",""brightness"":" + expectedBrightness) + @",""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new KL130(
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    Assert.DoesNotThrowAsync(
      async () => await device.SetColorAsync(
        hue: hue,
        saturation: saturation,
        brightness: brightness,
        transitionPeriod: transitionPeriod
      )
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_GetLightStateAsync()
  {
    yield return new object[] {
      @"{""system"":{""get_sysinfo"":{""sw_ver"":""x.y.z"",""model"":""KL130"",""light_state"":{""on_off"":1,""hue"":180,""saturation"":100,""color_temp"":5000,""brightness"":50},""err_code"":0}}}",
      new Action<KL130LightState>(static lightState => {
        Assert.That(lightState.IsOn, Is.True, nameof(lightState.IsOn));
        Assert.That(lightState.Hue, Is.Not.Null, nameof(lightState.Hue));
        Assert.That(lightState.Hue!.Value, Is.EqualTo(180), nameof(lightState.Hue));
        Assert.That(lightState.Saturation, Is.Not.Null, nameof(lightState.Saturation));
        Assert.That(lightState.Saturation!.Value, Is.EqualTo(100), nameof(lightState.Saturation));
        Assert.That(lightState.ColorTemperature, Is.Not.Null, nameof(lightState.ColorTemperature));
        Assert.That(lightState.ColorTemperature!.Value, Is.EqualTo(5000), nameof(lightState.ColorTemperature));
        Assert.That(lightState.Brightness, Is.Not.Null, nameof(lightState.Brightness));
        Assert.That(lightState.Brightness!.Value, Is.EqualTo(50), nameof(lightState.Brightness));
      }),
    };
    yield return new object[] {
      @"{""system"":{""get_sysinfo"":{""sw_ver"":""x.y.z"",""model"":""KL130"",""light_state"":{""on_off"":0,""dft_on_state"":{""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":5000,""brightness"":50}},""err_code"":0}}}",
      new Action<KL130LightState>(static lightState => {
        Assert.That(lightState.IsOn, Is.False, nameof(lightState.IsOn));
        Assert.That(lightState.Hue, Is.Null, nameof(lightState.Hue));
        Assert.That(lightState.Saturation, Is.Null, nameof(lightState.Saturation));
        Assert.That(lightState.ColorTemperature, Is.Null, nameof(lightState.ColorTemperature));
        Assert.That(lightState.Brightness, Is.Null, nameof(lightState.Brightness));
      }),
    };
    yield return new object[] {
      @"{""system"":{""get_sysinfo"":{""light_state"":{},""err_code"":0}}}",
      new Action<KL130LightState>(static lightState => {
        Assert.That(lightState.IsOn, Is.False, nameof(lightState.IsOn));
        Assert.That(lightState.Hue, Is.Null, nameof(lightState.Hue));
        Assert.That(lightState.Saturation, Is.Null, nameof(lightState.Saturation));
        Assert.That(lightState.ColorTemperature, Is.Null, nameof(lightState.ColorTemperature));
        Assert.That(lightState.Brightness, Is.Null, nameof(lightState.Brightness));
      }),
    };
    yield return new object[] {
      @"{""system"":{""get_sysinfo"":{""err_code"":0}}}",
      new Action<KL130LightState>(static lightState => {
        Assert.That(lightState.IsOn, Is.False, nameof(lightState.IsOn));
        Assert.That(lightState.Hue, Is.Null, nameof(lightState.Hue));
        Assert.That(lightState.Saturation, Is.Null, nameof(lightState.Saturation));
        Assert.That(lightState.ColorTemperature, Is.Null, nameof(lightState.ColorTemperature));
        Assert.That(lightState.Brightness, Is.Null, nameof(lightState.Brightness));
      }),
    };
  }

  [TestCaseSource(nameof(YieldTestCases_GetLightStateAsync))]
  public async Task GetLightStateAsync(string responseJson, Action<KL130LightState> assertLightState)
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

    using var device = new KL130(
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    KL130LightState lightState = default;

    Assert.DoesNotThrowAsync(async () => lightState = await device.GetLightStateAsync());

    assertLightState(lightState);
  }

  private static System.Collections.IEnumerable YieldTestCases_GetOnOffStateAsync()
  {
    yield return new object[] {
      @"{""system"":{""get_sysinfo"":{""sw_ver"":""x.y.z"",""model"":""KL130"",""light_state"":{""on_off"":1,""hue"":180,""saturation"":100,""color_temp"":5000,""brightness"":50},""err_code"":0}}}",
      true,
    };
    yield return new object[] {
      @"{""system"":{""get_sysinfo"":{""sw_ver"":""x.y.z"",""model"":""KL130"",""light_state"":{""on_off"":0,""dft_on_state"":{""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":5000,""brightness"":50}},""err_code"":0}}}",
      false,
    };
    yield return new object[] {
      @"{""system"":{""get_sysinfo"":{""light_state"":{},""err_code"":0}}}",
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

    using var device = new KL130(
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    bool? state = null;

    Assert.DoesNotThrowAsync(async () => state = await device.GetOnOffStateAsync());
    Assert.That(state, Is.EqualTo(expectedState), nameof(expectedState));
  }
}
