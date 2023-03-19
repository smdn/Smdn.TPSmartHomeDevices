// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

public static class TapoHttpClientFactoryServiceCollectionExtensions {
  public static IServiceCollection AddTapoHttpClient(
    this IServiceCollection services,
    Action<HttpClient>? configureClient = null
  )
  {
    if (services is null)
      throw new ArgumentNullException(nameof(services));

    services.TryAdd(
      ServiceDescriptor.Singleton(
        typeof(IHttpClientFactory),
        new TapoHttpClientFactory(
          configureClient: configureClient
        )
      )
    );

    return services;
  }
}
