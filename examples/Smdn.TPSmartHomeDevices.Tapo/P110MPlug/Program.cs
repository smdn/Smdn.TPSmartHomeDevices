// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using Smdn.TPSmartHomeDevices.Tapo;

using var plug = new P110M(IPAddress.Parse("192.0.2.255"), "user@mail.test", "password");

await plug.TurnOnAsync();

Console.WriteLine("Is on? {0}", await plug.GetOnOffStateAsync());

var report = await plug.GetMonitoringDataAsync();

Console.WriteLine($"Operating time (today): {report.TotalOperatingTimeToday?.TotalHours:N1} [hour]");
Console.WriteLine($"Operating time (this month): {report.TotalOperatingTimeThisMonth?.TotalHours:N1} [hour]");

Console.WriteLine($"Cumulative energy usage (today): {report.CumulativeEnergyUsageToday:N1} [Wh]");
Console.WriteLine($"Cumulative energy usage (this month): {report.CumulativeEnergyUsageThisMonth:N1} [Wh]");

Console.WriteLine($"Current power consumption: {report.CurrentPowerConsumption:N1} [W]");

while (true) {
  await Task.Delay(TimeSpan.FromSeconds(5));

  var powerConsumption = await plug.GetCurrentPowerConsumptionAsync();

  Console.WriteLine($"Current power consumption: {powerConsumption:N1} [W]");
}
