// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Smdn.Formats;

namespace Smdn.TPSmartHomeDevices.Json;

public sealed class Base16ByteArrayJsonConverter : JsonConverter<byte[]?> {
  public override byte[]? Read(
    ref Utf8JsonReader reader,
    Type typeToConvert,
    JsonSerializerOptions options
  )
  {
    var str = reader.TokenType switch {
      JsonTokenType.Null => null,
      JsonTokenType.String => reader.GetString(),
      _ => null,
    };

    if (str is null)
      return null;
    if (str.Length == 0)
      return Array.Empty<byte>();
    if ((str.Length & 0b1) != 0b0)
      return null; // length must be 2n

    var result = new byte[str.Length / 2];

    return Hexadecimal.TryDecode(str.AsSpan(), result.AsSpan(), out var length) && length == result.Length
      ? result
      : null;
  }

  public override void Write(
    Utf8JsonWriter writer,
    byte[]? value,
    JsonSerializerOptions options
  )
    => throw new NotImplementedException();
}
