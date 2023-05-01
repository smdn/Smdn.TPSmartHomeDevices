// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Smdn.TPSmartHomeDevices.Kasa;

public static class KasaDeviceExceptionHandlerServiceCollectionExtensions {
  /// <summary>
  /// Adds <see cref="KasaDeviceExceptionHandler"/> to <see cref="IServiceCollection"/>.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
  /// <param name="exceptionHandler">A <see cref="KasaDeviceExceptionHandler"/> used for exception handling in <see cref="KasaDevice"/>.</param>
  public static IServiceCollection AddKasaDeviceExceptionHandler(
    this IServiceCollection services,
    KasaDeviceExceptionHandler exceptionHandler
  )
  {
    if (services is null)
      throw new ArgumentNullException(nameof(services));
    if (exceptionHandler is null)
      throw new ArgumentNullException(nameof(exceptionHandler));

    services.TryAdd(
      ServiceDescriptor.Singleton(typeof(KasaDeviceExceptionHandler), exceptionHandler)
    );

    return services;
  }
}
