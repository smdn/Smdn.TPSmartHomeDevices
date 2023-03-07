// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

[TestFixture]
public class L530Tests {
  private ServiceCollection? services;

  [OneTimeSetUp]
  public void SetUp()
  {
    services = new ServiceCollection();

    services.AddTapoBase64EncodedCredential(
      base64UserNameSHA1Digest: Convert.ToBase64String(Encoding.UTF8.GetBytes("user")),
      base64Password: Convert.ToBase64String(Encoding.UTF8.GetBytes("pass"))
    );
  }

  private static System.Collections.IEnumerable YiledTestCases_Ctor_ArgumentNull()
  {
    yield return new object[] {
      new TestDelegate(() => new L530(hostName: null!)),
      "hostName"
    };

    yield return new object[] {
      new TestDelegate(() => new L530(ipAddress: null!, email: "user@mail.test", password: "pass")),
      "ipAddress"
    };

    yield return new object[] {
      new TestDelegate(() => new L530(deviceEndPointProvider: null!)),
      "deviceEndPointProvider"
    };

    yield return new object[] {
      new TestDelegate(() => new L530(hostName: null!, email: "user@mail.test", password: "pass")),
      "hostName"
    };
    yield return new object[] {
      new TestDelegate(() => new L530(hostName: "localhost", email: null!, password: "pass")),
      "email"
    };
    yield return new object[] {
      new TestDelegate(() => new L530(hostName: "localhost", email: "user@mail.test", password: null!)),
      "password"
    };
  }

  [TestCaseSource(nameof(YiledTestCases_Ctor_ArgumentNull))]
  public void Ctor_ArgumentNull(TestDelegate testAction, string expectedParamName)
  {
    var ex = Assert.Throws<ArgumentNullException>(testAction)!;

    Assert.AreEqual(expectedParamName, ex.ParamName, nameof(ex.ParamName));
  }

