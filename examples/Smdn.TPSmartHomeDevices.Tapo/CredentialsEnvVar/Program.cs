// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;

using Microsoft.Extensions.DependencyInjection;

using Smdn.TPSmartHomeDevices.Tapo;
using Smdn.TPSmartHomeDevices.Tapo.Credentials;

// Smdn.TPSmartHomeDevices.Tapo provides the service to retrieve
// credentials from environment variables via IServiceProvider.
//
// When adding an environment variable using with the
// AddTapoCredentialFromEnvironmentVariable() method, specify the name of
// the environment variable that holds the user name (email address of the
// Tapo account) and password used for authentication.
//
// The credentials added by this method can be used for both KLAP and
// 'secure pass through' authentication protocols.
var services1 = new ServiceCollection();

services1.AddTapoCredentialFromEnvironmentVariable(
  envVarUsername: "TAPO_USERNAME",
  envVarPassword: "TAPO_PASSWORD"
);

// In this example, plug1 obtains credentials from the environment
// variables TAPO_USERNAME and TAPO_PASSWORD.
using var plug1 = new P105(IPAddress.Parse("192.0.2.1"), services1.BuildServiceProvider());



// The above method requires that the user name and password be held
// in the environment variables in plain text.
//
// The AddTapoBase64EncodedKlapCredentialFromEnvironmentVariable() method
// can be used to hold a precomputed hash encoded in BASE64 instead of
// plain text.
//
// The credentials added by this method can be used only for KLAP
// authentication protocol.
var services2 = new ServiceCollection();

services2.AddTapoBase64EncodedKlapCredentialFromEnvironmentVariable(
  envVarBase64KlapLocalAuthHash: "TAPO_KLAP_LOCALAUTHHASH"
);

// In this example, plug2 obtains credentials from the environment
// variables TAPO_KLAP_LOCALAUTHHASH.
using var plug2 = new P105(IPAddress.Parse("192.0.2.2"), services2.BuildServiceProvider());



// The KLAP local auth hash can be computed by the following procedure.
//
//   KLAP local auth hash = SHA256(SHA1(username) + SHA1(password))
//
// To add to the environment variable, encode the resulting hash value
// with BASE64 encoding.
//
//   KLAP_LOCALAUTHHASH = BASE64(SHA256(SHA1(username) + SHA1(password)))
//
// Smdn.TPSmartHomeDevices.Tapo provides the TapoCredentials.TryComputeKlapLocalAuthHash()
// method, which can be used to calculate hash values.
var klapLocalAuthHash = new byte[32];

_ = TapoCredentials.TryComputeKlapLocalAuthHash(
  username: "user@mail.test"u8,
  password: "password"u8,
  destination: klapLocalAuthHash.AsSpan(),
  out var bytesWritten
);

Environment.SetEnvironmentVariable(
  "TAPO_KLAP_LOCALAUTHHASH",
  Convert.ToBase64String(klapLocalAuthHash)
);
