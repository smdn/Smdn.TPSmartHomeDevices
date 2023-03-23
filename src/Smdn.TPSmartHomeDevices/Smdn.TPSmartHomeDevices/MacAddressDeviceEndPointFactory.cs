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

public class MacAddressDeviceEndPointFactory : IDeviceEndPointFactory<PhysicalAddress>, IDisposable {
  protected class MacAddressDeviceEndPointProvider : IDynamicDeviceEndPointProvider {
    private readonly IAddressResolver<PhysicalAddress, IPAddress> resolver;
    private readonly PhysicalAddress address;
    private readonly int port;

    public MacAddressDeviceEndPointProvider(
      IAddressResolver<PhysicalAddress, IPAddress> resolver,
      PhysicalAddress address,
      int port
    )
    {
      this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
      this.address = address ?? throw new ArgumentNullException(nameof(address));
      this.port = port;
    }

    public async ValueTask<EndPoint?> GetEndPointAsync(CancellationToken cancellationToken)
    {
      var resolvedAddress = await resolver.ResolveAsync(
        address: address,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      return resolvedAddress is null
        ? null
        : new IPEndPoint(
            address: resolvedAddress,
            port: port
          );
    }

    public void InvalidateEndPoint()
      => resolver.Invalidate(address);

    public override string ToString()
      => address.ToMacAddressString();
  }

  /*
   * instance members
   */
  private IAddressResolver<PhysicalAddress, IPAddress>? resolver; // if null, it indicates a 'disposed' state.

  public MacAddressDeviceEndPointFactory(
    MacAddressResolverOptions? options = null,
    IServiceProvider? serviceProvider = null
  )
    : this(
      resolver: (IAddressResolver<PhysicalAddress, IPAddress>)MacAddressResolver.Create(
        options: options,
        serviceProvider: serviceProvider
      ),
      serviceProvider: serviceProvider
    )
  {
  }

  public MacAddressDeviceEndPointFactory(
    MacAddressResolver resolver,
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
    resolver = null; // mark as disposed
  }

  protected void ThrowIfDisposed()
  {
    if (resolver is null)
      throw new ObjectDisposedException(GetType().FullName);
  }

  public virtual IDeviceEndPointProvider Create(
    PhysicalAddress address,
    int port = 0
  )
  {
    ThrowIfDisposed();

    return new MacAddressDeviceEndPointProvider(
      resolver: resolver,
      address: address ?? throw new ArgumentNullException(nameof(address)),
      port: port
    );
  }
}