  [TestCase(0, "brightness")]
  [TestCase(101, "brightness")]
  public void SetBrightnessAsync_ArgumentOutOfRange(
    int newBrightness,
    string expectedParamName
  )
  {
    using var device = new L530(
      deviceEndPointProvider: new ThrowExceptionDeviceEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    var ex = Assert.Throws<ArgumentOutOfRangeException>(
      () => device.SetBrightnessAsync(brightness: newBrightness)
    )!;

    Assert.AreEqual(expectedParamName, ex.ParamName, nameof(ex.ParamName));
    Assert.AreEqual(newBrightness, ex.ActualValue, nameof(ex.ActualValue));
  }

  [Test]
  public async Task SetBrightnessAsync(
    [Values(1, 100)] int newBrightness
  )
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        Assert.AreEqual("set_device_info", method, "received request method");
        Assert.IsTrue(requestParams.GetProperty("device_on")!.GetBoolean());
        Assert.AreEqual(newBrightness, requestParams.GetProperty("brightness")!.GetInt32());
        return (
          ErrorCode.Success,
          new SetDeviceInfoResponse() {
            ErrorCode = ErrorCode.Success,
            Result = new(),
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = new L530(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    await device.SetBrightnessAsync(brightness: newBrightness);
  }

  // TODO: color temperature
  [TestCase(4000, 0, "brightness", 0)]
  [TestCase(6500, 101, "brightness", 101)]
  public void SetColorTemperatureAsync_ArgumentOutOfRange(
    int newColorTemperature,
    int newBrightness,
    string expectedParamName,
    object expectedActualValue
  )
  {
    using var device = new L530(
      deviceEndPointProvider: new ThrowExceptionDeviceEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    var ex = Assert.Throws<ArgumentOutOfRangeException>(
      () => device.SetColorTemperatureAsync(colorTemperature: newColorTemperature, brightness: newBrightness)
    )!;

    Assert.AreEqual(expectedParamName, ex.ParamName, nameof(ex.ParamName));
    Assert.AreEqual(expectedActualValue, ex.ActualValue, nameof(ex.ActualValue));
  }

  [Test]
  public async Task SetColorTemperatureAsync(
    [Values(4000, 6500)] int newTemperature,
    [Values(null, 1, 100)] int? newBrightness
  )
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        Assert.AreEqual("set_device_info", method, "received request method");
        Assert.IsTrue(requestParams.GetProperty("device_on")!.GetBoolean());
        Assert.AreEqual(newTemperature, requestParams.GetProperty("color_temp")!.GetInt32());
        if (newBrightness.HasValue)
          Assert.AreEqual(newBrightness, requestParams.GetProperty("brightness")!.GetInt32());
        else
          Assert.IsFalse(requestParams.TryGetProperty("brightness", out var discard));
        return (
          ErrorCode.Success,
          new SetDeviceInfoResponse() {
            ErrorCode = ErrorCode.Success,
            Result = new(),
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = new L530(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    await device.SetColorTemperatureAsync(newTemperature, newBrightness);
  }

  [TestCase(-1, 0, 1, "hue", -1)]
  [TestCase(361, 100, 100, "hue", 361)]
  [TestCase(0, -1, 1, "saturation", -1)]
  [TestCase(360, 101, 100, "saturation", 101)]
  [TestCase(0, 0, 0, "brightness", 0)]
  [TestCase(0, 100, 101, "brightness", 101)]
  public void SetColorAsync_ArgumentOutOfRange(
    int newHue,
    int newSaturation,
    int newBrightness,
    string expectedParamName,
    object expectedActualValue
  )
  {
    using var device = new L530(
      deviceEndPointProvider: new ThrowExceptionDeviceEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    var ex = Assert.Throws<ArgumentOutOfRangeException>(
      () => device.SetColorAsync(hue: newHue, saturation: newSaturation, brightness: newBrightness)
    )!;

    Assert.AreEqual(expectedParamName, ex.ParamName, nameof(ex.ParamName));
    Assert.AreEqual(expectedActualValue, ex.ActualValue, nameof(ex.ActualValue));
  }

  [TestCase(0, 0, null)]
  [TestCase(0, 0, 1)]
  [TestCase(0, 0, 100)]
  [TestCase(360, 0, null)]
  [TestCase(360, 100, null)]
  [TestCase(360, 100, 100)]
  public async Task SetColorAsync(
    int newHue,
    int newSaturation,
    int? newBrightness
  )
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        Assert.AreEqual("set_device_info", method, "received request method");
        Assert.IsTrue(requestParams.GetProperty("device_on")!.GetBoolean());
        Assert.AreEqual(newHue, requestParams.GetProperty("hue")!.GetInt32());
        Assert.AreEqual(newSaturation, requestParams.GetProperty("saturation")!.GetInt32());
        if (newBrightness.HasValue)
          Assert.AreEqual(newBrightness, requestParams.GetProperty("brightness")!.GetInt32());
        else
          Assert.IsFalse(requestParams.TryGetProperty("brightness", out var discard));
        return (
          ErrorCode.Success,
          new SetDeviceInfoResponse() {
            ErrorCode = ErrorCode.Success,
            Result = new(),
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = new L530(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    await device.SetColorAsync(hue: newHue, saturation: newSaturation, brightness: newBrightness);
  }

  [TestCase(-1, 100, "hue", -1)]
  [TestCase(361, 100, "hue", 361)]
  [TestCase(0, 0, "brightness", 0)]
  [TestCase(0, 101, "brightness", 101)]
  public void SetColorHueAsync_ArgumentOutOfRange(
    int newHue,
    int newBrightness,
    string expectedParamName,
    object expectedActualValue
  )
  {
    using var device = new L530(
      deviceEndPointProvider: new ThrowExceptionDeviceEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    var ex = Assert.Throws<ArgumentOutOfRangeException>(
      () => device.SetColorHueAsync(hue: newHue, brightness: newBrightness)
    )!;

    Assert.AreEqual(expectedParamName, ex.ParamName, nameof(ex.ParamName));
    Assert.AreEqual(expectedActualValue, ex.ActualValue, nameof(ex.ActualValue));
  }

  [TestCase(0, null)]
  [TestCase(0, 1)]
  [TestCase(360, null)]
  [TestCase(360, 100)]
  public async Task SetColorHueAsync(
    int newHue,
    int? newBrightness
  )
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        Assert.AreEqual("set_device_info", method, "received request method");
        Assert.IsTrue(requestParams.GetProperty("device_on")!.GetBoolean());
        Assert.AreEqual(newHue, requestParams.GetProperty("hue")!.GetInt32());
        if (newBrightness.HasValue)
          Assert.AreEqual(newBrightness, requestParams.GetProperty("brightness")!.GetInt32());
        else
          Assert.IsFalse(requestParams.TryGetProperty("brightness", out var discard));
        return (
          ErrorCode.Success,
          new SetDeviceInfoResponse() {
            ErrorCode = ErrorCode.Success,
            Result = new(),
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = new L530(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    await device.SetColorHueAsync(hue: newHue, brightness: newBrightness);
  }

  [TestCase(-1, 100, "saturation", -1)]
  [TestCase(101, 100, "saturation", 101)]
  [TestCase(0, 0, "brightness", 0)]
  [TestCase(0, 101, "brightness", 101)]
  public void SetColorSaturationAsync_ArgumentOutOfRange(
    int newSaturation,
    int newBrightness,
    string expectedParamName,
    object expectedActualValue
  )
  {
    using var device = new L530(
      deviceEndPointProvider: new ThrowExceptionDeviceEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    var ex = Assert.Throws<ArgumentOutOfRangeException>(
      () => device.SetColorSaturationAsync(saturation: newSaturation, brightness: newBrightness)
    )!;

    Assert.AreEqual(expectedParamName, ex.ParamName, nameof(ex.ParamName));
    Assert.AreEqual(expectedActualValue, ex.ActualValue, nameof(ex.ActualValue));
  }

  [TestCase(0, null)]
  [TestCase(0, 1)]
  [TestCase(100, null)]
  [TestCase(100, 100)]
  public async Task SetColorSaturationAsync(
    int newSaturation,
    int? newBrightness
  )
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        Assert.AreEqual("set_device_info", method, "received request method");
        Assert.IsTrue(requestParams.GetProperty("device_on")!.GetBoolean());
        Assert.AreEqual(newSaturation, requestParams.GetProperty("saturation")!.GetInt32());
        if (newBrightness.HasValue)
          Assert.AreEqual(newBrightness, requestParams.GetProperty("brightness")!.GetInt32());
        else
          Assert.IsFalse(requestParams.TryGetProperty("brightness", out var discard));
        return (
          ErrorCode.Success,
          new SetDeviceInfoResponse() {
            ErrorCode = ErrorCode.Success,
            Result = new(),
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = new L530(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    await device.SetColorSaturationAsync(saturation: newSaturation, brightness: newBrightness);
  }
}
