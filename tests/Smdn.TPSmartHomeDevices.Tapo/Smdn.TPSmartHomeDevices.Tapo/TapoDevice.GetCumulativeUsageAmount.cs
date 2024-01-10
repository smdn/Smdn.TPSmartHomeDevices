// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

partial class TapoDeviceTests {
  private readonly struct GetCumulativeUsageResult {
    [JsonPropertyName("time_usage")]
    public Usage? TimeUsage { get; init; }

    [JsonPropertyName("power_usage")]
    public Usage? EnergyUsage { get; init; }

    public readonly struct Usage {
      [JsonPropertyName("today")]
      public int? Today { get; init; }

      [JsonPropertyName("past7")]
      public int? Past7Days { get; init; }

      [JsonPropertyName("past30")]
      public int? Past30Days { get; init; }
    }
  }

  [Test]
  public async Task GetCumulativeUsageAsync()
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        return (
          KnownErrorCodes.Success,
          new TapoPassThroughResponse<GetCumulativeUsageResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new() {
              TimeUsage = new() {
                Today = 1,
                Past7Days = 2,
                Past30Days = 3,
              },
              EnergyUsage = new() {
                Today = 4,
                Past7Days = 5,
                Past30Days = 6,
              },
            },
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    var (timeUsage, energyUsage) = await device.GetCumulativeUsageAsync();

    Assert.That(timeUsage, Is.Not.Null);
    Assert.That(timeUsage.Value.Today, Is.Not.Null);
    Assert.That(timeUsage.Value.Today.Value, Is.EqualTo(TimeSpan.FromMinutes(1)));
    Assert.That(timeUsage.Value.Past7Days, Is.Not.Null);
    Assert.That(timeUsage.Value.Past7Days.Value, Is.EqualTo(TimeSpan.FromMinutes(2)));
    Assert.That(timeUsage.Value.Past30Days, Is.Not.Null);
    Assert.That(timeUsage.Value.Past30Days.Value, Is.EqualTo(TimeSpan.FromMinutes(3)));

    Assert.That(energyUsage, Is.Not.Null);
    Assert.That(energyUsage.Value.Today, Is.Not.Null);
    Assert.That(energyUsage.Value.Today.Value.WattHour, Is.EqualTo(4.0m));
    Assert.That(energyUsage.Value.Past7Days, Is.Not.Null);
    Assert.That(energyUsage.Value.Past7Days.Value.WattHour, Is.EqualTo(5.0m));
    Assert.That(energyUsage.Value.Past30Days, Is.Not.Null);
    Assert.That(energyUsage.Value.Past30Days.Value.WattHour, Is.EqualTo(6.0m));

    Assert.That(energyUsage.Value.Today.Value.KiloWattHour, Is.EqualTo(0.004m));
    Assert.That(energyUsage.Value.Past7Days.Value.KiloWattHour, Is.EqualTo(0.005m));
    Assert.That(energyUsage.Value.Past30Days.Value.KiloWattHour, Is.EqualTo(0.006m));
  }

  [Test]
  public async Task GetCumulativeUsageAsync_MissingTimeUsageInResponse()
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        return (
          KnownErrorCodes.Success,
          new TapoPassThroughResponse<GetCumulativeUsageResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new() {
              TimeUsage = null, // missing property in response
              EnergyUsage = new() {
                Today = 0,
                Past7Days = 0,
                Past30Days = 0,
              },
            },
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    var (timeUsage, energyUsage) = await device.GetCumulativeUsageAsync();

    Assert.That(timeUsage, Is.Null);
    Assert.That(energyUsage, Is.Not.Null);
  }

  [TestCase(false, true, true)]
  [TestCase(true, false, true)]
  [TestCase(true, true, false)]
  public async Task GetCumulativeUsageAsync_MissingTimeUsagePropertyInResponse(
    bool respondValueForToday,
    bool respondValueForPast7days,
    bool respondValueForPast30days
  )
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        return (
          KnownErrorCodes.Success,
          new TapoPassThroughResponse<GetCumulativeUsageResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new() {
              TimeUsage = new() {
                Today = respondValueForToday ? 0 : null,
                Past7Days = respondValueForPast7days ? 0 : null,
                Past30Days = respondValueForPast30days ? 0 : null,
              },
              EnergyUsage = new() {
                Today = 0,
                Past7Days = 0,
                Past30Days = 0,
              },
            },
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    var (timeUsage, energyUsage) = await device.GetCumulativeUsageAsync();

    Assert.That(timeUsage, Is.Not.Null);
    Assert.That(timeUsage.Value.Today, respondValueForToday ? Is.Not.Null : Is.Null);
    Assert.That(timeUsage.Value.Past7Days, respondValueForPast7days ? Is.Not.Null : Is.Null);
    Assert.That(timeUsage.Value.Past30Days, respondValueForPast30days ? Is.Not.Null : Is.Null);

    Assert.That(energyUsage, Is.Not.Null);
  }

