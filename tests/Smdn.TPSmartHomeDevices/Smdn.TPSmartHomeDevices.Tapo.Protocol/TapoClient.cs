// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

[TestFixture]
public partial class TapoClientTests {
  private class CredentialProvider : ITapoCredentialProvider {
    private readonly string base64UserNameSHA1Digest;
    private readonly string base64Password;

    public CredentialProvider(
      string base64UserNameSHA1Digest,
      string base64Password
    )
    {
      this.base64UserNameSHA1Digest = base64UserNameSHA1Digest;
      this.base64Password = base64Password;
    }

    public string GetBase64EncodedUserNameSHA1Digest(string host) => base64UserNameSHA1Digest;
    public string GetBase64EncodedPassword(string host) => base64Password;
  }

  private ServiceCollection? services;

  [OneTimeSetUp]
  public void SetUp()
  {
    services = new ServiceCollection();

    services.AddTapoBase64EncodedCredential(
      base64UserNameSHA1Digest: Convert.ToBase64String(Encoding.UTF8.GetBytes("user")),
      base64Password: Convert.ToBase64String(Encoding.UTF8.GetBytes("pass"))
    );
  }

  [Test]
  public void Ctor_ArgumentNullException_EndPoint()
  {
    Assert.Throws<ArgumentNullException>(() => {
      using var client = new TapoClient(
        endPoint: null!,
        serviceProvider: services?.BuildServiceProvider()
      );
    });
  }

  [Test]
  public void Ctor_ArgumentException_NoCredential()
  {
    Assert.Throws<ArgumentException>(() => {
      using var client = new TapoClient(
        endPoint: new IPEndPoint(IPAddress.Loopback, 80),
        credentialProvider: null,
        serviceProvider: null
      );
    });
  }

  [Test]
  public void Ctor_ArgumentException_NoCredentialViaIServiceProvider()
  {
    Assert.Throws<ArgumentException>(() => {
      using var client = new TapoClient(
        endPoint: new IPEndPoint(IPAddress.Loopback, 80),
        credentialProvider: null,
        serviceProvider: new ServiceCollection().BuildServiceProvider()
      );
    });
  }

  [Test]
  public void Ctor_CredentialProvider_ViaIServiceProvider()
  {
    Assert.DoesNotThrow(() => {
      using var client = new TapoClient(
        endPoint: new IPEndPoint(IPAddress.Loopback, 80),
        serviceProvider: services?.BuildServiceProvider()
      );
    });
  }

  [Test]
  public void Ctor_CredentialProvider_FromITapoCredentialProvider()
  {
    Assert.DoesNotThrow(() => {
      using var client = new TapoClient(
        endPoint: new IPEndPoint(IPAddress.Loopback, 80),
        credentialProvider: new CredentialProvider("user", "pass")
      );
    });
  }

  private class UnknownEndPoint : EndPoint {
    public override string ToString() => "device.unknown.test";
  }

  private static System.Collections.IEnumerable YieldTestCases_Ctor_EndPoint()
  {
    yield return new object[] { new IPEndPoint(IPAddress.Loopback, 0), new Uri("http://127.0.0.1/") };
    yield return new object[] { new IPEndPoint(IPAddress.Loopback, 8080), new Uri("http://127.0.0.1:8080/") };

    yield return new object[] { new DnsEndPoint("localhost", 0), new Uri("http://localhost/") };
    yield return new object[] { new DnsEndPoint("device.test", 8080), new Uri("http://device.test:8080/") };

    yield return new object[] { new UnknownEndPoint(), new Uri("http://device.unknown.test/") };
  }

  [TestCaseSource(nameof(YieldTestCases_Ctor_EndPoint))]
  public void Ctor_EndPoint(
    EndPoint endPoint,
    Uri expectedEndPointUri
  )
  {
    using var client = new TapoClient(
      endPoint: endPoint,
      serviceProvider: services?.BuildServiceProvider()
    );

    Assert.AreEqual(expectedEndPointUri, client.EndPointUri);
  }

  [Test]
  public void Ctor_EndPoint_DnsEndPoint()
  {
    using var client = new TapoClient(
      endPoint: new DnsEndPoint("localhost", 8080),
      serviceProvider: services?.BuildServiceProvider()
    );

    Assert.AreEqual(
      new Uri("http://localhost:8080/"),
      client.EndPointUri
    );
  }

  [Test]
  public void Dispose()
  {
    using var client = new TapoClient(
      endPoint: new IPEndPoint(IPAddress.Loopback, 80),
      serviceProvider: services?.BuildServiceProvider()
    );

    Assert.DoesNotThrow(client.Dispose, "Dispose not-disposed");
    Assert.DoesNotThrow(client.Dispose, "Dispose already-disposed");

    Assert.Throws<ObjectDisposedException>(() => Assert.IsNull(client.Session));
    Assert.Throws<ObjectDisposedException>(() => client.AuthenticateAsync());
    Assert.Throws<ObjectDisposedException>(() => client.CloseSession(), nameof(client.CloseSession));
  }

  [Test]
  public async Task Dispose_AuthenticatedState()
  {
    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      serviceProvider: services?.BuildServiceProvider()
    );

    await client.AuthenticateAsync();

    Assert.IsNotNull(client.Session, "Session before dispose");

    Assert.DoesNotThrow(client.Dispose, "Dispose not-disposed");
    Assert.DoesNotThrow(client.Dispose, "Dispose already-disposed");

    Assert.Throws<ObjectDisposedException>(() => Assert.IsNull(client.Session));
    Assert.Throws<ObjectDisposedException>(() => client.SendRequestAsync<GetDeviceInfoRequest, GetDeviceInfoResponse>());
    Assert.Throws<ObjectDisposedException>(() => client.AuthenticateAsync());
  }

  [Test]
  public async Task CloseSession_AuthenticatedState()
  {
    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      serviceProvider: services?.BuildServiceProvider()
    );

    await client.AuthenticateAsync();

    Assert.IsNotNull(client.Session, nameof(client.Session));

    Assert.DoesNotThrow(() => client.CloseSession(), $"{nameof(client.CloseSession)} #1");
    Assert.DoesNotThrow(() => client.CloseSession(), $"{nameof(client.CloseSession)} #2");

    Assert.IsNull(client.Session, nameof(client.Session));
  }

  [Test]
  public async Task CloseSession_NonAuthenticatedState()
  {
    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      serviceProvider: services?.BuildServiceProvider()
    );

    Assert.IsNull(client.Session, nameof(client.Session));

    Assert.DoesNotThrow(() => client.CloseSession(), $"{nameof(client.CloseSession)} #1");
    Assert.DoesNotThrow(() => client.CloseSession(), $"{nameof(client.CloseSession)} #2");

    Assert.IsNull(client.Session, nameof(client.Session));
  }
}
