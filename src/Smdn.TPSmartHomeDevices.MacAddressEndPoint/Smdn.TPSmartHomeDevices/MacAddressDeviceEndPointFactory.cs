// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Smdn.Net;
using Smdn.Net.AddressResolution;

namespace Smdn.TPSmartHomeDevices;

/// <summary>
/// Provides a mechanism for creating <see cref="IDeviceEndPoint"/> that represents the device endpoint by using the MAC address.
/// </summary>
/// <seealso cref="IDeviceEndPointFactory{TAddress}" />
public class MacAddressDeviceEndPointFactory : IDeviceEndPointFactory<PhysicalAddress>, IDisposable {
  /// <summary>
  /// A concrete implementation of <see cref="IDynamicDeviceEndPoint"/>, which represents a device endpoint by MAC address.
  /// </summary>
  protected class MacAddressDeviceEndPoint : IDynamicDeviceEndPoint {
    private readonly IAddressResolver<PhysicalAddress, IPAddress> resolver;
    private readonly PhysicalAddress address;

    /// <summary>
    /// Initializes a new instance of the <see cref="MacAddressDeviceEndPoint"/> class.
    /// </summary>
    /// <param name="resolver">An <see cref="IAddressResolver{TAddress, TResolvedAddress}"/> that resolves <see cref="PhysicalAddress"/> to <see cref="IPAddress"/>.</param>
    /// <param name="address"><see cref="PhysicalAddress"/> that representing the MAC address of the device endpoint.</param>
    public MacAddressDeviceEndPoint(
      IAddressResolver<PhysicalAddress, IPAddress> resolver,
      PhysicalAddress address
    )
    {
      this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
      this.address = address ?? throw new ArgumentNullException(nameof(address));
    }

    public async ValueTask<EndPoint?> ResolveAsync(CancellationToken cancellationToken)
    {
      var resolvedAddress = await resolver.ResolveAsync(
        address: address,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      return resolvedAddress is null
        ? null
        : new IPEndPoint(
            address: resolvedAddress,
            port: 0
          );
    }

    public void Invalidate()
      => resolver.Invalidate(address);

    public override string ToString()
      => address.ToMacAddressString();
  }

  /*
   * instance members
   */
  private IAddressResolver<PhysicalAddress, IPAddress> resolver; // if null, it indicates a 'disposed' state.
  private readonly bool shouldDisposeResolver;

  /// <summary>
  /// Initializes a new instance of the <see cref="MacAddressDeviceEndPointFactory"/> class.
  /// </summary>
  /// <param name="networkProfile">
  /// The <see cref="IPNetworkProfile"/> which specifying the network interface and target addresses for address resolution.
  /// This is used as necessary for address resolution.
  /// </param>
  /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
  /// <seealso cref="MacAddressResolver"/>
  public MacAddressDeviceEndPointFactory(
    IPNetworkProfile networkProfile,
    IServiceProvider? serviceProvider = null
  )
    : this(
#pragma warning disable CA2000
      resolver: (IAddressResolver<PhysicalAddress, IPAddress>)new MacAddressResolver(
        networkProfile: networkProfile,
        serviceProvider: serviceProvider
      ),
#pragma warning restore CA2000
      shouldDisposeResolver: true,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="MacAddressDeviceEndPointFactory"/> class.
  /// </summary>
  /// <param name="resolver">
  /// The <see cref="MacAddressResolverBase"/> that resolves from a MAC address to a specific <see cref="IPAddress"/>.
  /// </param>
  /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
  [Obsolete("Use an overload that specifies the parameter `shouldDisposeResolver`.")]
  public MacAddressDeviceEndPointFactory(
    MacAddressResolverBase resolver,
    IServiceProvider? serviceProvider = null
  )
    : this(
      resolver: resolver ?? throw new ArgumentNullException(nameof(resolver)),
      shouldDisposeResolver: false,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="MacAddressDeviceEndPointFactory"/> class.
  /// </summary>
  /// <param name="resolver">
  /// The <see cref="MacAddressResolverBase"/> that resolves from a MAC address to a specific <see cref="IPAddress"/>.
  /// </param>
  /// <param name="shouldDisposeResolver">
  /// A value that indicates whether the <see cref="MacAddressResolverBase"/> passed from the <paramref name="resolver"/> should also be disposed when the instance is disposed.
  /// </param>
  /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
  /// <seealso cref="MacAddressResolverBase"/>
  public MacAddressDeviceEndPointFactory(
    MacAddressResolverBase resolver,
    bool shouldDisposeResolver,
    IServiceProvider? serviceProvider = null
  )
    : this(
      resolver: (IAddressResolver<PhysicalAddress, IPAddress>)(resolver ?? throw new ArgumentNullException(nameof(resolver))),
      shouldDisposeResolver: shouldDisposeResolver,
      serviceProvider: serviceProvider
    )
  {
  }

  [Obsolete("Use an overload that specifies the parameter `shouldDisposeResolver`.")]
#pragma warning disable IDE0060
  protected MacAddressDeviceEndPointFactory(
    IAddressResolver<PhysicalAddress, IPAddress> resolver,
    IServiceProvider? serviceProvider = null
  )
#pragma warning restore IDE0060
    : this(
      resolver: resolver,
      shouldDisposeResolver: false,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="MacAddressDeviceEndPointFactory"/> class.
  /// </summary>
  /// <param name="resolver">
  /// The <see cref="IAddressResolver{PhysicalAddress,IPAddress}"/> that resolves from a MAC address to a specific <see cref="IPAddress"/>.
  /// </param>
  /// <param name="shouldDisposeResolver">
  /// A value that indicates whether the <see cref="IAddressResolver{PhysicalAddress,IPAddress}"/> passed
  /// from the <paramref name="resolver"/> should also be disposed when the instance is disposed
  /// and <paramref name="resolver"/> is disposable.
  /// </param>
  /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
#pragma warning disable IDE0060
  protected MacAddressDeviceEndPointFactory(
    IAddressResolver<PhysicalAddress, IPAddress> resolver,
    bool shouldDisposeResolver,
    IServiceProvider? serviceProvider
  )
#pragma warning restore IDE0060
  {
    this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    this.shouldDisposeResolver = shouldDisposeResolver;
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!disposing)
      return;

    if (shouldDisposeResolver && resolver is IDisposable disposableResolver)
      disposableResolver.Dispose();

    resolver = null!; // mark as disposed
  }

  protected void ThrowIfDisposed()
  {
    if (resolver is null)
      throw new ObjectDisposedException(GetType().FullName);
  }

  public virtual IDeviceEndPoint Create(PhysicalAddress address)
  {
    ThrowIfDisposed();

    return new MacAddressDeviceEndPoint(
      resolver: resolver,
      address: address ?? throw new ArgumentNullException(nameof(address))
    );
  }
}
