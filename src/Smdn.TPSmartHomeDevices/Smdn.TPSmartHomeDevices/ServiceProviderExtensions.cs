// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using Microsoft.Extensions.DependencyInjection;

namespace Smdn.TPSmartHomeDevices;

// public interface IDeviceEndPointFactory<TAddress> where TAddress : notnull {
//
internal static class ServiceProviderExtensions {
  public static IDeviceEndPointFactory<TAddress> GetDeviceEndPointFactory<TAddress>(
    this IServiceProvider serviceProvider
  ) where TAddress : notnull
    => serviceProvider is null
      ? throw new ArgumentNullException(nameof(serviceProvider))
      : serviceProvider.GetRequiredService<IDeviceEndPointFactory<TAddress>>();
}
