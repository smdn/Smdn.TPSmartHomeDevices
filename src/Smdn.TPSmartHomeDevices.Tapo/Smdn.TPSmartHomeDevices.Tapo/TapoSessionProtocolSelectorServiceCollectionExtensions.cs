// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

public static class TapoSessionProtocolSelectorServiceCollectionExtensions {
  /// <summary>
  /// Adds <see cref="TapoSessionProtocolSelector"/> that selects an <see cref="TapoSessionProtocol"/> to <see cref="IServiceCollection"/>.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
  /// <param name="protocol">
  /// The protocol used by <see cref="TapoDevice"/> for authentication and requests.
  /// The protocol specified by this value will be used for all <see cref="TapoDevice"/>s.
  /// </param>
  /// <seealso cref="TapoSessionProtocol"/>
  public static IServiceCollection AddTapoProtocolSelector(
    this IServiceCollection services,
    TapoSessionProtocol protocol
  )
    => AddTapoProtocolSelector(
      services,
      new TapoSessionProtocolSelector.ConstantSelector(
        protocol switch {
          TapoSessionProtocol.SecurePassThrough => TapoSessionProtocol.SecurePassThrough,
          TapoSessionProtocol.Klap => TapoSessionProtocol.Klap,
          _ => throw new ArgumentException("undefined protocol", paramName: nameof(protocol)),
        }
      )
    );

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
    => AddTapoProtocolSelector(
      services,
      new TapoSessionProtocolSelector.FuncSelector(
        selectProtocol ?? throw new ArgumentNullException(nameof(selectProtocol))
      )
    );

  /// <summary>
  /// Adds <see cref="TapoSessionProtocolSelector"/> that selects an <see cref="TapoSessionProtocol"/> for the <see cref="TapoDevice"/>, to <see cref="IServiceCollection"/>.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
  /// <param name="selector">
  /// The concrete instance of <see cref="TapoSessionProtocolSelector"/> that selects the <see cref="TapoSessionProtocol"/> value
  /// that represents the protocol used by <see cref="TapoDevice"/> for authentication and requests.
  /// </param>
  /// <seealso cref="TapoSessionProtocol"/>
  public static IServiceCollection AddTapoProtocolSelector(
    this IServiceCollection services,
    TapoSessionProtocolSelector selector
  )
  {
    if (services is null)
      throw new ArgumentNullException(nameof(services));
    if (selector is null)
      throw new ArgumentNullException(nameof(selector));

    services.TryAdd(
      ServiceDescriptor.Singleton(
        typeof(TapoSessionProtocolSelector),
        selector
      )
    );

    return services;
  }
}
