// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

public static class TapoSessionProtocolSelectorServiceCollectionExtensions {
  /// <summary>
  /// Adds <see cref="Func{T, TResult}"/> that selects an <see cref="TapoSessionProtocol"/> for the <see cref="TapoDevice"/>, to <see cref="IServiceCollection"/>.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
  /// <param name="selectProtocol">
  /// The callback delegate that selects the <see cref="TapoSessionProtocol"/> value
  /// that represents the protocol used by <see cref="TapoDevice"/> for authentication and requests.
  /// </param>
  /// <seealso cref="TapoSessionProtocol"/>
  public static IServiceCollection AddTapoProtocolSelector(
    this IServiceCollection services,
    Func<TapoDevice, TapoSessionProtocol?> selectProtocol
  )
  {
    if (services is null)
      throw new ArgumentNullException(nameof(services));
    if (selectProtocol is null)
      throw new ArgumentNullException(nameof(selectProtocol));

    services.TryAdd(
      ServiceDescriptor.Singleton(
        typeof(TapoSessionProtocolSelector),
        new TapoSessionProtocolSelector.FuncSelector(selectProtocol)
      )
    );

    return services;
  }
}
