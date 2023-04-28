// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;

namespace Smdn.TPSmartHomeDevices.Tapo.Credentials;

/// <summary>
/// Provides a mechanism for abstracting credentials used for authentication in Tapo's communication protocol.
/// </summary>
public interface ITapoCredential : IDisposable {
  /// <summary>
  /// Writes the password corresponding to this credential into a JSON property.
  /// </summary>
  /// <remarks>
  /// When implementing this method, use <see cref="Utf8JsonWriter.WriteStringValue(string?)"/> or <see cref="Utf8JsonWriter.WriteBase64StringValue"/> to write the JSON property value for the password.
  /// Also, the password must be written as a BASE64-encoded value.
  /// </remarks>
  /// <param name="writer">The <see cref="Utf8JsonWriter"/> currently pointing to where the property value is to be written.</param>
  /// <seealso cref="Protocol.LoginDeviceRequest"/>
  /// <seealso cref="TapoCredentials.ToBase64EncodedString(ReadOnlySpan{char})"/>
  void WritePasswordPropertyValue(Utf8JsonWriter writer);

  /// <summary>
  /// Writes the user name corresponding to this credential into a JSON property.
  /// </summary>
  /// <remarks>
  /// When implementing this method, use <see cref="Utf8JsonWriter.WriteStringValue(string?)"/> or <see cref="Utf8JsonWriter.WriteBase64StringValue"/> to write the JSON property value for the user name.
  /// Also, the user name must be written as a BASE64-encoded value of the its SHA-1 digest.
  /// </remarks>
  /// <param name="writer">The <see cref="Utf8JsonWriter"/> currently pointing to where the property value is to be written.</param>
  /// <seealso cref="Protocol.LoginDeviceRequest"/>
  /// <seealso cref="TapoCredentials.ToBase64EncodedSHA1DigestString(ReadOnlySpan{char})"/>
  void WriteUsernamePropertyValue(Utf8JsonWriter writer);
}
