// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using Smdn.TPSmartHomeDevices.Kasa;

using var plug = new HS105(IPAddress.Parse("192.0.2.255"));

// You can configure the timeout to the entire method call, including
// address resolution (when using MAC address) and connecting before the request,
// with specifying CancellationToken in the method argument.
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10.0));

await plug.TurnOnAsync(cancellationToken: cts.Token);
