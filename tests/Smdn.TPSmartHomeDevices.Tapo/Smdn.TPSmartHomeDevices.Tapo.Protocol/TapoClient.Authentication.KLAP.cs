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
  public async Task AuthenticateAsync_KLAP()
  {
    await using var device = new PseudoTapoDevice() {
      FuncGenerateKlapAuthHash = (_, _, authHash) =>
        _ = TapoCredentials.TryComputeKlapAuthHash(
          defaultCredentialProvider!.GetCredential(null),
          authHash.Span,
          out _
        )
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(
        protocol: TapoSessionProtocol.Klap,
        identity: null,
        credential: defaultCredentialProvider!
      )
    );

    Assert.That(client.Session, Is.Not.Null);
    Assert.That(client.Session!.Token, Is.Null);
    Assert.That(client.Session!.SessionId, Is.Not.Null);
    Assert.That(client.Session.SessionId, Is.Not.Empty);
    Assert.That(client.Session.ExpiresOn, Is.Not.EqualTo(DateTime.MaxValue));
    Assert.That(client.Session.HasExpired, Is.False);
  }

  [Test]
  public async Task AuthenticateAsync_KLAP_Handshake1_AuthHashMissmatch()
  {
    await using var device = new PseudoTapoDevice() {
      FuncGenerateKlapAuthHash = (_, _, authHash) => authHash.Span.Clear()
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync(
        protocol: TapoSessionProtocol.Klap,
        identity: null,
        credential: defaultCredentialProvider!
      )
    );

    Assert.That(ex!.EndPoint, Is.EqualTo(device.EndPointUri), nameof(ex.EndPoint));
    Assert.That(ex.InnerException, Is.Null, nameof(ex.InnerException));

    Assert.That(client.Session, Is.Null, nameof(client.Session));
  }

  [Test]
  public async Task AuthenticateAsync_KLAP_Handshake2_HttpNonSuccessStatusCode()
  {
    const HttpStatusCode handshake2NonSuccessStatusCode = HttpStatusCode.Unauthorized;

    await using var device = new PseudoTapoDevice() {
      FuncGenerateKlapAuthHash = (_, _, authHash) =>
        _ = TapoCredentials.TryComputeKlapAuthHash(
          defaultCredentialProvider!.GetCredential(null),
          authHash.Span,
          out _
        ),
      FuncGenerateKlapHandshake2Response = _ => (handshake2NonSuccessStatusCode, handshake2NonSuccessStatusCode.ToString()),
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync(
        protocol: TapoSessionProtocol.Klap,
        identity: null,
        credential: defaultCredentialProvider!
      )
    );

    Assert.That(ex!.EndPoint, Is.EqualTo(device.EndPointUri), nameof(ex.EndPoint));

    Assert.That(ex.InnerException, Is.InstanceOf<HttpRequestException>(), nameof(ex.InnerException));

    var httpRequestException = ex.InnerException as HttpRequestException;

    Assert.That(
      httpRequestException!.StatusCode,
      Is.EqualTo(handshake2NonSuccessStatusCode),
      nameof(httpRequestException.StatusCode)
    );

    Assert.That(client.Session, Is.Null, nameof(client.Session));
  }
}
