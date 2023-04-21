// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Kasa;

[TestFixture]
public class KasaDeviceEndPointTests {
  [Test]
  public void Create_FromHostName_ArgumentNull()
    => Assert.Throws<ArgumentNullException>(() => KasaDeviceEndPoint.Create(host: null!));

  [Test]
  public async Task Create_FromHostName()
  {
    var endPoint = KasaDeviceEndPoint.Create(host: "localhost");

    Assert.IsNotNull(endPoint);
    Assert.That(endPoint, Is.AssignableTo<IDeviceEndPoint>(), nameof(endPoint));
    Assert.That(endPoint, Is.Not.AssignableTo<IDynamicDeviceEndPoint>(), nameof(endPoint));
    Assert.AreEqual(
      new DnsEndPoint("localhost", 9999),
      await endPoint.ResolveAsync(default)
    );
    Assert.AreEqual(
      new DnsEndPoint("localhost", 9999).ToString(),
      endPoint.ToString(),
      nameof(endPoint.ToString)
    );
  }

  [Test]
  public void Create_FromIPAddress_ArgumentNull()
    => Assert.Throws<ArgumentNullException>(() => KasaDeviceEndPoint.Create(ipAddress: null!));

  [Test]
  public async Task Create_FromIPAddress_V4()
  {
    var endPoint = KasaDeviceEndPoint.Create(ipAddress: IPAddress.Loopback);

    Assert.IsNotNull(endPoint);
    Assert.That(endPoint, Is.AssignableTo<IDeviceEndPoint>(), nameof(endPoint));
    Assert.That(endPoint, Is.Not.AssignableTo<IDynamicDeviceEndPoint>(), nameof(endPoint));
    Assert.AreEqual(
      new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999),
      await endPoint.ResolveAsync(default)
    );
    Assert.AreEqual(
      new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999).ToString(),
      endPoint.ToString(),
      nameof(endPoint.ToString)
    );
  }

  [Test]
  public async Task Create_FromIPAddress_V6()
  {
    var endPoint = KasaDeviceEndPoint.Create(ipAddress: IPAddress.IPv6Loopback);

    Assert.IsNotNull(endPoint);
    Assert.That(endPoint, Is.AssignableTo<IDeviceEndPoint>(), nameof(endPoint));
    Assert.That(endPoint, Is.Not.AssignableTo<IDynamicDeviceEndPoint>(), nameof(endPoint));
    Assert.AreEqual(
      new IPEndPoint(IPAddress.Parse("::1"), 9999),
      await endPoint.ResolveAsync(default)
    );
    Assert.AreEqual(
      new IPEndPoint(IPAddress.Parse("::1"), 9999).ToString(),
      endPoint.ToString(),
      nameof(endPoint.ToString)
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_IDeviceEndPoint_ResolveAsync_WithCancelledToken()
  {
    yield return new object[] { KasaDeviceEndPoint.Create(host: "localhost") };
    yield return new object[] { KasaDeviceEndPoint.Create(ipAddress: IPAddress.Loopback) };
    yield return new object[] { KasaDeviceEndPoint.Create(ipAddress: IPAddress.IPv6Loopback) };
  }

  [TestCaseSource(nameof(YieldTestCases_IDeviceEndPoint_ResolveAsync_WithCancelledToken))]
  public void IDeviceEndPoint_ResolveAsync_WithCancelledToken(IDeviceEndPoint endPoint)
  {
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    Assert.ThrowsAsync<TaskCanceledException>(
      async () => await endPoint.ResolveAsync(cts.Token)
    );
  }
}
