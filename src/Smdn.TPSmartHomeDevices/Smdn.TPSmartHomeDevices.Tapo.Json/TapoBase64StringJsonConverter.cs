// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Json;

internal sealed class TapoBase64StringJsonConverter : JsonConverter<string?> {
  public override string? Read(
    ref Utf8JsonReader reader,
    Type typeToConvert,
    JsonSerializerOptions options
  )
    => reader.TokenType == JsonTokenType.String && reader.TryGetBytesFromBase64(out var base64) && base64 is not null
      ? Encoding.UTF8.GetString(base64)
      : null;

  public override void Write(
    Utf8JsonWriter writer,
    string? value,
    JsonSerializerOptions options
  )
    => throw new NotImplementedException();
}
