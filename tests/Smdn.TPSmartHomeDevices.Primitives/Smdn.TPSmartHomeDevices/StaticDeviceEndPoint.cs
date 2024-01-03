// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices;

[TestFixture]
public class StaticDeviceEndPointTests {
  private static System.Collections.IEnumerable YieldTestCases_EndPoints()
  {
    yield return new object[] { new IPEndPoint(IPAddress.Any, 80) };
    yield return new object[] { new DnsEndPoint("localhost", 80) };
    yield return new object[] { new CustomEndPoint(80) };
  }

  [TestCaseSource(nameof(YieldTestCases_EndPoints))]
  public void Ctor(EndPoint endPoint)
    => Assert.DoesNotThrow(() => new StaticDeviceEndPoint(endPoint));

  [Test]
  public void Ctor_EndPointNull()
    => Assert.Throws<ArgumentNullException>(() => new StaticDeviceEndPoint(endPoint: null!));

  [TestCaseSource(nameof(YieldTestCases_EndPoints))]
  public async Task ResolveAsync(EndPoint endPoint)
  {
    var deviceEndPoint = new StaticDeviceEndPoint(endPoint);

    var resolvedEndPoint = await deviceEndPoint.ResolveAsync();

    Assert.That(resolvedEndPoint, Is.SameAs(endPoint));
  }

  [Test]
  public void ResolveAsync_CancellationRequested()
  {
    using var cts = new CancellationTokenSource();
    var deviceEndPoint = new StaticDeviceEndPoint(new IPEndPoint(IPAddress.Any, 0));

    cts.Cancel();

    Assert.ThrowsAsync<TaskCanceledException>(
      async () => await deviceEndPoint.ResolveAsync(cts.Token)
    );
  }

  [TestCaseSource(nameof(YieldTestCases_EndPoints))]
  public void ToString(EndPoint endPoint)
  {
    var deviceEndPoint = new StaticDeviceEndPoint(endPoint);

    Assert.That(deviceEndPoint.ToString(), Is.EqualTo(endPoint.ToString()));
  }
}
