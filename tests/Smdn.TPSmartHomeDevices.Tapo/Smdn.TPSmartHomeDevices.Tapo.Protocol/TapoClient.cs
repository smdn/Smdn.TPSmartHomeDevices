// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

[TestFixture]
public partial class TapoClientTests {
  private ServiceCollection? services;
  private ITapoCredentialProvider? defaultCredentialProvider;
  private ITapoCredentialProvider? defaultKlapCredentialProvider;
  private IHttpClientFactory? defaultHttpClientFactory;

  private readonly struct NullResult { }

  [OneTimeSetUp]
  public void SetUp()
  {
    services = new ServiceCollection();

    services.AddTapoBase64EncodedCredential(
      base64UserNameSHA1Digest: Convert.ToBase64String(Encoding.UTF8.GetBytes("user")),
      base64Password: Convert.ToBase64String(Encoding.UTF8.GetBytes("pass"))
    );

    if (TestEnvironment.IsRunningOnGitHubActionsMacOSRunner) {
      services.AddTapoHttpClient(
        configureClient: client => {
          client.Timeout = TimeSpan.FromMinutes(1.0);
        }
      );
    }

    defaultCredentialProvider = services?.BuildServiceProvider()!.GetRequiredService<ITapoCredentialProvider>();
    defaultHttpClientFactory = services?.BuildServiceProvider()!.GetService<IHttpClientFactory>();

    var servicesForDefaultKlapCredentialProvider = new ServiceCollection();

    servicesForDefaultKlapCredentialProvider.AddTapoCredential(
      email: "user",
      password: "pass"
    );

    defaultKlapCredentialProvider = servicesForDefaultKlapCredentialProvider.BuildServiceProvider()!.GetRequiredService<ITapoCredentialProvider>();
  }

  [Test]
  public void Ctor_ArgumentNullException_EndPoint()
  {
    Assert.Throws<ArgumentNullException>(() => {
      using var client = new TapoClient(
        endPoint: null!
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

    yield return new object[] { new IPEndPoint(IPAddress.IPv6Loopback, 0), new Uri("http://[::1]/") };
    yield return new object[] { new IPEndPoint(IPAddress.IPv6Loopback, 8080), new Uri("http://[::1]:8080/") };

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
      endPoint: endPoint
    );

    Assert.That(client.EndPointUri, Is.EqualTo(expectedEndPointUri));
  }

  [Test]
  public void Ctor_EndPoint_DnsEndPoint()
  {
    using var client = new TapoClient(
      endPoint: new DnsEndPoint("localhost", 8080)
    );

    Assert.That(client.EndPointUri, Is.EqualTo(new Uri("http://localhost:8080/")));
  }

  [Test]
  public void Dispose()
  {
    using var client = new TapoClient(
      endPoint: new IPEndPoint(IPAddress.Loopback, 80)
    );

    Assert.DoesNotThrow(client.Dispose, "Dispose not-disposed");
    Assert.DoesNotThrow(client.Dispose, "Dispose already-disposed");

    Assert.Throws<ObjectDisposedException>(() => Assert.That(client.Session, Is.Null));

    Assert.ThrowsAsync<ObjectDisposedException>(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
    );
#pragma warning disable CA2012
    Assert.Throws<ObjectDisposedException>(
      () => client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
    );
#pragma warning restore CA2012
  }

  [Test]
  public async Task Dispose_AuthenticatedState()
  {
    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    await client.AuthenticateAsync(
      identity: null,
      credential: defaultCredentialProvider!
    );

    Assert.That(client.Session, Is.Not.Null, "Session before dispose");

    Assert.DoesNotThrow(client.Dispose, "Dispose not-disposed");
    Assert.DoesNotThrow(client.Dispose, "Dispose already-disposed");

    Assert.Throws<ObjectDisposedException>(() => Assert.That(client.Session, Is.Null));

#pragma warning disable CA2012
    Assert.Throws<ObjectDisposedException>(() => client.SendRequestAsync<GetDeviceInfoRequest, PassThroughResponse<NullResult>>());
    Assert.ThrowsAsync<ObjectDisposedException>(async () => await client.SendRequestAsync<GetDeviceInfoRequest, PassThroughResponse<NullResult>>());

    Assert.Throws<ObjectDisposedException>(
      () => client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
    );
    Assert.ThrowsAsync<ObjectDisposedException>(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
    );
#pragma warning restore CA2012
  }
}
