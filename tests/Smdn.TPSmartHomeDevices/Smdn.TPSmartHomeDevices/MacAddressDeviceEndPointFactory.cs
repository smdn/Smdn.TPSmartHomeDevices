// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Smdn.Net;
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

      public void Invalidate(PhysicalAddress address) { }
    }

    public StaticMacAddressDeviceEndPointFactory(IPAddress ipAddress)
      : base(new StaticAddressResolver(ipAddress))
    {
    }
  }

  private static readonly IPAddress TestIPAddress = IPAddress.Parse("192.0.2.255");
  private static readonly PhysicalAddress TestMacAddress = PhysicalAddress.Parse("00:00:5E:00:53:00");

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
    Assert.IsInstanceOf<IDynamicDeviceEndPoint>(endPoint, nameof(endPoint));
    Assert.DoesNotThrowAsync(async () => await endPoint.ResolveAsync());
    Assert.AreEqual(new IPEndPoint(TestIPAddress, port: 0), await endPoint.ResolveAsync());
  }

  [TestCase(0)]
  [TestCase(80)]
  [TestCase(9999)]
  public async Task Create_WithPort(int port)
  {
    using var factory = new StaticMacAddressDeviceEndPointFactory(TestIPAddress);

    var endPoint = factory.Create(PhysicalAddress.None, port);

    Assert.IsNotNull(endPoint, nameof(endPoint));
    Assert.IsInstanceOf<IDynamicDeviceEndPoint>(endPoint, nameof(endPoint));
    Assert.DoesNotThrowAsync(async () => await endPoint.ResolveAsync());
    Assert.AreEqual(new IPEndPoint(TestIPAddress, port), await endPoint.ResolveAsync());
  }

  [Test]
  public void Create_ArgumentNull_Address()
  {
    using var factory = new MacAddressDeviceEndPointFactory(resolver: MacAddressResolver.Null);

    Assert.Throws<ArgumentNullException>(() => factory.Create(address: null!));
  }

  private class ConcreteMacAddressDeviceEndPointFactory : MacAddressDeviceEndPointFactory {
    public ConcreteMacAddressDeviceEndPointFactory(
      IAddressResolver<PhysicalAddress, IPAddress> resolver,
      IServiceProvider? serviceProvider = null
    )
      : base(resolver, serviceProvider)
    {
    }
  }

  private class PseudoMacAddressResolver : IAddressResolver<PhysicalAddress, IPAddress> {
    private readonly IReadOnlyDictionary<PhysicalAddress, IPAddress> addressMap;
    private readonly List<IPAddress> invalidatedAddresses = new();

    public PseudoMacAddressResolver(IReadOnlyDictionary<PhysicalAddress, IPAddress> addressMap)
    {
      this.addressMap = addressMap ?? throw new ArgumentNullException(nameof(addressMap));
    }

    public ValueTask<IPAddress?> ResolveAsync(PhysicalAddress address, CancellationToken cancellationToken)
    {
      if (addressMap.TryGetValue(address, out var resolvedAddress)) {
        if (invalidatedAddresses.Contains(resolvedAddress))
          return default;

        return new ValueTask<IPAddress?>(resolvedAddress);
      }

      return default;
    }

    public void Invalidate(PhysicalAddress address)
    {
      if (addressMap.TryGetValue(address, out var resolvedAddress))
        invalidatedAddresses.Add(resolvedAddress);
    }
  }

  [Test]
  public async Task CreatedDeviceEndPoint_ToString()
  {
    using var factory = new MacAddressDeviceEndPointFactory(resolver: MacAddressResolver.Null);
    var endPoint = factory.Create(address: TestMacAddress);

    Assert.AreEqual(TestMacAddress.ToMacAddressString(), endPoint.ToString(), nameof(endPoint.ToString));
  }

  [Test]
  public async Task CreatedDeviceEndPoint_ResolveAsync_Resolved()
  {
    const int port = 80;

    using var factory = new ConcreteMacAddressDeviceEndPointFactory(
      resolver: new PseudoMacAddressResolver(
        new Dictionary<PhysicalAddress, IPAddress>() {
          { TestMacAddress, TestIPAddress }
        }
      )
    );

    var endPoint = factory.Create(address: TestMacAddress, port: port);

    Assert.IsNotNull(endPoint, nameof(endPoint));

    Assert.DoesNotThrowAsync(async () => await endPoint.ResolveAsync());
    Assert.AreEqual(new IPEndPoint(TestIPAddress, port), await endPoint.ResolveAsync());
  }

  [Test]
  public async Task CreatedDeviceEndPoint_ResolveAsync_NotResolved()
  {
    const int port = 80;

    using var factory = new ConcreteMacAddressDeviceEndPointFactory(
      resolver: new PseudoMacAddressResolver(
        new Dictionary<PhysicalAddress, IPAddress>() { }
      )
    );

    var endPoint = factory.Create(address: TestMacAddress, port: port);

    Assert.IsNotNull(endPoint, nameof(endPoint));

    Assert.DoesNotThrowAsync(async () => await endPoint.ResolveAsync());
    Assert.IsNull(await endPoint.ResolveAsync());
  }

  [Test]
  public async Task CreatedDeviceEndPoint_Invalidate()
  {
    const int port = 80;

    using var factory = new ConcreteMacAddressDeviceEndPointFactory(
      resolver: new PseudoMacAddressResolver(
        new Dictionary<PhysicalAddress, IPAddress>() {
          { TestMacAddress, TestIPAddress }
        }
      )
    );

    var endPoint = factory.Create(address: TestMacAddress, port: port);

    Assert.IsNotNull(endPoint, nameof(endPoint));
    Assert.IsInstanceOf<IDynamicDeviceEndPoint>(endPoint, nameof(endPoint));

    EndPoint? resolvedEndPointAddress = null;

    Assert.DoesNotThrowAsync(
      async () => resolvedEndPointAddress = await endPoint.ResolveAsync()
    );
    Assert.IsNotNull(resolvedEndPointAddress, nameof(resolvedEndPointAddress));
    Assert.AreEqual(resolvedEndPointAddress, new IPEndPoint(TestIPAddress, port), nameof(resolvedEndPointAddress));

    // invalidate
    Assert.DoesNotThrow(() => (endPoint as IDynamicDeviceEndPoint).Invalidate());

    EndPoint? resolvedEndPointAddressAfterInvalidation = null;

    Assert.DoesNotThrowAsync(
      async () => resolvedEndPointAddressAfterInvalidation = await endPoint.ResolveAsync()
    );
    Assert.IsNull(resolvedEndPointAddressAfterInvalidation, nameof(resolvedEndPointAddressAfterInvalidation));
  }
}
