using System.Net;
using Smdn.TPSmartHomeDevices.Tapo;

var lightstrip = new L900(IPAddress.Parse("192.0.2.255"), "user@mail.test", "password");

for (var hue = 0; hue <= 360; hue += 30) {
  await lightstrip.SetColorAsync(hue: hue, saturation: 30);

  for (var brightness = -100; brightness <= 100; brightness += 25) {
    await lightstrip.SetBrightnessAsync(brightness: Math.Max(Math.Abs(brightness), 1));

    await Task.Delay(500);
  }
}

await lightstrip.TurnOffAsync();
