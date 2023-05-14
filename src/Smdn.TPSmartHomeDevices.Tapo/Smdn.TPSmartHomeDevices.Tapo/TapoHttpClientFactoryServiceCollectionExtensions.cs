// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net.Http;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

public static class TapoHttpClientFactoryServiceCollectionExtensions {
  /// <summary>
  /// Adds <see cref="IHttpClientFactory"/> that creates an <see cref="HttpClient"/> configured for the Tapo client, to <see cref="IServiceCollection"/>.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
  /// <param name="configureClient">The callback delegate to configure the created <see cref="HttpClient"/>.</param>
  /// <seealso cref="TapoClient.DefaultHttpClientFactory"/>
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
