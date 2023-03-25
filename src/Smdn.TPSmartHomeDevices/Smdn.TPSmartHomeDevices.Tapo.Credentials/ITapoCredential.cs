// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;

namespace Smdn.TPSmartHomeDevices.Tapo.Credentials;

public interface ITapoCredential : IDisposable {
  void WritePasswordPropertyValue(Utf8JsonWriter writer);
  void WriteUsernamePropertyValue(Utf8JsonWriter writer);
}
