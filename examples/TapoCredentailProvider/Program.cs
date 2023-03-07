using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Smdn.TPSmartHomeDevices.Tapo;

var services = new ServiceCollection();

// Configures the Tapo credentials using with ITapoCredentialProvider
services.AddTapoBase64EncodedCredential(
  base64UserNameSHA1Digest: "YjhlY2VjNWIzNjk0ZTVlNzE0YTYxMmNhZTZlZTJiNmExMjQ5ZmZmZQ==",
  base64Password: "cGFzc3dvcmQ="
);

// Note:
//   You can generate base64-encoded credentail strings using with the class
//   Smdn.TPSmartHomeDevices.Tapo.Protocol.TapoCredentialUtils.
//
//   For user name (email), use TapoCredentialUtils.ToBase64EncodedSHA1DigestString.
//   For password, use TapoCredentialUtils.ToBase64EncodedString.
//
//   The credentials also can be configured in plain text.
//   In that case, use AddTapoCredential() extension method instead.
//
//      services.AddTapoCredential(
//        userName: "user@mail.test",
//        password: "password"
//      );

using var plug = new P105("192.0.2.255", services.BuildServiceProvider());

await plug.TurnOnAsync();

await Task.Delay(2000);

await plug.TurnOffAsync();
