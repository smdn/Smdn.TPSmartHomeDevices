// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using NUnit.Framework;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

partial class TapoClientTests {
  [Test]
  public void AuthenticateAsync_KLAP()
  {
    var device = CommonPseudoTapoDevice.Configure(
      funcGenerateKlapAuthHash: (_, _, authHash) => defaultKlapCredentialProvider!.GetKlapCredential(null).WriteLocalAuthHash(authHash.Span)
    );

    using var client = new TapoClient(
      endPoint: device.GetListenerEndPoint()
    );

    Assert.That(
      async () => await client.AuthenticateAsync(
        protocol: TapoSessionProtocol.Klap,
        identity: null,
        credential: defaultKlapCredentialProvider!
      ),
      Throws.Nothing
    );

    Assert.That(client.Session, Is.Not.Null);
    Assert.That(client.Session!.Token, Is.Null);
    Assert.That(client.Session!.SessionId, Is.Not.Null);
    Assert.That(client.Session.SessionId, Is.Not.Empty);
    Assert.That(client.Session.ExpiresOn, Is.Not.EqualTo(DateTime.MaxValue));
    Assert.That(client.Session.HasExpired, Is.False);
    Assert.That(client.Session.Protocol, Is.EqualTo(TapoSessionProtocol.Klap));
  }

  [Test]
  public void AuthenticateAsync_KLAP_Handshake1_AuthHashMismatch()
  {
    var device = CommonPseudoTapoDevice.Configure(
      funcGenerateKlapAuthHash: (_, _, authHash) => authHash.Span.Clear()
    );

    using var client = new TapoClient(
      endPoint: device.GetListenerEndPoint()
    );

    Assert.That(
      async () => await client.AuthenticateAsync(
        protocol: TapoSessionProtocol.Klap,
        identity: null,
        credential: defaultKlapCredentialProvider!
      ),
      Throws
        .TypeOf<TapoAuthenticationException>()
        .With
        .Property(nameof(TapoAuthenticationException.EndPoint))
        .EqualTo(device.EndPointUri)
        .And
        .InnerException.Null
    );

    Assert.That(client.Session, Is.Null, nameof(client.Session));
  }

  [Test]
  public void AuthenticateAsync_KLAP_Handshake2_HttpNonSuccessStatusCode()
  {
    const HttpStatusCode Handshake2NonSuccessStatusCode = HttpStatusCode.Unauthorized;

    var device = CommonPseudoTapoDevice.Configure(
      funcGenerateKlapAuthHash: (_, _, authHash) => defaultKlapCredentialProvider!.GetKlapCredential(null).WriteLocalAuthHash(authHash.Span),
      funcGenerateKlapHandshake2Response: _ => (Handshake2NonSuccessStatusCode, Handshake2NonSuccessStatusCode.ToString())
    );

    using var client = new TapoClient(
      endPoint: device.GetListenerEndPoint()
    );

    Assert.That(
      async () => await client.AuthenticateAsync(
        protocol: TapoSessionProtocol.Klap,
        identity: null,
        credential: defaultKlapCredentialProvider!
      ),
      Throws
        .TypeOf<TapoAuthenticationException>()
        .With
        .Property(nameof(TapoAuthenticationException.EndPoint))
        .EqualTo(device.EndPointUri)
        .And.InnerException.TypeOf<HttpRequestException>()
        .And.InnerException.Property(nameof(HttpRequestException.StatusCode)).EqualTo(Handshake2NonSuccessStatusCode)
    );

    Assert.That(client.Session, Is.Null, nameof(client.Session));
  }

  [TestCaseSource(nameof(YieldTestCases_AuthenticateAsync_IdentifyCredentialForIdentity))]
  public async Task AuthenticateAsync_KLAP_IdentifyCredentialForIdentity(
    ITapoCredentialProvider credentialProvider,
    ITapoCredentialIdentity identity,
    ITapoKlapCredential credentialForKlapAuthHash,
    Type? typeOfExpectedException
  )
  {
    var device = CommonPseudoTapoDevice.Configure(
      funcGenerateKlapAuthHash: (_, _, authHash) => credentialForKlapAuthHash.WriteLocalAuthHash(authHash.Span)
    );

    using var client = new TapoClient(
      endPoint: device.GetListenerEndPoint()
    );

    await Assert.ThatAsync(
      async () => await client.AuthenticateAsync(
        protocol: TapoSessionProtocol.Klap,
        identity: identity,
        credential: credentialProvider
      ),
      typeOfExpectedException is null ? Throws.Nothing : Throws.TypeOf(typeOfExpectedException)
    );

    Assert.That(client.Session, typeOfExpectedException is null ? Is.Not.Null : Is.Null);
  }
}
