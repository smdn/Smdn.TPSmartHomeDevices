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
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        Assert.That(method, Is.EqualTo("set_device_info"), "received request method");
        Assert.That(requestParams.GetProperty("device_on")!.GetBoolean(), Is.True);
        return (
          KnownErrorCodes.Success,
          new SetDeviceInfoResponse<None>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = default,
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    await device.TurnOnAsync();
  }

  [Test]
  public async Task TurnOffAsync()
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        Assert.That(method, Is.EqualTo("set_device_info"), "received request method");
        Assert.That(requestParams.GetProperty("device_on")!.GetBoolean(), Is.False);
        return (
          KnownErrorCodes.Success,
          new SetDeviceInfoResponse<None>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = default,
          }
        );
      }
    };

    pseudoDevice.Start();

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
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        Assert.That(method, Is.EqualTo("set_device_info"), "received request method");
        Assert.That(requestParams.GetProperty("device_on")!.GetBoolean(), Is.EqualTo(newState));
        return (
          KnownErrorCodes.Success,
          new SetDeviceInfoResponse<None>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = default,
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    await device.SetOnOffStateAsync(newState);
  }
}
