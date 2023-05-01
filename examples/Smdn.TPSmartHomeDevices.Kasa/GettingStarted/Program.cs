using Smdn.TPSmartHomeDevices.Kasa;

// Creates device controller for Kasa KL130 multicolor light bulb.
using var bulb = new KL130(
  "192.0.2.255" // IP address currently associated to KL130
);

// Turns on the bulb, and set the color temperature and brightness.
await bulb.SetColorTemperatureAsync(colorTemperature: 5500, brightness: 80);

// Turns off the bulb
await bulb.TurnOffAsync();
