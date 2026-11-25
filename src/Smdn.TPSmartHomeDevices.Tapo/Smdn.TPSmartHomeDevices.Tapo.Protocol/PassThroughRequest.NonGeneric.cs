// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1815 // CA1815: Override equals and operator equals on value types

using System;
using System.Text.Json;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <seealso cref="PassThroughRequestConverter"/>
/// <seealso cref="TapoSecurePassThroughSession.EncryptPassThroughRequest"/>
internal readonly struct PassThroughRequest(Action<Utf8JsonWriter> writeAction) {
  public void WriteTo(Utf8JsonWriter writer)
    => (writeAction ?? throw new InvalidOperationException($"{nameof(writeAction)} is null")).Invoke(writer);
}
