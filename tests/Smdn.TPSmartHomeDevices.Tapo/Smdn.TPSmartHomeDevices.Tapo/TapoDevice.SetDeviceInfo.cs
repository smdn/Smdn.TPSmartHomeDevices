// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

partial class TapoDeviceTests {
  [Test]
  public async Task TurnOnAsync()
  {
    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token",
      funcGeneratePassThroughResponse: (_, method, requestParams) => {
        Assert.That(method, Is.EqualTo("set_device_info"), "received request method");
        Assert.That(requestParams.GetProperty("device_on")!.GetBoolean(), Is.True);
        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<None>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = default,
          }
        );
      }
    );

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    await device.TurnOnAsync();
  }

  [Test]
  public async Task TurnOffAsync()
  {
    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token",
      funcGeneratePassThroughResponse: (_, method, requestParams) => {
        Assert.That(method, Is.EqualTo("set_device_info"), "received request method");
        Assert.That(requestParams.GetProperty("device_on")!.GetBoolean(), Is.False);
        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<None>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = default,
          }
        );
      }
    );

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    await device.TurnOffAsync();
  }

  [TestCase(true)]
  [TestCase(false)]
  public async Task SetOnOffStateAsync(bool newState)
  {
    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token",
      funcGeneratePassThroughResponse: (_, method, requestParams) => {
        Assert.That(method, Is.EqualTo("set_device_info"), "received request method");
        Assert.That(requestParams.GetProperty("device_on")!.GetBoolean(), Is.EqualTo(newState));
        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<None>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = default,
          }
        );
      }
    );

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    await device.SetOnOffStateAsync(newState);
  }
}
