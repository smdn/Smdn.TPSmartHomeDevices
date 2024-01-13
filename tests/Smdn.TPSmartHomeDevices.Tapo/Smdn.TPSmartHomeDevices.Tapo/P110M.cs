// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

[TestFixture]
public class P110MTests {
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
    => ConcreteTapoDeviceCommonTests.TestToString<P110M>();

  [TestCaseSource(typeof(ConcreteTapoDeviceCommonTests), nameof(ConcreteTapoDeviceCommonTests.YiledTestCases_Ctor_ArgumentException))]
  public void Ctor_ArgumentException(Type[] ctorParameterTypes, object?[] ctorParameters, Type? expectedExceptionType, string expectedParamName)
    => ConcreteTapoDeviceCommonTests.TestCtor_ArgumentException<P110M>(ctorParameterTypes, ctorParameters, expectedExceptionType, expectedParamName);

  private readonly struct GetCurrentPowerResult(int currentPower) {
    [JsonPropertyName("current_power")]
    public int? CurrentPower { get; } = currentPower;
  }

  [TestCase(0, 0)]
  [TestCase(1, 1)]
  public async Task GetCurrentPowerConsumptionAsync(int currentPower, decimal expectedValueInWatt)
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        return (
          KnownErrorCodes.Success,
          new GetCurrentPowerResponse<GetCurrentPowerResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new(currentPower: currentPower),
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = new P110M(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    var powerConsumption = await device.GetCurrentPowerConsumptionAsync();

    Assert.That(powerConsumption, Is.Not.Null);
    Assert.That(powerConsumption.Value, Is.EqualTo(expectedValueInWatt));
  }
}
