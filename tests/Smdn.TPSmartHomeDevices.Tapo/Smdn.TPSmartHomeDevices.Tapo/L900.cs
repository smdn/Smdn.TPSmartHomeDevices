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
public class L900Tests {
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

  [Test]
  public new void ToString()
    => ConcreteTapoDeviceCommonTests.TestToString<L900>();

  [TestCaseSource(typeof(ConcreteTapoDeviceCommonTests), nameof(ConcreteTapoDeviceCommonTests.YiledTestCases_Ctor_ArgumentException))]
  public void Ctor_ArgumentException(Type[] ctorParameterTypes, object?[] ctorParameters, Type? expectedExceptionType, string expectedParamName)
    => ConcreteTapoDeviceCommonTests.TestCtor_ArgumentException<L900>(ctorParameterTypes, ctorParameters, expectedExceptionType, expectedParamName);

  [TestCase(0, "brightness")]
  [TestCase(101, "brightness")]
  public void SetBrightnessAsync_ArgumentOutOfRange(
    int newBrightness,
    string expectedParamName
  )
  {
    using var device = new L900(
      deviceEndPoint: new ThrowExceptionDeviceEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

#pragma warning disable CA2012
    var ex = Assert.Throws<ArgumentOutOfRangeException>(
      () => device.SetBrightnessAsync(brightness: newBrightness)
    )!;
#pragma warning restore CA2012

    Assert.That(ex.ParamName, Is.EqualTo(expectedParamName), nameof(ex.ParamName));
    Assert.That(ex.ActualValue, Is.EqualTo(newBrightness), nameof(ex.ActualValue));
  }

  [Test]
  public async Task SetBrightnessAsync(
    [Values(1, 100)] int newBrightness
  )
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        Assert.That(method, Is.EqualTo("set_device_info"), "received request method");
        Assert.That(requestParams.GetProperty("device_on")!.GetBoolean(), Is.True);
        Assert.That(requestParams.GetProperty("lighting_effect")!.GetProperty("enable")!.GetInt32(), Is.EqualTo(0));
        Assert.That(requestParams.GetProperty("brightness")!.GetInt32(), Is.EqualTo(newBrightness));
        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<None>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = default,
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = new L900(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    await device.SetBrightnessAsync(brightness: newBrightness);
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
    using var device = new L900(
      deviceEndPoint: new ThrowExceptionDeviceEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

#pragma warning disable CA2012
    var ex = Assert.Throws<ArgumentOutOfRangeException>(
      () => device.SetColorAsync(hue: newHue, saturation: newSaturation, brightness: newBrightness)
    )!;
#pragma warning restore CA2012

    Assert.That(ex.ParamName, Is.EqualTo(expectedParamName), nameof(ex.ParamName));
    Assert.That(ex.ActualValue, Is.EqualTo(expectedActualValue), nameof(ex.ActualValue));
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
        Assert.That(method, Is.EqualTo("set_device_info"), "received request method");
        Assert.That(requestParams.GetProperty("device_on")!.GetBoolean(), Is.True);
        Assert.That(requestParams.GetProperty("lighting_effect")!.GetProperty("enable")!.GetInt32(), Is.EqualTo(0));
        Assert.That(requestParams.GetProperty("hue")!.GetInt32(), Is.EqualTo(newHue));
        Assert.That(requestParams.GetProperty("saturation")!.GetInt32(), Is.EqualTo(newSaturation));
        if (newBrightness.HasValue)
          Assert.That(requestParams.GetProperty("brightness")!.GetInt32(), Is.EqualTo(newBrightness));
        else
          Assert.That(requestParams.TryGetProperty("brightness", out var discard), Is.False);
        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<None>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = default,
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = new L900(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
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
    using var device = new L900(
      deviceEndPoint: new ThrowExceptionDeviceEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

#pragma warning disable CA2012
    var ex = Assert.Throws<ArgumentOutOfRangeException>(
      () => device.SetColorHueAsync(hue: newHue, brightness: newBrightness)
    )!;
#pragma warning restore CA2012

    Assert.That(ex.ParamName, Is.EqualTo(expectedParamName), nameof(ex.ParamName));
    Assert.That(ex.ActualValue, Is.EqualTo(expectedActualValue), nameof(ex.ActualValue));
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
        Assert.That(method, Is.EqualTo("set_device_info"), "received request method");
        Assert.That(requestParams.GetProperty("device_on")!.GetBoolean(), Is.True);
        Assert.That(requestParams.GetProperty("lighting_effect")!.GetProperty("enable")!.GetInt32(), Is.EqualTo(0));
        Assert.That(requestParams.GetProperty("hue")!.GetInt32(), Is.EqualTo(newHue));
        if (newBrightness.HasValue)
          Assert.That(requestParams.GetProperty("brightness")!.GetInt32(), Is.EqualTo(newBrightness));
        else
          Assert.That(requestParams.TryGetProperty("brightness", out var discard), Is.False);
        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<None>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = default,
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = new L900(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
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
    using var device = new L900(
      deviceEndPoint: new ThrowExceptionDeviceEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

#pragma warning disable CA2012
    var ex = Assert.Throws<ArgumentOutOfRangeException>(
      () => device.SetColorSaturationAsync(saturation: newSaturation, brightness: newBrightness)
    )!;
#pragma warning restore CA2012

    Assert.That(ex.ParamName, Is.EqualTo(expectedParamName), nameof(ex.ParamName));
    Assert.That(ex.ActualValue, Is.EqualTo(expectedActualValue), nameof(ex.ActualValue));
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
        Assert.That(method, Is.EqualTo("set_device_info"), "received request method");
        Assert.That(requestParams.GetProperty("device_on")!.GetBoolean(), Is.True);
        Assert.That(requestParams.GetProperty("lighting_effect")!.GetProperty("enable")!.GetInt32(), Is.EqualTo(0));
        Assert.That(requestParams.GetProperty("saturation")!.GetInt32(), Is.EqualTo(newSaturation));
        if (newBrightness.HasValue)
          Assert.That(requestParams.GetProperty("brightness")!.GetInt32(), Is.EqualTo(newBrightness));
        else
          Assert.That(requestParams.TryGetProperty("brightness", out var discard), Is.False);
        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<None>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = default,
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = new L900(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    await device.SetColorSaturationAsync(saturation: newSaturation, brightness: newBrightness);
  }
}
