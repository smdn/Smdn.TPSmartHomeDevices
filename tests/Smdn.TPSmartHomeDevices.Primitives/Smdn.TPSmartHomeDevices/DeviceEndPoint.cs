// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices;

[TestFixture]
public class DeviceEndPointTests {
  private class MacAddressDeviceEndPointPseudoFactory : IDeviceEndPointFactory<PhysicalAddress> {
    public static readonly IPEndPoint ResolvedEndPoint = new(IPAddress.Loopback, 0);

    public IDeviceEndPoint Create(PhysicalAddress address)
      => new DynamicDeviceEndPoint(ResolvedEndPoint);
  }

  private class CustomAddress { }

  private class CustomAddressDeviceEndPointPseudoFactory : IDeviceEndPointFactory<CustomAddress> {
    public static readonly IPEndPoint ResolvedEndPoint = new(IPAddress.Loopback, 0);

    public IDeviceEndPoint Create(CustomAddress address)
      => new DynamicDeviceEndPoint(ResolvedEndPoint);
  }

  [Test]
  public void Create_FromHostName_ArgumentNull()
    => Assert.Throws<ArgumentNullException>(() => DeviceEndPoint.Create(host: null!));

  [Test]
  public async Task Create_FromHostName()
  {
    var endPoint = DeviceEndPoint.Create(host: "localhost");

    Assert.IsNotNull(endPoint);
    Assert.That(endPoint, Is.AssignableTo<IDeviceEndPoint>(), nameof(endPoint));
    Assert.That(endPoint, Is.Not.AssignableTo<IDynamicDeviceEndPoint>(), nameof(endPoint));
    Assert.AreEqual(
      new DnsEndPoint("localhost", 0),
      await endPoint.ResolveAsync(default)
    );
    Assert.AreEqual(
      new DnsEndPoint("localhost", 0).ToString(),
      endPoint.ToString(),
      nameof(endPoint.ToString)
    );
  }

  [Test]
  public void Create_FromIPAddress_ArgumentNull()
    => Assert.Throws<ArgumentNullException>(() => DeviceEndPoint.Create(ipAddress: null!));

  [Test]
  public async Task Create_FromIPAddress_V4()
  {
    var endPoint = DeviceEndPoint.Create(ipAddress: IPAddress.Loopback);

    Assert.IsNotNull(endPoint);
    Assert.That(endPoint, Is.AssignableTo<IDeviceEndPoint>(), nameof(endPoint));
    Assert.That(endPoint, Is.Not.AssignableTo<IDynamicDeviceEndPoint>(), nameof(endPoint));
    Assert.AreEqual(
      new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0),
      await endPoint.ResolveAsync(default)
    );
    Assert.AreEqual(
      new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0).ToString(),
      endPoint.ToString(),
      nameof(endPoint.ToString)
    );
  }

  [Test]
  public async Task Create_FromIPAddress_V6()
  {
    var endPoint = DeviceEndPoint.Create(ipAddress: IPAddress.IPv6Loopback);

    Assert.IsNotNull(endPoint);
    Assert.That(endPoint, Is.AssignableTo<IDeviceEndPoint>(), nameof(endPoint));
    Assert.That(endPoint, Is.Not.AssignableTo<IDynamicDeviceEndPoint>(), nameof(endPoint));
    Assert.AreEqual(
      new IPEndPoint(IPAddress.Parse("::1"), 0),
      await endPoint.ResolveAsync(default)
    );
    Assert.AreEqual(
      new IPEndPoint(IPAddress.Parse("::1"), 0).ToString(),
      endPoint.ToString(),
      nameof(endPoint.ToString)
    );
  }

  [Test]
  public async Task Create_FromMacAddress()
  {
    var endPoint = DeviceEndPoint.Create(address: PhysicalAddress.None, new MacAddressDeviceEndPointPseudoFactory());

    Assert.IsNotNull(endPoint);
    Assert.AreEqual(
      MacAddressDeviceEndPointPseudoFactory.ResolvedEndPoint,
      await endPoint.ResolveAsync(default)
    );
  }

  [Test]
  public void Create_FromMacAddress_ArgumentNull()
    => Assert.Throws<ArgumentNullException>(() => DeviceEndPoint.Create(address: null!, new MacAddressDeviceEndPointPseudoFactory()));

  [Test]
  public async Task Create_FromCustomAddressType()
  {
    var endPoint = DeviceEndPoint.Create(address: new CustomAddress(), new CustomAddressDeviceEndPointPseudoFactory());

    Assert.IsNotNull(endPoint);
    Assert.AreEqual(
      CustomAddressDeviceEndPointPseudoFactory.ResolvedEndPoint,
      await endPoint.ResolveAsync(default)
    );
  }

  [Test]
  public void Create_FromCustomAddressType_ArgumentNull()
    => Assert.Throws<ArgumentNullException>(() => DeviceEndPoint.Create(address: null!, new CustomAddressDeviceEndPointPseudoFactory()));

  private static System.Collections.IEnumerable YieldTestCases_IDeviceEndPoint_ResolveAsync_WithCancelledToken()
  {
    yield return new object[] { DeviceEndPoint.Create(host: "localhost") };
    yield return new object[] { DeviceEndPoint.Create(ipAddress: IPAddress.Loopback) };
    yield return new object[] { DeviceEndPoint.Create(ipAddress: IPAddress.IPv6Loopback) };
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
