using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Smdn.TPSmartHomeDevices;

public static class DeviceEndPointFactoryServiceCollectionExtensions {
  /// <summary>
  /// Adds <see cref="IDeviceEndPointFactory{TAddress}"/> to create an <see cref="IDeviceEndPoint"/>
  /// that uses <typeparamref name="TAddress"/> as the address type to represent the device endpoint.
  /// </summary>
  /// <typeparam name="TAddress">The type that represents an address of device endpoint.</typeparam>
  /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
  /// <param name="endPointFactory">The <see cref="IDeviceEndPointFactory{TAddress}"/> that is added to services.</param>
  public static IServiceCollection AddDeviceEndPointFactory<TAddress>(
    this IServiceCollection services,
    IDeviceEndPointFactory<TAddress> endPointFactory
  ) where TAddress : notnull
  {
    if (services is null)
      throw new ArgumentNullException(nameof(services));
    if (endPointFactory is null)
      throw new ArgumentNullException(nameof(endPointFactory));

    services.TryAdd(ServiceDescriptor.Singleton(typeof(IDeviceEndPointFactory<TAddress>), endPointFactory));

    return services;
  }
}
