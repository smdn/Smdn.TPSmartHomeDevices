// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Smdn.Net.AddressResolution;

namespace Smdn.TPSmartHomeDevices;

[TestFixture]
public class MacAddressDeviceEndPointFactoryTests {
  private class IAddressResolverMacAddressDeviceEndPointFactory : MacAddressDeviceEndPointFactory {
    public IAddressResolverMacAddressDeviceEndPointFactory(
      IAddressResolver<PhysicalAddress, IPAddress> resolver
    )
      : base(resolver)
    {
    }
  }

  private class StaticMacAddressDeviceEndPointFactory : MacAddressDeviceEndPointFactory {
    private class StaticAddressResolver : IAddressResolver<PhysicalAddress, IPAddress> {
      private readonly IPAddress ipAddress;

      public StaticAddressResolver(IPAddress ipAddress)
      {
        this.ipAddress = ipAddress;
      }

      public ValueTask<IPAddress?> ResolveAsync(PhysicalAddress address, CancellationToken cancellationToken)
        => new(ipAddress);
    }

    public StaticMacAddressDeviceEndPointFactory(IPAddress ipAddress)
      : base(new StaticAddressResolver(ipAddress))
    {
    }
  }

  private static readonly IPAddress TestIPAddress = IPAddress.Parse("192.0.2.255");

  [Test]
  public void Ctor_ArgumentNull_IAddressResolver()
    => Assert.Throws<ArgumentNullException>(() => new IAddressResolverMacAddressDeviceEndPointFactory(resolver: null!));

  [Test]
  public void Ctor_ArgumentNull_MacAddressResolver()
    => Assert.Throws<ArgumentNullException>(() => new MacAddressDeviceEndPointFactory((MacAddressResolver)null!));

  [Test]
  public void Dispose()
  {
    using var factory = new MacAddressDeviceEndPointFactory(resolver: MacAddressResolver.Null);

    Assert.DoesNotThrow(factory.Dispose, "Dispose #1");
    Assert.DoesNotThrow(factory.Dispose, "Dispose #2");

    Assert.Throws<ObjectDisposedException>(() => factory.Create(PhysicalAddress.None));
    Assert.Throws<ObjectDisposedException>(() => factory.Create(PhysicalAddress.None, port: 39999));
  }

  [Test]
  public async Task Create()
  {
    using var factory = new StaticMacAddressDeviceEndPointFactory(TestIPAddress);

    var endPoint = factory.Create(PhysicalAddress.None);

    Assert.IsNotNull(endPoint, nameof(endPoint));
    Assert.IsFalse(endPoint.IsStaticEndPoint, nameof(endPoint.IsStaticEndPoint));
    Assert.DoesNotThrowAsync(async () => await endPoint.GetEndPointAsync());
    Assert.AreEqual(new IPEndPoint(TestIPAddress, port: 0), await endPoint.GetEndPointAsync());
  }

  [TestCase(0)]
  [TestCase(80)]
  [TestCase(9999)]
  public async Task Create_WithPort(int port)
  {
    using var factory = new StaticMacAddressDeviceEndPointFactory(TestIPAddress);

    var endPoint = factory.Create(PhysicalAddress.None, port);

    Assert.IsNotNull(endPoint, nameof(endPoint));
    Assert.IsFalse(endPoint.IsStaticEndPoint, nameof(endPoint.IsStaticEndPoint));
    Assert.DoesNotThrowAsync(async () => await endPoint.GetEndPointAsync());
    Assert.AreEqual(new IPEndPoint(TestIPAddress, port), await endPoint.GetEndPointAsync());
  }

  [Test]
  public void Create_ArgumentNull_Address()
  {
    using var factory = new MacAddressDeviceEndPointFactory(resolver: MacAddressResolver.Null);

    Assert.Throws<ArgumentNullException>(() => factory.Create(address: null!));
  }
}
