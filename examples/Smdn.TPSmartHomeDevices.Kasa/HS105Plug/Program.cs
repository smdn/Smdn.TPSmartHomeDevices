﻿// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using Smdn.TPSmartHomeDevices.Kasa;

using var plug = new HS105(IPAddress.Parse("192.0.2.1"));

await plug.TurnOnAsync();

Console.WriteLine("Is on? {0}", await plug.GetOnOffStateAsync());

await Task.Delay(5000);

await plug.TurnOffAsync();

Console.WriteLine("Is on? {0}", await plug.GetOnOffStateAsync());
