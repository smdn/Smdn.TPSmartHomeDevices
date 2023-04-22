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
        Assert.AreEqual("set_device_info", method, "received request method");
        Assert.IsTrue(requestParams.GetProperty("device_on")!.GetBoolean());
        return (
          KnownErrorCodes.Success,
          new SetDeviceInfoResponse() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new(),
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services.BuildServiceProvider()
    );

    await device.TurnOnAsync();
  }

  [Test]
  public async Task TurnOffAsync()
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        Assert.AreEqual("set_device_info", method, "received request method");
        Assert.IsFalse(requestParams.GetProperty("device_on")!.GetBoolean());
        return (
          KnownErrorCodes.Success,
          new SetDeviceInfoResponse() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new(),
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services.BuildServiceProvider()
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
        Assert.AreEqual("set_device_info", method, "received request method");
        Assert.AreEqual(newState, requestParams.GetProperty("device_on")!.GetBoolean());
        return (
          KnownErrorCodes.Success,
          new SetDeviceInfoResponse() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new(),
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services.BuildServiceProvider()
    );

    await device.SetOnOffStateAsync(newState);
  }
}
