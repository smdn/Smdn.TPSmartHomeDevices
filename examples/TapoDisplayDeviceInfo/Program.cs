using System.Net;
using Smdn.Net;
using Smdn.TPSmartHomeDevices.Tapo;

var device = TapoDevice.Create(IPAddress.Parse("192.0.2.255"), "user@mail.test", "password");

var info = await device.GetDeviceInfoAsync();

const string format = "|{0,25} | {1,-40}|";

Console.WriteLine(format, nameof(info.MacAddress), info.MacAddress?.ToMacAddressString(':'));
Console.WriteLine(format, nameof(info.IPAddress), info.IPAddress);
Console.WriteLine(format, nameof(info.ModelName), info.ModelName);
Console.WriteLine(format, nameof(info.TypeName), info.TypeName);
Console.WriteLine(format, nameof(info.NickName), info.NickName);
Console.WriteLine(format, nameof(info.Language), info.Language);
Console.WriteLine(format, nameof(info.HardwareVersion), info.HardwareVersion);
Console.WriteLine(format, nameof(info.HardwareSpecifications), info.HardwareSpecifications);
Console.WriteLine(format, nameof(info.FirmwareVersion), info.FirmwareVersion);
Console.WriteLine(format, nameof(info.IsOn), info.IsOn);
Console.WriteLine(format, nameof(info.OnTimeDuration), info.OnTimeDuration);
Console.WriteLine(format, nameof(info.IsOverheated), info.IsOverheated);

if (info.HasGeolocationInfoSet) {
  Console.WriteLine(format, nameof(info.TimeZoneRegion), info.TimeZoneRegion);
  Console.WriteLine(format, nameof(info.TimeZoneOffset), info.TimeZoneOffset);
  Console.WriteLine(format, nameof(info.GeolocationLongitude), info.GeolocationLongitude);
  Console.WriteLine(format, nameof(info.GeolocationLatitude), info.GeolocationLatitude);
}

Console.WriteLine(format, nameof(info.NetworkSsid), info.NetworkSsid);

var position = (Console.CursorLeft, Console.CursorTop);

for (;;) {
  info = await device.GetDeviceInfoAsync();

  (Console.CursorLeft, Console.CursorTop) = position;

  Console.WriteLine(format, nameof(info.OnTimeDuration), info.OnTimeDuration);
  Console.WriteLine(format, nameof(info.NetworkSignalLevel), info.NetworkSignalLevel);
  Console.WriteLine(format, nameof(info.NetworkRssi), info.NetworkRssi);

  await Task.Delay(3000);
}
