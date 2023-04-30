// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Smdn.TPSmartHomeDevices.Tapo;
using Smdn.TPSmartHomeDevices.Tapo.Credentials;

// For basic usage, you can create Tapo device instance by specifying the
// email address and password used for authentication in the constructor.
using var plug0 = new P105(IPAddress.Parse("192.0.2.0"), "user@mail.test", "password");

// In addition to this, IServiceProvider can be used to specify the
// credentials to be used for authentication.
var services1 = new ServiceCollection();

services1.AddTapoCredential("user@mail.test", "password");

using var plug1 = new P105(IPAddress.Parse("192.0.2.1"), services1.BuildServiceProvider());

// You can also specify BASE64-encoded credentials instead of
// plaintext credentials.
var services2 = new ServiceCollection();

services2.AddTapoBase64EncodedCredential(
  base64UserNameSHA1Digest: "YjhlY2VjNWIzNjk0ZTVlNzE0YTYxMmNhZTZlZTJiNmExMjQ5ZmZmZQ==", // = BASE64(SHA1("user@mail.test"))
  base64Password: "cGFzc3dvcmQ=" // = BASE64("password")
);

using var plug2 = new P105(IPAddress.Parse("192.0.2.2"), services2.BuildServiceProvider());

// You can generate BASE64-encoded credentials using the TapoCredentials class.
//
// For user name (email), use TapoCredentials.ToBase64EncodedSHA1DigestString.
// For password, use TapoCredentials.ToBase64EncodedString.
var services3 = new ServiceCollection();

services3.AddTapoBase64EncodedCredential(
  base64UserNameSHA1Digest: TapoCredentials.ToBase64EncodedSHA1DigestString("user@mail.test"),
  base64Password: TapoCredentials.ToBase64EncodedString("password")
);

using var plug3 = new P105(IPAddress.Parse("192.0.2.3"), services2.BuildServiceProvider());

// Note:
//   As you can see from the examples above, the authentication in Tapo's
//   communication protocol uses a BASE64-encoded string for the password.
//
//   This means that passwords can be *easily* decoded by BASE64 decoding.
//   Therefore, consider the risks when writing passwords in your code.
//
//   If you want to provide credentials by other methods for security,
//   you can implement and use the ITapoCredentialProvider interface.
