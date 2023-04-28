// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using Smdn.TPSmartHomeDevices.Kasa;

using var bulb = new KL130(IPAddress.Parse("192.0.2.255"));

await bulb.SetColorTemperatureAsync(5000, 20, TimeSpan.FromSeconds(3.0));

await Task.Delay(5000);

for (var hue = 0; hue < 360; hue += 20) {
  await bulb.SetColorAsync(hue, 100, 100, TimeSpan.FromSeconds(0.0));

  await Task.Delay(2000);
}

await bulb.TurnOffAsync();
