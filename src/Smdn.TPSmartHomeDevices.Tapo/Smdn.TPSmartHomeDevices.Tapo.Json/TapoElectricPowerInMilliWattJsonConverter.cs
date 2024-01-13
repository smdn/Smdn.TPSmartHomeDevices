// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices.Tapo.Json;

public sealed class TapoElectricPowerInMilliWattJsonConverter : TapoElectricPowerJsonConverter {
  private protected override decimal ToElectricPower(int value) => value * 0.001m;
}
