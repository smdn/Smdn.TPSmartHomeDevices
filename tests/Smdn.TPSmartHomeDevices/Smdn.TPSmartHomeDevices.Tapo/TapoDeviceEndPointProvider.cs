// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo;

[TestFixture]
public class TapoDeviceEndPointTests {
  [Test]
  public void Create_FromHostName_ArgumentNull()
    => Assert.Throws<ArgumentNullException>(() => TapoDeviceEndPoint.Create(host: null!));

  [Test]
  public async Task Create_FromHostName()
  {
    var endPoint = TapoDeviceEndPoint.Create(host: "localhost");

    Assert.IsNotNull(endPoint);
    Assert.That(endPoint, Is.AssignableTo<IDeviceEndPoint>(), nameof(endPoint));
    Assert.That(endPoint, Is.Not.AssignableTo<IDynamicDeviceEndPoint>(), nameof(endPoint));
    Assert.AreEqual(
      new DnsEndPoint("localhost", 80),
      await endPoint.GetEndPointAsync(default)
    );
    Assert.AreEqual(
      new DnsEndPoint("localhost", 80).ToString(),
      endPoint.ToString(),
      nameof(endPoint.ToString)
    );
  }

  [Test]
  public void Create_FromIPAddress_ArgumentNull()
    => Assert.Throws<ArgumentNullException>(() => TapoDeviceEndPoint.Create(ipAddress: null!));

  [Test]
  public async Task Create_FromIPAddress_V4()
  {
    var endPoint = TapoDeviceEndPoint.Create(ipAddress: IPAddress.Loopback);

    Assert.IsNotNull(endPoint);
    Assert.That(endPoint, Is.AssignableTo<IDeviceEndPoint>(), nameof(endPoint));
    Assert.That(endPoint, Is.Not.AssignableTo<IDynamicDeviceEndPoint>(), nameof(endPoint));
    Assert.AreEqual(
      new IPEndPoint(IPAddress.Parse("127.0.0.1"), 80),
      await endPoint.GetEndPointAsync(default)
    );
    Assert.AreEqual(
      new IPEndPoint(IPAddress.Parse("127.0.0.1"), 80).ToString(),
      endPoint.ToString(),
      nameof(endPoint.ToString)
    );
  }

  [Test]
  public async Task Create_FromIPAddress_V6()
  {
    var endPoint = TapoDeviceEndPoint.Create(ipAddress: IPAddress.IPv6Loopback);

    Assert.IsNotNull(endPoint);
    Assert.That(endPoint, Is.AssignableTo<IDeviceEndPoint>(), nameof(endPoint));
    Assert.That(endPoint, Is.Not.AssignableTo<IDynamicDeviceEndPoint>(), nameof(endPoint));
    Assert.AreEqual(
      new IPEndPoint(IPAddress.Parse("::1"), 80),
      await endPoint.GetEndPointAsync(default)
    );
    Assert.AreEqual(
      new IPEndPoint(IPAddress.Parse("::1"), 80).ToString(),
      endPoint.ToString(),
      nameof(endPoint.ToString)
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_IDeviceEndPoint_GetEndPointAsync_WithCancelledToken()
  {
    yield return new object[] { TapoDeviceEndPoint.Create(host: "localhost") };
    yield return new object[] { TapoDeviceEndPoint.Create(ipAddress: IPAddress.Loopback) };
    yield return new object[] { TapoDeviceEndPoint.Create(ipAddress: IPAddress.IPv6Loopback) };
  }

  [TestCaseSource(nameof(YieldTestCases_IDeviceEndPoint_GetEndPointAsync_WithCancelledToken))]
  public void IDeviceEndPoint_GetEndPointAsync_WithCancelledToken(IDeviceEndPoint endPoint)
  {
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    Assert.ThrowsAsync<TaskCanceledException>(
      async () => await endPoint.GetEndPointAsync(cts.Token)
    );
  }
}
