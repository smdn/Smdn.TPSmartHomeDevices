// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Smdn.TPSmartHomeDevices.Tapo;

public static class TapoDeviceExceptionHandlerServiceCollectionExtensions {
  /// <summary>
  /// Adds <see cref="TapoDeviceExceptionHandler"/> to <see cref="IServiceCollection"/>.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
  /// <param name="exceptionHandler">A <see cref="TapoDeviceExceptionHandler"/> used for exception handling in <see cref="TapoDevice"/>.</param>
  public static IServiceCollection AddTapoDeviceExceptionHandler(
    this IServiceCollection services,
    TapoDeviceExceptionHandler exceptionHandler
  )
  {
    if (services is null)
      throw new ArgumentNullException(nameof(services));
    if (exceptionHandler is null)
      throw new ArgumentNullException(nameof(exceptionHandler));

    services.TryAdd(
      ServiceDescriptor.Singleton(typeof(TapoDeviceExceptionHandler), exceptionHandler)
    );

    return services;
  }
}
