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
      IAddressResolver<PhysicalAddress, IPAddress> resolver,
      bool shouldDisposeResolver
    )
      : base(resolver, shouldDisposeResolver: shouldDisposeResolver, serviceProvider: null)
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
      : base(new StaticAddressResolver(ipAddress), shouldDisposeResolver: false, serviceProvider: null)
    {
    }
  }

  internal class DisposableMacAddressResolver : MacAddressResolverBase {
    public bool HasDisposed { get; private set; }

    public override bool HasInvalidated => throw new NotImplementedException();

    public DisposableMacAddressResolver()
    {
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);

      HasDisposed = true;
    }

    protected override ValueTask<PhysicalAddress?> ResolveIPAddressToMacAddressAsyncCore(
      IPAddress ipAddress,
      CancellationToken cancellationToken
    ) => throw new NotImplementedException();

    protected override void InvalidateCore(IPAddress ipAddress)
      => throw new NotImplementedException();

    protected override ValueTask<IPAddress?> ResolveMacAddressToIPAddressAsyncCore(
      PhysicalAddress macAddress,
      CancellationToken cancellationToken
    ) => throw new NotImplementedException();

    protected override void InvalidateCore(PhysicalAddress macAddress)
      => throw new NotImplementedException();
  }

  private class NonDisposableAddressResolver : IAddressResolver<PhysicalAddress, IPAddress> {
    public NonDisposableAddressResolver()
    {
    }

    public ValueTask<IPAddress?> ResolveAsync(PhysicalAddress address, CancellationToken cancellationToken)
      => throw new NotImplementedException();

    public void Invalidate(PhysicalAddress address)
      => throw new NotImplementedException();
  }


  private static readonly IPAddress TestIPAddress = IPAddress.Parse("192.0.2.255");
  private static readonly PhysicalAddress TestMacAddress = PhysicalAddress.Parse("00:00:5E:00:53:00");

  [Test]
  public void Ctor_ArgumentNull_IAddressResolver()
    => Assert.Throws<ArgumentNullException>(() => new IAddressResolverMacAddressDeviceEndPointFactory(resolver: null!, shouldDisposeResolver: false));

  [Test]
  public void Ctor_ArgumentNull_MacAddressResolver([Values(true, false)] bool shouldDisposeResolver)
    => Assert.Throws<ArgumentNullException>(() => new MacAddressDeviceEndPointFactory((MacAddressResolverBase)null!, shouldDisposeResolver: shouldDisposeResolver));

  [TestCase(true)]
  [TestCase(false)]
  public void Ctor_ShouldDisposeResolver(bool shouldDisposeResolver)
  {
    using var resolver = new DisposableMacAddressResolver();
    using var factory = new MacAddressDeviceEndPointFactory(resolver, shouldDisposeResolver: shouldDisposeResolver);

    Assert.DoesNotThrow(factory.Dispose);

    if (shouldDisposeResolver)
      Assert.IsTrue(resolver.HasDisposed, nameof(resolver.HasDisposed));
    else
      Assert.IsFalse(resolver.HasDisposed, nameof(resolver.HasDisposed));
  }

  [Test]
  public void Dispose()
  {
    using var factory = new MacAddressDeviceEndPointFactory(
      resolver: MacAddressResolver.Null,
      shouldDisposeResolver: false,
      serviceProvider: null
    );

    Assert.DoesNotThrow(factory.Dispose, "Dispose #1");
    Assert.DoesNotThrow(factory.Dispose, "Dispose #2");

    Assert.Throws<ObjectDisposedException>(() => factory.Create(PhysicalAddress.None));
  }

  [TestCase(true)]
  [TestCase(false)]
  public void Dispose_ResolverIsNotDisposable(bool shouldDisposeResolver)
  {
    using var factory = new IAddressResolverMacAddressDeviceEndPointFactory(
      resolver: new NonDisposableAddressResolver(),
      shouldDisposeResolver: shouldDisposeResolver
    );

    Assert.DoesNotThrow(factory.Dispose, "Dispose #1");
    Assert.DoesNotThrow(factory.Dispose, "Dispose #2");

    Assert.Throws<ObjectDisposedException>(() => factory.Create(PhysicalAddress.None));
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

  [Test]
  public void Create_ArgumentNull_Address()
  {
    using var factory = new MacAddressDeviceEndPointFactory(
      resolver: MacAddressResolver.Null,
      shouldDisposeResolver: false,
      serviceProvider: null
    );

    Assert.Throws<ArgumentNullException>(() => factory.Create(address: null!));
  }

  private class ConcreteMacAddressDeviceEndPointFactory : MacAddressDeviceEndPointFactory {
    public ConcreteMacAddressDeviceEndPointFactory(
      IAddressResolver<PhysicalAddress, IPAddress> resolver,
      bool shouldDisposeResolver,
      IServiceProvider? serviceProvider = null
    )
      : base(resolver, shouldDisposeResolver, serviceProvider)
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
  public void CreatedDeviceEndPoint_ToString()
  {
    using var factory = new MacAddressDeviceEndPointFactory(
      resolver: MacAddressResolver.Null,
      shouldDisposeResolver: false,
      serviceProvider: null
    );
    var endPoint = factory.Create(address: TestMacAddress);

    Assert.AreEqual(TestMacAddress.ToMacAddressString(), endPoint.ToString(), nameof(endPoint.ToString));
  }

  [Test]
  public async Task CreatedDeviceEndPoint_ResolveAsync_Resolved()
  {
    using var factory = new ConcreteMacAddressDeviceEndPointFactory(
      resolver: new PseudoMacAddressResolver(
        new Dictionary<PhysicalAddress, IPAddress>() {
          { TestMacAddress, TestIPAddress }
        }
      ),
      shouldDisposeResolver: true
    );

    var endPoint = factory.Create(address: TestMacAddress);

    Assert.IsNotNull(endPoint, nameof(endPoint));

    Assert.DoesNotThrowAsync(async () => await endPoint.ResolveAsync());
    Assert.AreEqual(new IPEndPoint(TestIPAddress, 0), await endPoint.ResolveAsync());
  }

  [Test]
  public async Task CreatedDeviceEndPoint_ResolveAsync_NotResolved()
  {
    using var factory = new ConcreteMacAddressDeviceEndPointFactory(
      resolver: new PseudoMacAddressResolver(
        new Dictionary<PhysicalAddress, IPAddress>() { }
      ),
      shouldDisposeResolver: true
    );

    var endPoint = factory.Create(address: TestMacAddress);

    Assert.IsNotNull(endPoint, nameof(endPoint));

    Assert.DoesNotThrowAsync(async () => await endPoint.ResolveAsync());
    Assert.IsNull(await endPoint.ResolveAsync());
  }

  [Test]
  public void CreatedDeviceEndPoint_Invalidate()
  {
    using var factory = new ConcreteMacAddressDeviceEndPointFactory(
      resolver: new PseudoMacAddressResolver(
        new Dictionary<PhysicalAddress, IPAddress>() {
          { TestMacAddress, TestIPAddress }
        }
      ),
      shouldDisposeResolver: true
    );

    var endPoint = factory.Create(address: TestMacAddress);

    Assert.IsNotNull(endPoint, nameof(endPoint));
    Assert.IsInstanceOf<IDynamicDeviceEndPoint>(endPoint, nameof(endPoint));

    EndPoint? resolvedEndPointAddress = null;

    Assert.DoesNotThrowAsync(
      async () => resolvedEndPointAddress = await endPoint.ResolveAsync()
    );
    Assert.IsNotNull(resolvedEndPointAddress, nameof(resolvedEndPointAddress));
    Assert.AreEqual(resolvedEndPointAddress, new IPEndPoint(TestIPAddress, 0), nameof(resolvedEndPointAddress));

    // invalidate
    Assert.DoesNotThrow(() => (endPoint as IDynamicDeviceEndPoint)!.Invalidate());

    EndPoint? resolvedEndPointAddressAfterInvalidation = null;

    Assert.DoesNotThrowAsync(
      async () => resolvedEndPointAddressAfterInvalidation = await endPoint.ResolveAsync()
    );
    Assert.IsNull(resolvedEndPointAddressAfterInvalidation, nameof(resolvedEndPointAddressAfterInvalidation));
  }
}
