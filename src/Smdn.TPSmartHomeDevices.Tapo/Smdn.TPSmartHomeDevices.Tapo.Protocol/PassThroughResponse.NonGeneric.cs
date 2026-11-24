// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1815 // CA1815: Override equals and operator equals on value types

using System;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <seealso cref="PassThroughRequestConverter"/>
/// <seealso cref="TapoSecurePassThroughSession.EncryptPassThroughRequest"/>
internal readonly struct PassThroughResponse(ReadOnlyMemory<byte> encryptedPayload) {
  public ReadOnlyMemory<byte> EncryptedPayload => encryptedPayload;
}
