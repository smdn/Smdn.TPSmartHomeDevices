// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Smdn.TPSmartHomeDevices.Tapo;

var services = new ServiceCollection();

services
  .AddLogging(
    static builder => builder
      .AddSimpleConsole(static options => {
        // By setting IncludeScopes to true enables the current endpoint information
        // to be output as a operation scope (see ILogger.BeginScope).
        options.IncludeScopes = true;
        options.SingleLine = true;
      })
      .AddFilter(static level => LogLevel.Trace <= level)
  );

using var plug = new P105(
  IPAddress.Parse("192.0.2.1"),
  "user@mail.test",
  "password",
  services.BuildServiceProvider()
);

await plug.TurnOnAsync();

await Task.Delay(2000);

await plug.TurnOffAsync();
