// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo;

internal static class TapoCredentialProviderServiceCollectionExtensions {
  /// <summary>
  /// Adds <see cref="ITapoCredentialProvider"/> to <see cref="IServiceCollection"/>.
  /// This overload creates <see cref="ITapoCredentialProvider"/> that holds email address and password in plaintext.
  /// </summary>
  /// <remarks>
  /// The <see cref="ITapoCredentialProvider"/> added by this overload can be used for both <see cref="Protocol.TapoSessionProtocol.SecurePassThrough"/> and <see cref="Protocol.TapoSessionProtocol.Klap"/> authentication process.
  /// </remarks>
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
      ServiceDescriptor.Singleton<ITapoCredentialProvider>(
        TapoCredentials.CreateProviderFromPlainText(email, password)
      )
    );

    return services;
  }

  /// <summary>
  /// Adds <see cref="ITapoCredentialProvider"/> to <see cref="IServiceCollection"/>.
  /// This overload creates <see cref="ITapoCredentialProvider"/> that holds user name (email address) and password in base64 encoded text.
  /// </summary>
  /// <remarks>
  /// The <see cref="ITapoCredentialProvider"/> added by this overload can only be used for <see cref="Protocol.TapoSessionProtocol.SecurePassThrough"/> authentication process.
  /// </remarks>
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
      ServiceDescriptor.Singleton<ITapoCredentialProvider>(
        TapoCredentials.CreateProviderFromBase64EncodedText(base64UserNameSHA1Digest, base64Password)
      )
    );

    return services;
  }

  /// <summary>
  /// Adds <see cref="ITapoCredentialProvider"/> to <see cref="IServiceCollection"/>.
  /// This overload creates <see cref="ITapoCredentialProvider"/> that retrieve user name (email address) and password from environment variables.
  /// </summary>
  /// <remarks>
  /// The <see cref="ITapoCredentialProvider"/> added by this overload can be used for both <see cref="Protocol.TapoSessionProtocol.SecurePassThrough"/> and <see cref="Protocol.TapoSessionProtocol.Klap"/> authentication process.
  /// </remarks>
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
      ServiceDescriptor.Singleton<ITapoCredentialProvider>(
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
  /// This overload creates <see cref="ITapoCredentialProvider"/> that retrieves the base64-encoded KLAP <c>local_auth_hash</c> from environment variables.
  /// To authenticate using this method, the device firmware must support KLAP.
  /// </summary>
  /// <remarks>
  /// The <see cref="ITapoCredentialProvider"/> added by this overload can only be used for <see cref="Protocol.TapoSessionProtocol.Klap"/> authentication process.
  /// </remarks>
  /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
  /// <param name="envVarBase64KlapLocalAuthHash">The name of environment variable that holds the <c>local_auth_hash</c> for authentication to the Tapo device.</param>
  public static IServiceCollection AddTapoBase64EncodedKlapCredentialFromEnvironmentVariable(
    this IServiceCollection services,
    string envVarBase64KlapLocalAuthHash
  )
  {
    if (services is null)
      throw new ArgumentNullException(nameof(services));

    services.TryAdd(
      ServiceDescriptor.Singleton<ITapoCredentialProvider>(
        TapoCredentials.CreateProviderFromEnvironmentVariables(
          envVarBase64KlapLocalAuthHash: envVarBase64KlapLocalAuthHash
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
      ServiceDescriptor.Singleton<ITapoCredentialProvider>(credentialProvider)
    );

    return services;
  }
}
