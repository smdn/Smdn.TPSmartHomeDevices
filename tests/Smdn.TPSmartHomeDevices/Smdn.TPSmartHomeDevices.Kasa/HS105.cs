// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using Smdn.TPSmartHomeDevices.Kasa.Protocol;

namespace Smdn.TPSmartHomeDevices.Kasa;

[TestFixture]
public class HS105Tests {
  private static System.Collections.IEnumerable YiledTestCases_Ctor_ArgumentNull()
  {
    yield return new object[] {
      new TestDelegate(() => new HS105(hostName: null!)),
      "hostName"
    };
    yield return new object[] {
      new TestDelegate(() => new HS105(ipAddress: null!)),
      "ipAddress"
    };
    yield return new object[] {
      new TestDelegate(() => new HS105(deviceEndPointProvider: null!)),
      "deviceEndPointProvider"
    };
  }

  [TestCaseSource(nameof(YiledTestCases_Ctor_ArgumentNull))]
  public void Ctor_ArgumentNull(TestDelegate testAction, string expectedParamName)
  {
    var ex = Assert.Throws<ArgumentNullException>(testAction)!;

    Assert.AreEqual(expectedParamName, ex.ParamName, nameof(ex.ParamName));
  }

  [Test]
  public async Task TurnOnAsync()
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, request) => {
        Assert.AreEqual(
          @"{""system"":{""set_relay_state"":{""state"":1}}}",
          JsonSerializer.Serialize(request),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""system"":{""set_relay_state"":{""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new HS105(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider()
    );

    Assert.DoesNotThrowAsync(async () => await device.TurnOnAsync());
  }

  [Test]
  public async Task TurnOffAsync()
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, request) => {
        Assert.AreEqual(
          @"{""system"":{""set_relay_state"":{""state"":0}}}",
          JsonSerializer.Serialize(request),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""system"":{""set_relay_state"":{""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new HS105(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider()
    );

    Assert.DoesNotThrowAsync(async () => await device.TurnOffAsync());
  }

  [TestCase(true)]
  [TestCase(false)]
  public async Task SetOnOffStateAsync(bool newState)
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, request) => {
        Assert.AreEqual(
          @"{""system"":{""set_relay_state"":{""state"":" + (newState ? "1" : "0") + "}}}",
          JsonSerializer.Serialize(request),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""system"":{""set_relay_state"":{""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    using var device = new HS105(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider()
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
        Assert.AreEqual(
          @"{""system"":{""get_sysinfo"":{}}}",
          JsonSerializer.Serialize(request),
          nameof(request)
        );

        return JsonDocument.Parse(responseJson);
      }
    };

    pseudoDevice.Start();

    using var device = new HS105(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider()
    );

    Assert.DoesNotThrowAsync(async () => {
      Assert.AreEqual(expectedState, await device.GetOnOffStateAsync(), nameof(expectedState));
    });
  }
}
