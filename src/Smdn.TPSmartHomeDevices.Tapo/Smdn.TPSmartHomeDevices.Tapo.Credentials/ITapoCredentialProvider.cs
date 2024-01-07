// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices.Tapo.Credentials;

/// <summary>
/// Provides a mechanism to select the <see cref="ITapoCredential"/> or <see cref="ITapoKlapCredential"/> corresponding to the <see cref="ITapoCredentialIdentity"/> and
/// provide it to the authentication process in Tapo's protocol.
/// </summary>
/// <remarks>
///   <para>
///     In the current implementation of <see cref="TapoDevice"/>, the <see cref="TapoDevice"/> object itself,
///     that is attempting to authenticate to the device, is used as the <see cref="ITapoCredentialIdentity"/>.
///   </para>
///   <para>
///     If the method <see cref="Protocol.TapoClient.AuthenticateAsync(ITapoCredentialIdentity?, ITapoCredentialProvider, System.Threading.CancellationToken)"/> is called directly,
///     the <see cref="ITapoCredentialIdentity"/> specified with its parameter is used.
///   </para>
/// </remarks>
/// <seealso cref="ITapoCredential"/>
/// <seealso cref="ITapoKlapCredential"/>
/// <seealso cref="ITapoCredentialIdentity"/>
/// <seealso cref="Protocol.TapoSessionProtocol"/>
/// <seealso cref="Protocol.TapoClient.AuthenticateAsync(Protocol.TapoSessionProtocol, ITapoCredentialIdentity?, ITapoCredentialProvider, System.Threading.CancellationToken)"/>
public interface ITapoCredentialProvider {
  /// <summary>
  /// Gets the <see cref="ITapoCredential"/> corresponding to the specified identity, used for authentication process in 'secure pass through' protocol, or <c>login_device</c> command.
  /// </summary>
  /// <remarks>
  /// The concrete implementation of this method should return the <see cref="ITapoCredential"/>
  /// selected by <paramref name="identity"/> from the this <see cref="ITapoCredentialProvider"/>.
  /// If <paramref name="identity"/> is <see langword="null"/>, a default credential
  /// that is not specific to a particular <see cref="ITapoCredentialIdentity"/> must be returned.
  /// </remarks>
  /// <param name="identity">
  /// The <see cref="ITapoCredentialIdentity"/> that is requesting to obtain the
  /// corresponding <see cref="ITapoCredential"/> from this <see cref="ITapoCredentialProvider"/>.
  /// </param>
  /// <seealso cref="Protocol.TapoSessionProtocol.SecurePassThrough"/>
  /// <seealso cref="Protocol.TapoClient.AuthenticateAsync(ITapoCredentialIdentity?, ITapoCredentialProvider, System.Threading.CancellationToken)"/>
  /// <seealso cref="Protocol.TapoClient.AuthenticateAsync(Protocol.TapoSessionProtocol, ITapoCredentialIdentity?, ITapoCredentialProvider, System.Threading.CancellationToken)"/>
  /// <seealso cref="Protocol.LoginDeviceRequest"/>
  /// <seealso cref="Protocol.SecurePassThroughJsonConverterFactory"/>
  /// <seealso cref="TapoDevice"/>
  ITapoCredential GetCredential(ITapoCredentialIdentity? identity);

  /// <summary>
  /// Gets the <see cref="ITapoKlapCredential"/> corresponding to the specified identity, used for authentication process in Tapo's KLAP protocol.
  /// </summary>
  /// <remarks>
  /// The concrete implementation of this method should return the <see cref="ITapoKlapCredential"/>
  /// selected by <paramref name="identity"/> from the this <see cref="ITapoCredentialProvider"/>.
  /// If <paramref name="identity"/> is <see langword="null"/>, a default credential
  /// that is not specific to a particular <see cref="ITapoCredentialIdentity"/> must be returned.
  /// </remarks>
  /// <param name="identity">
  /// The <see cref="ITapoCredentialIdentity"/> that is requesting to obtain the
  /// corresponding <see cref="ITapoKlapCredential"/> from this <see cref="ITapoCredentialProvider"/>.
  /// </param>
  /// <seealso cref="Protocol.TapoSessionProtocol.Klap"/>
  /// <seealso cref="Protocol.TapoClient.AuthenticateAsync(ITapoCredentialIdentity?, ITapoCredentialProvider, System.Threading.CancellationToken)"/>
  /// <seealso cref="Protocol.TapoClient.AuthenticateAsync(Protocol.TapoSessionProtocol, ITapoCredentialIdentity?, ITapoCredentialProvider, System.Threading.CancellationToken)"/>
  /// <seealso cref="TapoDevice"/>
  ITapoKlapCredential GetKlapCredential(ITapoCredentialIdentity? identity);
}
