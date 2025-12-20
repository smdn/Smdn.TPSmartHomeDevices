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
[NonParallelizable]
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

  [TestCaseSource(typeof(ConcreteTapoDeviceCommonTests), nameof(ConcreteTapoDeviceCommonTests.YieldTestCases_Ctor_ArgumentException))]
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
    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token",
      funcGeneratePassThroughResponse: (_, method, requestParams) => {
        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<GetCurrentPowerResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new(currentPower: currentPower),
          }
        );
      }
    );

    using var device = new P110M(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    var powerConsumption = await device.GetCurrentPowerConsumptionAsync();

    Assert.That(powerConsumption, Is.Not.Null);
    Assert.That(powerConsumption.Value, Is.EqualTo(expectedValueInWatt));
  }

  private readonly struct GetEnergyUsageResult {
    [JsonPropertyName("today_runtime")]
    public int? TodayRuntime { get; init; }

    [JsonPropertyName("month_runtime")]
    public int? MonthRuntime { get; init; }

    [JsonPropertyName("today_energy")]
    public int? TodayEnergy { get; init; }

    [JsonPropertyName("month_energy")]
    public int? MonthEnergy { get; init; }

    [JsonPropertyName("local_time")]
    public string? LocalTime { get; init; }

    // TODO: electricity_charge
    // [JsonPropertyName("electricity_charge")]

    [JsonPropertyName("current_power")]
    public int? CurrentPower { get; init; }
  }

  [Test]
  public async Task GetMonitoringDataAsync()
  {
    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token",
      funcGeneratePassThroughResponse: (_, method, requestParams) => {
        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<GetEnergyUsageResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new() {
              TodayRuntime = 41,
              MonthRuntime = 125,
              TodayEnergy = 13,
              MonthEnergy = 16,
              LocalTime = "2024-01-13 16:54:32",
              CurrentPower = 6900,
            },
          }
        );
      }
    );

    using var device = new P110M(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    var monitoringData = await device.GetMonitoringDataAsync();

    Assert.That(monitoringData, Is.Not.Null);
    Assert.That(monitoringData.TotalOperatingTimeToday, Is.EqualTo(TimeSpan.FromMinutes(41)));
    Assert.That(monitoringData.TotalOperatingTimeThisMonth, Is.EqualTo(TimeSpan.FromMinutes(125)));
    Assert.That(monitoringData.CumulativeEnergyUsageToday, Is.EqualTo(13.0m));
    Assert.That(monitoringData.CumulativeEnergyUsageThisMonth, Is.EqualTo(16.0m));
    Assert.That(monitoringData.TimeStamp, Is.EqualTo(new DateTime(2024, 1, 13, 16, 54, 32, DateTimeKind.Unspecified)));
    Assert.That(monitoringData.CurrentPowerConsumption, Is.EqualTo(6.9m));
  }
}
