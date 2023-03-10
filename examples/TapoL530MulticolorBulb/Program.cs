using System.Net;
using Smdn.TPSmartHomeDevices.Tapo;

var bulb = new L530(IPAddress.Parse("192.0.2.255"), "user@mail.test", "password");

for (var temp = 2500; temp <= 6500; temp += 500) {
  await bulb.SetColorTemperatureAsync(colorTemperature: temp, brightness: 15);
  await Task.Delay(1000);
}

for (var hue = 0; hue <= 360; hue += 40) {
  await bulb.SetColorAsync(hue: hue, saturation: 100, brightness: 5);
  await Task.Delay(1000);
}

await bulb.TurnOffAsync();
