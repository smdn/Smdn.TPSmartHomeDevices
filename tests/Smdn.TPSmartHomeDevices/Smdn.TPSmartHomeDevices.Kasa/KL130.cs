// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using Smdn.TPSmartHomeDevices.Kasa.Protocol;

namespace Smdn.TPSmartHomeDevices.Kasa;

[TestFixture]
public class KL130Tests {
  [TestCaseSource(typeof(ConcreteKasaDeviceCommonTests), nameof(ConcreteKasaDeviceCommonTests.YiledTestCases_Ctor_ArgumentException))]
  public void Ctor_ArgumentException(Type[] ctorParameterTypes, object?[] ctorParameters, Type? expectedExceptionType, string expectedParamName)
    => ConcreteKasaDeviceCommonTests.TestCtor_ArgumentException<KL130>(ctorParameterTypes, ctorParameters, expectedExceptionType, expectedParamName);

  private static System.Collections.IEnumerable YieldTestCases_TransitionPeriod()
  {
    yield return new object?[] { null, "0" };
    yield return new object?[] { TimeSpan.Zero, "0" };
    yield return new object?[] { TimeSpan.FromMilliseconds(1), "1" };
    yield return new object?[] { TimeSpan.FromSeconds(1), "1000" };
  }

  [Test]
  public async Task TurnOnAsync()
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, request) => {
        Assert.AreEqual(
          @"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":1,""transition_period"":0,""ignore_default"":0}}}",
          JsonSerializer.Serialize(request),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":1,""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":0,""brightness"":100,""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new KL130(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider()
    );

    Assert.DoesNotThrowAsync(async () => await device.TurnOnAsync());
  }

  [TestCaseSource(nameof(YieldTestCases_TransitionPeriod))]
  public async Task TurnOnAsync_WithTransitionPeriod(
    TimeSpan? transitionPeriod,
    string expectedTransitionPeriod
  )
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, request) => {
        Assert.AreEqual(
          @"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":1,""transition_period"":" + expectedTransitionPeriod + @",""ignore_default"":0}}}",
          JsonSerializer.Serialize(request),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":1,""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":0,""brightness"":100,""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new KL130(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider()
    );

    Assert.DoesNotThrowAsync(async () => await device.TurnOnAsync(transitionPeriod: transitionPeriod));
  }

  [Test]
  public async Task TurnOffAsync()
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, request) => {
        Assert.AreEqual(
          @"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":0,""transition_period"":0,""ignore_default"":0}}}",
          JsonSerializer.Serialize(request),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":0,""dft_on_state"":{""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":0,""brightness"":100},""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new KL130(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider()
    );

    Assert.DoesNotThrowAsync(async () => await device.TurnOffAsync());
  }

  [TestCaseSource(nameof(YieldTestCases_TransitionPeriod))]
  public async Task TurnOffAsync_WithTransitionPeriod(
    TimeSpan? transitionPeriod,
    string expectedTransitionPeriod
  )
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, request) => {
        Assert.AreEqual(
          @"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":0,""transition_period"":" + expectedTransitionPeriod + @",""ignore_default"":0}}}",
          JsonSerializer.Serialize(request),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":0,""dft_on_state"":{""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":0,""brightness"":100},""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new KL130(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider()
    );

    Assert.DoesNotThrowAsync(async () => await device.TurnOffAsync(transitionPeriod: transitionPeriod));
  }

  [TestCase(true)]
  [TestCase(false)]
  public async Task SetOnOffStateAsync(bool newState)
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, request) => {
        Assert.AreEqual(
          @"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":" + (newState ? "1" : "0") + @",""transition_period"":0,""ignore_default"":0}}}",
          JsonSerializer.Serialize(request),
          nameof(request)
        );

        return newState
          ? JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":1,""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":0,""brightness"":100,""err_code"":0}}}")
          : JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":0,""dft_on_state"":{""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":0,""brightness"":100},""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new KL130(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider()
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
    TimeSpan? transitionPeriod,
    string expectedTransitionPeriod
  )
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, request) => {
        Assert.AreEqual(
          @"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":" + (newState ? "1" : "0") + @",""transition_period"":"+ expectedTransitionPeriod + @",""ignore_default"":0}}}",
          JsonSerializer.Serialize(request),
          nameof(request)
        );

        return newState
          ? JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":1,""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":0,""brightness"":100,""err_code"":0}}}")
          : JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":0,""dft_on_state"":{""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":0,""brightness"":100},""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new KL130(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider()
    );

    Assert.DoesNotThrowAsync(async () => await device.SetOnOffStateAsync(newOnOffState: newState, transitionPeriod: transitionPeriod));
  }

  private static System.Collections.IEnumerable YieldTestCases_SetColorTemperatureAsync()
  {
    yield return new object?[] { 3000, null, null, null, "0" };
    yield return new object?[] { 5000, 1, null, "1", "0" };
    yield return new object?[] { 5000, 100, null, "100", "0" };
    yield return new object?[] { 8000, 100, TimeSpan.Zero, "100", "0" };
    yield return new object?[] { 8000, 100, TimeSpan.FromMilliseconds(1), "100", "1" };
    yield return new object?[] { 8000, 100, TimeSpan.FromSeconds(1), "100", "1000" };
  }

  [TestCaseSource(nameof(YieldTestCases_SetColorTemperatureAsync))]
  public async Task SetColorTemperatureAsync(
    int colorTemperature,
    int? brightness,
    TimeSpan? transitionPeriod,
    string expectedBrightness,
    string expectedTransitionPeriod
  )
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, request) => {
        Assert.AreEqual(
          @"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""color_temp"":" + colorTemperature.ToString() + (expectedBrightness is null ? string.Empty : @",""brightness"":" + expectedBrightness) + @",""transition_period"":"+ expectedTransitionPeriod + @",""on_off"":1,""ignore_default"":1}}}",
          JsonSerializer.Serialize(request),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":1,""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":" + colorTemperature.ToString() + (expectedBrightness is null ? string.Empty : @",""brightness"":" + expectedBrightness) + @",""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new KL130(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider()
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
    yield return new object?[] { 0, 0, null, null, null, "0" };
    yield return new object?[] { 360, 100, null, null, null, "0" };
    yield return new object?[] { 180, 50, 1, null, "1", "0" };
    yield return new object?[] { 180, 50, 100, null, "100", "0" };
    yield return new object?[] { 360, 100, 100, TimeSpan.Zero, "100", "0" };
    yield return new object?[] { 360, 100, 100, TimeSpan.FromMilliseconds(1), "100", "1" };
    yield return new object?[] { 360, 100, 100, TimeSpan.FromSeconds(1), "100", "1000" };
  }

  [TestCaseSource(nameof(YieldTestCases_SetColorAsync))]
  public async Task SetColorAsync(
    int hue,
    int saturation,
    int? brightness,
    TimeSpan? transitionPeriod,
    string expectedBrightness,
    string expectedTransitionPeriod
  )
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, request) => {
        Assert.AreEqual(
          @"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""hue"":" + hue.ToString() + @",""saturation"":" + saturation.ToString() + (expectedBrightness is null ? string.Empty : @",""brightness"":" + expectedBrightness) + @",""transition_period"":"+ expectedTransitionPeriod + @",""color_temp"":0,""on_off"":1,""ignore_default"":1}}}",
          JsonSerializer.Serialize(request),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""smartlife.iot.smartbulb.lightingservice"":{""transition_light_state"":{""on_off"":1,""mode"":""normal"",""hue"":" + hue.ToString() + @",""saturation"":" + saturation.ToString() + @",""color_temp"":0" + (expectedBrightness is null ? string.Empty : @",""brightness"":" + expectedBrightness) + @",""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new KL130(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider()
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
        Assert.IsTrue(lightState.IsOn, nameof(lightState.IsOn));
        Assert.IsNotNull(lightState.Hue, nameof(lightState.Hue));
        Assert.AreEqual(180, lightState.Hue!.Value, nameof(lightState.Hue));
        Assert.IsNotNull(lightState.Saturation, nameof(lightState.Saturation));
        Assert.AreEqual(100, lightState.Saturation!.Value, nameof(lightState.Saturation));
        Assert.IsNotNull(lightState.ColorTemperature, nameof(lightState.ColorTemperature));
        Assert.AreEqual(5000, lightState.ColorTemperature!.Value, nameof(lightState.ColorTemperature));
        Assert.IsNotNull(lightState.Brightness, nameof(lightState.Brightness));
        Assert.AreEqual(50, lightState.Brightness!.Value, nameof(lightState.Brightness));
      }),
    };
    yield return new object[] {
      @"{""system"":{""get_sysinfo"":{""sw_ver"":""x.y.z"",""model"":""KL130"",""light_state"":{""on_off"":0,""dft_on_state"":{""mode"":""normal"",""hue"":180,""saturation"":100,""color_temp"":5000,""brightness"":50}},""err_code"":0}}}",
      new Action<KL130LightState>(static lightState => {
        Assert.IsFalse(lightState.IsOn, nameof(lightState.IsOn));
        Assert.IsNull(lightState.Hue, nameof(lightState.Hue));
        Assert.IsNull(lightState.Saturation, nameof(lightState.Saturation));
        Assert.IsNull(lightState.ColorTemperature, nameof(lightState.ColorTemperature));
        Assert.IsNull(lightState.Brightness, nameof(lightState.Brightness));
      }),
    };
    yield return new object[] {
      @"{""system"":{""get_sysinfo"":{""light_state"":{},""err_code"":0}}}",
      new Action<KL130LightState>(static lightState => {
        Assert.IsFalse(lightState.IsOn, nameof(lightState.IsOn));
        Assert.IsNull(lightState.Hue, nameof(lightState.Hue));
        Assert.IsNull(lightState.Saturation, nameof(lightState.Saturation));
        Assert.IsNull(lightState.ColorTemperature, nameof(lightState.ColorTemperature));
        Assert.IsNull(lightState.Brightness, nameof(lightState.Brightness));
      }),
    };
    yield return new object[] {
      @"{""system"":{""get_sysinfo"":{""err_code"":0}}}",
      new Action<KL130LightState>(static lightState => {
        Assert.IsFalse(lightState.IsOn, nameof(lightState.IsOn));
        Assert.IsNull(lightState.Hue, nameof(lightState.Hue));
        Assert.IsNull(lightState.Saturation, nameof(lightState.Saturation));
        Assert.IsNull(lightState.ColorTemperature, nameof(lightState.ColorTemperature));
        Assert.IsNull(lightState.Brightness, nameof(lightState.Brightness));
      }),
    };
  }

  [TestCaseSource(nameof(YieldTestCases_GetLightStateAsync))]
  public async Task GetLightStateAsync(string responseJson, Action<KL130LightState> assertLightState)
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, request) => {
        Assert.AreEqual(
          @"{""system"":{""get_sysinfo"":{}}}",
          JsonSerializer.Serialize(request),
          nameof(request)
        );

        return JsonDocument.Parse(responseJson);
      }
    };

    pseudoDevice.Start();

    using var device = new KL130(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider()
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
        Assert.AreEqual(
          @"{""system"":{""get_sysinfo"":{}}}",
          JsonSerializer.Serialize(request),
          nameof(request)
        );

        return JsonDocument.Parse(responseJson);
      }
    };

    pseudoDevice.Start();

    using var device = new KL130(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider()
    );

    bool? state = null;

    Assert.DoesNotThrowAsync(async () => state = await device.GetOnOffStateAsync());
    Assert.AreEqual(expectedState, state, nameof(expectedState));
  }
}
