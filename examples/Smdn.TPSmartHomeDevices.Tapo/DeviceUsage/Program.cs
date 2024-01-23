// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;

using Smdn.TPSmartHomeDevices.Tapo;

using var device = TapoDevice.Create(IPAddress.Parse("192.0.2.1"), "user@mail.test", "password");

// The TapoDevice.GetDeviceUsageAsync() method can be used to obtain the
// total operating times and the cumulative usage amount of electric
// energy of the device.
var (operatingTime, energyUsage) = await device.GetDeviceUsageAsync();

// The total operating time is taken as a TimeSpan value.
Console.WriteLine("Total operating time");
Console.WriteLine($"  Today:        {operatingTime?.Today?.TotalHours} hrs");
Console.WriteLine($"  Past 7 days:  {operatingTime?.Past7Days?.TotalHours} hrs");
Console.WriteLine($"  Past 30 days: {operatingTime?.Past30Days?.TotalHours} hrs");

// The cumulative usage amount of electric energy is taken
// as a decimal value in watt-hours.
Console.WriteLine("Cumulative energy usage");
Console.WriteLine($"  Today:        {energyUsage?.Today} Wh");
Console.WriteLine($"  Past 7 days:  {energyUsage?.Past7Days} Wh");
Console.WriteLine($"  Past 30 days: {energyUsage?.Past30Days} Wh");
