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
  /// <param name="writer">The <see cref="Utf8JsonWriter"/> currently pointing to where the property value is to be written.</param>
  /// <seealso cref="Protocol.LoginDeviceRequest"/>
  void WritePasswordPropertyValue(Utf8JsonWriter writer);

  /// <summary>
  /// Writes the user name corresponding to this credential into a JSON property.
  /// </summary>
  /// <param name="writer">The <see cref="Utf8JsonWriter"/> currently pointing to where the property value is to be written.</param>
  /// <seealso cref="Protocol.LoginDeviceRequest"/>
  void WriteUsernamePropertyValue(Utf8JsonWriter writer);
}
