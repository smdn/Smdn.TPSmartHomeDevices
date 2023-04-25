// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo;

public static class TapoCredentailProviderServiceCollectionExtensions {
  public static IServiceCollection AddTapoCredential(
    this IServiceCollection services,
    string email,
    string password
  )
  {
    if (services is null)
      throw new ArgumentNullException(nameof(services));

    services.TryAdd(
      ServiceDescriptor.Singleton(
        typeof(ITapoCredentialProvider),
        TapoCredentials.CreateProviderFromPlainText(email, password)
      )
    );

    return services;
  }

  public static IServiceCollection AddTapoBase64EncodedCredential(
    this IServiceCollection services,
    string base64UserNameSHA1Digest,
    string base64Password
  )
  {
    if (services is null)
      throw new ArgumentNullException(nameof(services));

    services.TryAdd(
      ServiceDescriptor.Singleton(
        typeof(ITapoCredentialProvider),
        TapoCredentials.CreateProviderFromBase64EncodedText(base64UserNameSHA1Digest, base64Password)
      )
    );

    return services;
  }

  public static IServiceCollection AddTapoCredentialProvider(
    this IServiceCollection services,
    ITapoCredentialProvider credentialProvider
  )
  {
    if (services is null)
      throw new ArgumentNullException(nameof(services));
    if (credentialProvider is null)
      throw new ArgumentNullException(nameof(credentialProvider));

    services.TryAdd(
      ServiceDescriptor.Singleton(typeof(ITapoCredentialProvider), credentialProvider)
    );

    return services;
  }
}
