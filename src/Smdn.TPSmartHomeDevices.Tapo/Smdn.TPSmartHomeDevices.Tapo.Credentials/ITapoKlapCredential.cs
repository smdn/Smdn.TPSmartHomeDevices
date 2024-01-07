// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.TPSmartHomeDevices.Tapo.Credentials;

/// <summary>
/// Provides a mechanism for abstracting credentials used for authentication in Tapo's KLAP protocol.
/// </summary>
/// <seealso cref="ITapoCredentialProvider"/>
/// <seealso cref="Protocol.TapoSessionProtocol.Klap"/>
public interface ITapoKlapCredential : IDisposable {
  /// <summary>
  /// Writes the <c>local_auth_hash</c> corresponding to this credential into a <see cref="Span{Byte}"/>.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///   The <c>local_auth_hash</c> represents a hash value based on the username and password corresponding with the credential, calculated by the following pseudo expression.
  ///   <code>
  ///     local_auth_hash = SHA256(SHA1(username) + SHA1(password))
  ///   </code>
  ///   </para>
  /// </remarks>
  /// <param name="destination">The <see cref="Span{Byte}"/> to where the credential will be written.</param>
  /// <seealso cref="Protocol.TapoSessionProtocol.Klap"/>
  /// <seealso cref="Protocol.TapoClient.AuthenticateAsync(Protocol.TapoSessionProtocol, ITapoCredentialIdentity?, ITapoCredentialProvider, System.Threading.CancellationToken)"/>
  // <seealso cref="TapoCredentials.ToBase64EncodedString(ReadOnlySpan{char})"/>
  void WriteLocalAuthHash(Span<byte> destination);
}
