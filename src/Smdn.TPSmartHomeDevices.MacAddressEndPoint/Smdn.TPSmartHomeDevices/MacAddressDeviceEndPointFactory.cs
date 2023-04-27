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

  /// <summary>
  /// Initializes a new instance of the <see cref="MacAddressDeviceEndPointFactory"/> class.
  /// </summary>
  /// <param name="networkProfile">
  /// The <see cref="IPNetworkProfile"/> which specifying the network interface and network scan target addresses.
  /// This is used as necessary for network scan in address resolution.
  /// </param>
  /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
  public MacAddressDeviceEndPointFactory(
    IPNetworkProfile networkProfile,
    IServiceProvider? serviceProvider = null
  )
    : this(
      resolver: (IAddressResolver<PhysicalAddress, IPAddress>)new MacAddressResolver(
        networkProfile: networkProfile,
        serviceProvider: serviceProvider
      ),
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
  public MacAddressDeviceEndPointFactory(
    MacAddressResolverBase resolver,
    IServiceProvider? serviceProvider = null
  )
    : this(
      resolver: (IAddressResolver<PhysicalAddress, IPAddress>)(resolver ?? throw new ArgumentNullException(nameof(resolver))),
      serviceProvider: serviceProvider
    )
  {
  }

#pragma warning disable IDE0060
  protected MacAddressDeviceEndPointFactory(
    IAddressResolver<PhysicalAddress, IPAddress> resolver,
    IServiceProvider? serviceProvider = null
  )
#pragma warning restore IDE0060
  {
    this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
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

    (resolver as IDisposable)?.Dispose();
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
