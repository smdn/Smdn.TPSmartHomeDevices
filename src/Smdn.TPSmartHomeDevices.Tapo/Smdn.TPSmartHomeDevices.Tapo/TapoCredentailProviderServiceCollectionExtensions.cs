// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using Microsoft.Extensions.DependencyInjection;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo;

/// <inheritdoc cref="TapoCredentialProviderServiceCollectionExtensions"/>
// cSpell:ignore Credentail
// TODO: fix naming Credentail->Credential
// [Obsolete($"Use {nameof(TapoCredentialProviderServiceCollectionExtensions)} instead.")]
public static class TapoCredentailProviderServiceCollectionExtensions {
  /// <inheritdoc cref="TapoCredentialProviderServiceCollectionExtensions.AddTapoCredential"/>
  public static IServiceCollection AddTapoCredential(
    this IServiceCollection services,
    string email,
    string password
  )
    => TapoCredentialProviderServiceCollectionExtensions.AddTapoCredential(
      services: services ?? throw new ArgumentNullException(nameof(services)),
      email: email,
      password: password
    );

  /// <inheritdoc cref="TapoCredentialProviderServiceCollectionExtensions.AddTapoBase64EncodedCredential"/>
  public static IServiceCollection AddTapoBase64EncodedCredential(
    this IServiceCollection services,
    string base64UserNameSHA1Digest,
    string base64Password
  )
    => TapoCredentialProviderServiceCollectionExtensions.AddTapoBase64EncodedCredential(
      services: services ?? throw new ArgumentNullException(nameof(services)),
      base64UserNameSHA1Digest: base64UserNameSHA1Digest,
      base64Password: base64Password
    );

  /// <inheritdoc cref="TapoCredentialProviderServiceCollectionExtensions.AddTapoCredentialFromEnvironmentVariable"/>
  public static IServiceCollection AddTapoCredentialFromEnvironmentVariable(
    this IServiceCollection services,
    string envVarUsername,
    string envVarPassword
  )
    => TapoCredentialProviderServiceCollectionExtensions.AddTapoCredentialFromEnvironmentVariable(
      services: services ?? throw new ArgumentNullException(nameof(services)),
      envVarUsername: envVarUsername,
      envVarPassword: envVarPassword
    );

  /// <inheritdoc cref="TapoCredentialProviderServiceCollectionExtensions.AddTapoBase64EncodedKlapCredentialFromEnvironmentVariable"/>
  public static IServiceCollection AddTapoBase64EncodedKlapCredentialFromEnvironmentVariable(
    this IServiceCollection services,
    string envVarBase64KlapLocalAuthHash
  )
    => TapoCredentialProviderServiceCollectionExtensions.AddTapoBase64EncodedKlapCredentialFromEnvironmentVariable(
      services: services ?? throw new ArgumentNullException(nameof(services)),
      envVarBase64KlapLocalAuthHash: envVarBase64KlapLocalAuthHash
    );

  /// <inheritdoc cref="TapoCredentialProviderServiceCollectionExtensions.AddTapoCredentialProvider"/>
  public static IServiceCollection AddTapoCredentialProvider(
    this IServiceCollection services,
    ITapoCredentialProvider credentialProvider
  )
    => TapoCredentialProviderServiceCollectionExtensions.AddTapoCredentialProvider(
      services: services ?? throw new ArgumentNullException(nameof(services)),
      credentialProvider: credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider))
    );
}
