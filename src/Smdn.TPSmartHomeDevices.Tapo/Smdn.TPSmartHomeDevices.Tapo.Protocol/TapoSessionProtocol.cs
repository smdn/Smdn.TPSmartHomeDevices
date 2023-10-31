// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// Specifies the protocol used for the session for authenticating and operating the Tapo device.
/// </summary>
public enum TapoSessionProtocol {
  /// <summary>
  /// Specifies the protocol that use the 'securePassthrough' request method.
  /// </summary>
  SecurePassThrough,

  /// <summary>
  /// Specifies the 'KLAP' protocol.
  /// </summary>
  /// <remarks>
  /// Tapo devices with firmware version <c>1.1.0 Build 230721 Rel 224802</c> or later installed use the 'KLAP' protocol.
  /// </remarks>
  Klap,
}
