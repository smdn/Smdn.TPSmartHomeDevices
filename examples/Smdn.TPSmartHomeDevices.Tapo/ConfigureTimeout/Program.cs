// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Smdn.TPSmartHomeDevices.Tapo;

var services = new ServiceCollection();

services.AddTapoCredential(
  email: "user@mail.test",
  password: "password"
);

// Configures the HttpClient to be created and used by TapoDevice and its derived classes.
services.AddTapoHttpClient(
  static client => {
    // You can configure the timeout period for HTTP requests by setting the HttpClient.Timeout property.
    client.Timeout = TimeSpan.FromSeconds(5.0);

    // You can also configure other HttpClient properties except BaseAddress.

    // The configuration for the BaseAddress property will be ignored.
    // This property will be overwritten at the start of the HTTP request.
    client.BaseAddress = new Uri("http://tapo.invalid/");
  }
);

using var plug = new P105("192.0.2.255", services.BuildServiceProvider());

// Here, the timeout period configured above will be used for HTTP requests.
// Note that the timeout period applies *only to each HTTP request*.
await plug.TurnOnAsync();

// If you want to configure the timeout to the entire method call, including
// address resolution (when using MAC address) and authentication before the request,
// specify CancellationToken in the method argument.
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10.0));

await plug.TurnOffAsync(cancellationToken: cts.Token);
