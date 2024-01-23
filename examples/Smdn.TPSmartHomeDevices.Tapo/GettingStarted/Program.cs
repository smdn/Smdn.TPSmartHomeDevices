using Smdn.TPSmartHomeDevices.Tapo;

// Creates device controller for Tapo L530 multicolor light bulb.
using var bulb = new L530(
  "192.0.2.1",      // IP address currently assigned to L530
  "user@mail.test", // E-mail address for your Tapo account
  "password"        // Password for your Tapo account
);

// Turns on the bulb, and set the color temperature and brightness.
await bulb.SetColorTemperatureAsync(colorTemperature: 5500, brightness: 80);

// Turns off the bulb
await bulb.TurnOffAsync();
