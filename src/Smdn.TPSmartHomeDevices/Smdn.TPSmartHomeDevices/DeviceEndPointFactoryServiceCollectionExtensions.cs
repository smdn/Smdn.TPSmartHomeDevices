using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Smdn.TPSmartHomeDevices;

public static class DeviceEndPointFactoryServiceCollectionExtensions {
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
