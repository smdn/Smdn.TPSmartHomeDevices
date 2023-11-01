// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo;

public static class TapoCredentailProviderServiceCollectionExtensions {
  /// <summary>
  /// Adds <see cref="ITapoCredentialProvider"/> to <see cref="IServiceCollection"/>.
  /// This overload creates <see cref="ITapoCredentialProvider"/> that holds email address and password in plaintext.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
  /// <param name="email">A plaintext email address used for authentication to the Tapo device.</param>
  /// <param name="password">A plaintext password used for authentication to the Tapo device.</param>
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

  /// <summary>
  /// Adds <see cref="ITapoCredentialProvider"/> to <see cref="IServiceCollection"/>.
  /// This overload creates <see cref="ITapoCredentialProvider"/> that holds user name (email address) and password in base64 encoded text.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
  /// <param name="base64UserNameSHA1Digest">A base64 encoded SHA1 digest of user name (email address) text, used for authentication to the Tapo device.</param>
  /// <param name="base64Password">A base64 encoded password text, used for authentication to the Tapo device.</param>
  /// <seealso cref="TapoCredentials.ToBase64EncodedSHA1DigestString(ReadOnlySpan{char})"/>
  /// <seealso cref="TapoCredentials.ToBase64EncodedString(ReadOnlySpan{char})"/>
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

  /// <summary>
  /// Adds <see cref="ITapoCredentialProvider"/> to <see cref="IServiceCollection"/>.
  /// This overload creates <see cref="ITapoCredentialProvider"/> that retrieve user name (email address) and password from environment variables.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
  /// <param name="envVarUsername">The name of environment variable that holds the user name (email address) for authentication to the Tapo device.</param>
  /// <param name="envVarPassword">The name of environment variable that holds the password for authentication to the Tapo device.</param>
  public static IServiceCollection AddTapoCredentialFromEnvironmentVariable(
    this IServiceCollection services,
    string envVarUsername,
    string envVarPassword
  )
  {
    if (services is null)
      throw new ArgumentNullException(nameof(services));

    services.TryAdd(
      ServiceDescriptor.Singleton(
        typeof(ITapoCredentialProvider),
        TapoCredentials.CreateProviderFromEnvironmentVariables(
          envVarUsername: envVarUsername,
          envVarPassword: envVarPassword
        )
      )
    );

    return services;
  }

  /// <summary>
  /// Adds <see cref="ITapoCredentialProvider"/> to <see cref="IServiceCollection"/>.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
  /// <param name="credentialProvider">A <see cref="ITapoCredentialProvider"/> used for authentication to the Tapo device.</param>
  /// <seealso cref="ITapoCredentialProvider"/>
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