  [Test]
  public async Task GetCumulativeUsageAsync_MissingEnergyUsageInResponse()
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        return (
          KnownErrorCodes.Success,
          new TapoPassThroughResponse<GetCumulativeUsageResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new() {
              TimeUsage = new() {
                Today = 0,
                Past7Days = 0,
                Past30Days = 0,
              },
              EnergyUsage = null, // missing property in response
            },
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    var (timeUsage, energyUsage) = await device.GetCumulativeUsageAsync();

    Assert.That(timeUsage, Is.Not.Null);
    Assert.That(energyUsage, Is.Null);
  }

  [TestCase(false, true, true)]
  [TestCase(true, false, true)]
  [TestCase(true, true, false)]
  public async Task GetCumulativeUsageAsync_MissingEnergyUsagePropertyInResponse(
    bool respondValueForToday,
    bool respondValueForPast7days,
    bool respondValueForPast30days
  )
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        return (
          KnownErrorCodes.Success,
          new TapoPassThroughResponse<GetCumulativeUsageResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new() {
              TimeUsage = new() {
                Today = 0,
                Past7Days = 0,
                Past30Days = 0,
              },
              EnergyUsage = new() {
                Today = respondValueForToday ? 0 : null,
                Past7Days = respondValueForPast7days ? 0 : null,
                Past30Days = respondValueForPast30days ? 0 : null,
              },
            },
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    var (timeUsage, energyUsage) = await device.GetCumulativeUsageAsync();

    Assert.That(timeUsage, Is.Not.Null);

    Assert.That(energyUsage, Is.Not.Null);
    Assert.That(energyUsage.Value.Today, respondValueForToday ? Is.Not.Null : Is.Null);
    Assert.That(energyUsage.Value.Past7Days, respondValueForPast7days ? Is.Not.Null : Is.Null);
    Assert.That(energyUsage.Value.Past30Days, respondValueForPast30days ? Is.Not.Null : Is.Null);
  }

  [Test]
  public async Task GetCumulativeTimeUsageAsync()
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        return (
          KnownErrorCodes.Success,
          new TapoPassThroughResponse<GetCumulativeUsageResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new() {
              TimeUsage = new() {
                Today = 1,
                Past7Days = 60,
                Past30Days = 1440,
              },
              // This property will be just ignored
              EnergyUsage = new() {
                Today = -1,
                Past7Days = -1,
                Past30Days = -1,
              },
            },
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    var timeUsage = await device.GetCumulativeTimeUsageAsync();

    Assert.That(timeUsage, Is.Not.Null);
    Assert.That(timeUsage.Value.Today, Is.Not.Null);
    Assert.That(timeUsage.Value.Today.Value, Is.EqualTo(TimeSpan.FromMinutes(1)));
    Assert.That(timeUsage.Value.Past7Days, Is.Not.Null);
    Assert.That(timeUsage.Value.Past7Days.Value, Is.EqualTo(TimeSpan.FromHours(1)));
    Assert.That(timeUsage.Value.Past30Days, Is.Not.Null);
    Assert.That(timeUsage.Value.Past30Days.Value, Is.EqualTo(TimeSpan.FromDays(1)));
  }

  [Test]
  public async Task GetCumulativeEnergyUsageAsync()
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        return (
          KnownErrorCodes.Success,
          new TapoPassThroughResponse<GetCumulativeUsageResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new() {
              // This property will be just ignored
              TimeUsage = new() {
                Today = -1,
                Past7Days = -1,
                Past30Days = -1,
              },
              EnergyUsage = new() {
                Today = 1,
                Past7Days = 1_000,
                Past30Days = 1_000_000,
              },
            },
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    var energyUsage = await device.GetCumulativeEnergyUsageAsync();

    Assert.That(energyUsage, Is.Not.Null);

    Assert.That(energyUsage.Value.Today, Is.Not.Null);
    Assert.That(energyUsage.Value.Today.Value.WattHour, Is.EqualTo(1.0m));
    Assert.That(energyUsage.Value.Today.Value.KiloWattHour, Is.EqualTo(0.001m));

    Assert.That(energyUsage.Value.Past7Days, Is.Not.Null);
    Assert.That(energyUsage.Value.Past7Days.Value.WattHour, Is.EqualTo(1000.0m));
    Assert.That(energyUsage.Value.Past7Days.Value.KiloWattHour, Is.EqualTo(1.0m));

    Assert.That(energyUsage.Value.Past30Days, Is.Not.Null);
    Assert.That(energyUsage.Value.Past30Days.Value.WattHour, Is.EqualTo(1_000_000.0m));
    Assert.That(energyUsage.Value.Past30Days.Value.KiloWattHour, Is.EqualTo(1000.0m));
  }
}
