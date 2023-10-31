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

    Assert.IsNotNull(client.Session);
    Assert.IsNull(client.Session!.Token);
    Assert.IsNotNull(client.Session!.SessionId);
    Assert.IsNotEmpty(client.Session.SessionId);
    Assert.AreNotEqual(DateTime.MaxValue, client.Session.ExpiresOn);
    Assert.IsFalse(client.Session.HasExpired);
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

    Assert.AreEqual(device.EndPointUri, ex!.EndPoint, nameof(ex.EndPoint));
    Assert.IsNull(ex.InnerException, nameof(ex.InnerException));

    Assert.IsNull(client.Session, nameof(client.Session));
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

    Assert.AreEqual(device.EndPointUri, ex!.EndPoint, nameof(ex.EndPoint));

    Assert.IsInstanceOf<HttpRequestException>(ex.InnerException, nameof(ex.InnerException));

    var httpRequestException = ex.InnerException as HttpRequestException;

    Assert.AreEqual(
      handshake2NonSuccessStatusCode,
      httpRequestException!.StatusCode,
      nameof(httpRequestException.StatusCode)
    );

    Assert.IsNull(client.Session, nameof(client.Session));
  }
}
