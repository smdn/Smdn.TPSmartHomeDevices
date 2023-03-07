// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Smdn.TPSmartHomeDevices.Tapo;

public static class TapoCredentailProviderServiceCollectionExtensions {
  public static IServiceCollection AddTapoCredential(
    this IServiceCollection services,
    string userName,
    string password
  )
  {
    if (services is null)
      throw new ArgumentNullException(nameof(services));

    services.AddSingleton<ITapoCredentialProvider>(
      new PlainTextCredentialProvider(
        userName ?? throw new ArgumentNullException(nameof(userName)),
        password ?? throw new ArgumentNullException(nameof(password))
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

    services.AddSingleton<ITapoCredentialProvider>(
      new Base64EncodedCredentialProvider(
        base64UserNameSHA1Digest ?? throw new ArgumentNullException(nameof(base64UserNameSHA1Digest)),
        base64Password ?? throw new ArgumentNullException(nameof(base64Password))
      )
    );

    return services;
  }
}
