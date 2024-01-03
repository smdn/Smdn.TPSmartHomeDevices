// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices;

[TestFixture]
public class IDeviceEndPointExtensionsTests {
  [Test]
  public void ResolveOrThrowAsync_DeviceEndPointNull()
  {
    IDeviceEndPoint? nullDeviceEndPoint = null;

    Assert.ThrowsAsync<ArgumentNullException>(
      async () => await nullDeviceEndPoint!.ResolveOrThrowAsync(defaultPort: 12345)
    );

#pragma warning disable CA2012
    Assert.Throws<ArgumentNullException>(
      () => nullDeviceEndPoint!.ResolveOrThrowAsync(defaultPort: 12345)
    );
#pragma warning restore CA2012
  }

  [TestCase(-1)]
  [TestCase(int.MinValue)]
  public void ResolveOrThrowAsync_DefaultPortNegative(int defaultPort)
  {
    Assert.ThrowsAsync<ArgumentOutOfRangeException>(
      async () => await new StaticDeviceEndPoint(new IPEndPoint(IPAddress.Loopback, 0)).ResolveOrThrowAsync(defaultPort: defaultPort)
    );

#pragma warning disable CA2012
    Assert.Throws<ArgumentOutOfRangeException>(
      () => new StaticDeviceEndPoint(new IPEndPoint(IPAddress.Loopback, 0)).ResolveOrThrowAsync(defaultPort: defaultPort)
    );
#pragma warning restore CA2012
  }

  private static System.Collections.IEnumerable YieldTestCases_ResolveEndPointAsync_ResolveToDefaultPort()
  {
    foreach (var defaultPort in new[] { 12345 /*specific port*/, 0 /* default port*/ } ) {
      yield return new object[] { new IPEndPoint(IPAddress.Loopback, 0), defaultPort, new IPEndPoint(IPAddress.Loopback, defaultPort) };
      yield return new object[] { new IPEndPoint(IPAddress.Loopback, 80), defaultPort, new IPEndPoint(IPAddress.Loopback, 80) };
      yield return new object[] { new DnsEndPoint("localhost", 0), defaultPort, new DnsEndPoint("localhost", defaultPort) };
      yield return new object[] { new DnsEndPoint("localhost", 80), defaultPort, new DnsEndPoint("localhost", 80) };
      yield return new object[] { new CustomEndPoint(port: 0), defaultPort, new CustomEndPoint(port: 0) }; // can not change port number
      yield return new object[] { new CustomEndPoint(port: 80), defaultPort, new CustomEndPoint(port: 80) }; // can not change port number
    }
  }

  [TestCaseSource(nameof(YieldTestCases_ResolveEndPointAsync_ResolveToDefaultPort))]
  public async Task ResolveOrThrowAsync_ResolveDefaultPort(EndPoint endPoint, int defaultPort, EndPoint expectedEndPoint)
  {
    var deviceEndPoint = new StaticDeviceEndPoint(endPoint);

    Assert.That(
      await deviceEndPoint.ResolveOrThrowAsync(defaultPort: defaultPort),
      Is.EqualTo(expectedEndPoint)
    );
  }

  [Test]
  public void ResolveOrThrowAsync_FailedToResolve()
  {
    var deviceEndPoint = new UnresolvedDeviceEndPoint();
    var ex = Assert.ThrowsAsync<DeviceEndPointResolutionException>(
      async () => await deviceEndPoint.ResolveOrThrowAsync(defaultPort: 80)
    );

    Assert.That(ex!.DeviceEndPoint, Is.Not.Null, nameof(ex.DeviceEndPoint));
    Assert.That(ex.DeviceEndPoint, Is.SameAs(deviceEndPoint), nameof(ex.DeviceEndPoint));
  }

  [Test]
  public void ResolveOrThrowAsync_FailedToResolve_IDynamicDeviceEndPoint()
  {
    var dynamicDeviceEndPoint = new DynamicDeviceEndPoint(endPoint: null);
    var ex = Assert.ThrowsAsync<DeviceEndPointResolutionException>(
      async () => await dynamicDeviceEndPoint.ResolveOrThrowAsync(defaultPort: 80)
    );

    Assert.That(dynamicDeviceEndPoint.HasInvalidated, Is.True, nameof(dynamicDeviceEndPoint.HasInvalidated));

    Assert.That(ex!.DeviceEndPoint, Is.Not.Null, nameof(ex.DeviceEndPoint));
    Assert.That(ex.DeviceEndPoint, Is.SameAs(dynamicDeviceEndPoint), nameof(ex.DeviceEndPoint));
  }

  [Test]
  public void ResolveEndPointAsync_CancellationRequested()
  {
    using var cts = new CancellationTokenSource();
    var deviceEndPoint = new ThrowExceptionDeviceEndPoint(); // must not thrown by ResolveAsync()

    cts.Cancel();

    Assert.ThrowsAsync<TaskCanceledException>(
      async () => await deviceEndPoint.ResolveOrThrowAsync(defaultPort: 80, cancellationToken: cts.Token)
    );
  }
}
