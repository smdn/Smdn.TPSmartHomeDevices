// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Smdn.TPSmartHomeDevices.Tapo;

public static class TapoHttpClientFactoryServiceCollectionExtensions {
  public static IHttpClientBuilder AddTapoHttpClient(
    this IServiceCollection services,
    string name,
    Action<IServiceProvider, HttpClient>? configureClient = null
  )
    => AddTapoHttpClient(
      services: services,
      name: name,
      timeout: Timeout.InfiniteTimeSpan,
      configureClient: configureClient
    );

  public static IHttpClientBuilder AddTapoHttpClient(
    this IServiceCollection services,
    string name,
    TimeSpan timeout,
    Action<IServiceProvider, HttpClient>? configureClient = null
  )
  {
    return (services ?? throw new ArgumentNullException(nameof(services)))
      .AddHttpClient(
        name: name,
        (serviceProvider, client) => {
          client.Timeout = timeout;
          configureClient?.Invoke(serviceProvider, client);
        }
      );
  }
}
