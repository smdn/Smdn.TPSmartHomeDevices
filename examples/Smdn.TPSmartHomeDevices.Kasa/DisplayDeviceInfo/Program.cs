// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using System.Net.NetworkInformation;
using Smdn.TPSmartHomeDevices.Kasa;

using var device = KasaDevice.Create(IPAddress.Parse("192.0.2.255"));

var info = await device.GetDeviceInfoAsync();

const string format = "|{0,25} | {1,-40}|";

Console.WriteLine(format, nameof(info.MacAddress), ToMacAddressString(info?.MacAddress));
Console.WriteLine(format, nameof(info.ModelName), info?.ModelName);
Console.WriteLine(format, nameof(info.TypeName), info?.TypeName);
Console.WriteLine(format, nameof(info.Description), info?.Description);
Console.WriteLine(format, nameof(info.HardwareVersion), info?.HardwareVersion);
Console.WriteLine(format, nameof(info.FirmwareVersion), info?.FirmwareVersion);
Console.WriteLine(format, nameof(info.NetworkRssi), info?.NetworkRssi);

static string ToMacAddressString(PhysicalAddress? macAddress)
  => string.Join(
    ':',
    (macAddress?.GetAddressBytes() ?? Array.Empty<byte>()).Select(static b => b.ToString("X2", provider: null))
  );
