// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

namespace Smdn.TPSmartHomeDevices.Tapo;

/// <summary>
/// Holds and expresses the value that is the amount of electric power.
/// </summary>
public readonly struct ElectricEnergyAmount {
  /// <summary>
  /// Gets the amount of electric energy in unit of watt-hours [Wh].
  /// </summary>
  public decimal WattHour { get; }

  /// <summary>
  /// Gets the amount of electric energy in unit of kilowatt-hours [kWh].
  /// </summary>
  public decimal KiloWattHour => WattHour / 1000.0m;

  public ElectricEnergyAmount(decimal valueInWattHour)
  {
    WattHour = valueInWattHour;
  }
}
