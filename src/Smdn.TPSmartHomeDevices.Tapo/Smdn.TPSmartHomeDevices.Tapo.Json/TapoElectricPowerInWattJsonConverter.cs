// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices.Tapo.Json;

public sealed class TapoElectricPowerInWattJsonConverter : TapoElectricPowerJsonConverter {
  private protected override decimal ToElectricPower(int value) => value;
}
