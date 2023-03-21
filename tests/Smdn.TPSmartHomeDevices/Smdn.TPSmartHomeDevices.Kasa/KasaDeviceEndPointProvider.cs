// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Kasa;

[TestFixture]
public class KasaDeviceEndPointProviderTests {
  [Test]
  public void Create_FromHostName_ArgumentNull()
    => Assert.Throws<ArgumentNullException>(() => KasaDeviceEndPointProvider.Create(host: null!));

  [Test]
  public async Task Create_FromHostName()
  {
    var provider = KasaDeviceEndPointProvider.Create(host: "localhost");

    Assert.IsNotNull(provider);
    Assert.That(provider, Is.AssignableTo<IDeviceEndPointProvider>(), nameof(provider));
    Assert.That(provider, Is.Not.AssignableTo<IDynamicDeviceEndPointProvider>(), nameof(provider));
    Assert.AreEqual(
      new DnsEndPoint("localhost", 9999),
      await provider.GetEndPointAsync(default)
    );
    Assert.AreEqual(
      new DnsEndPoint("localhost", 9999).ToString(),
      provider.ToString(),
      nameof(provider.ToString)
    );
  }

  [Test]
  public void Create_FromIPAddress_ArgumentNull()
    => Assert.Throws<ArgumentNullException>(() => KasaDeviceEndPointProvider.Create(ipAddress: null!));

  [Test]
  public async Task Create_FromIPAddress_V4()
  {
    var provider = KasaDeviceEndPointProvider.Create(ipAddress: IPAddress.Loopback);

    Assert.IsNotNull(provider);
    Assert.That(provider, Is.AssignableTo<IDeviceEndPointProvider>(), nameof(provider));
    Assert.That(provider, Is.Not.AssignableTo<IDynamicDeviceEndPointProvider>(), nameof(provider));
    Assert.AreEqual(
      new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999),
      await provider.GetEndPointAsync(default)
    );
    Assert.AreEqual(
      new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999).ToString(),
      provider.ToString(),
      nameof(provider.ToString)
    );
  }

  [Test]
  public async Task Create_FromIPAddress_V6()
  {
    var provider = KasaDeviceEndPointProvider.Create(ipAddress: IPAddress.IPv6Loopback);

    Assert.IsNotNull(provider);
    Assert.That(provider, Is.AssignableTo<IDeviceEndPointProvider>(), nameof(provider));
    Assert.That(provider, Is.Not.AssignableTo<IDynamicDeviceEndPointProvider>(), nameof(provider));
    Assert.AreEqual(
      new IPEndPoint(IPAddress.Parse("::1"), 9999),
      await provider.GetEndPointAsync(default)
    );
    Assert.AreEqual(
      new IPEndPoint(IPAddress.Parse("::1"), 9999).ToString(),
      provider.ToString(),
      nameof(provider.ToString)
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_IDeviceEndPointProvider_GetEndPointAsync_WithCancelledToken()
  {
    yield return new object[] { KasaDeviceEndPointProvider.Create(host: "localhost") };
    yield return new object[] { KasaDeviceEndPointProvider.Create(ipAddress: IPAddress.Loopback) };
    yield return new object[] { KasaDeviceEndPointProvider.Create(ipAddress: IPAddress.IPv6Loopback) };
  }

  [TestCaseSource(nameof(YieldTestCases_IDeviceEndPointProvider_GetEndPointAsync_WithCancelledToken))]
  public void IDeviceEndPointProvider_GetEndPointAsync_WithCancelledToken(IDeviceEndPointProvider provider)
  {
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    Assert.ThrowsAsync<TaskCanceledException>(
      async () => await provider.GetEndPointAsync(cts.Token)
    );
  }
}
