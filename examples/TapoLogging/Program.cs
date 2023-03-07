using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Smdn.TPSmartHomeDevices.Tapo;

var services = new ServiceCollection();

services
  .AddLogging(
    static builder => builder
      .AddSimpleConsole(static options => options.SingleLine = true)
      .AddFilter(static level => LogLevel.Trace <= level)
  );

using var plug = new P105(
  IPAddress.Parse("192.0.2.255"),
  "user@mail.test",
  "password",
  services.BuildServiceProvider()
);

await plug.TurnOnAsync();

await Task.Delay(2000);

await plug.TurnOffAsync();
