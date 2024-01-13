// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Json;

public sealed class TapoLocalDateAndTimeJsonConverter : JsonConverter<DateTime?> {
  private const string DateAndTimeFormat = "yyyy-MM-dd HH:mm:ss";

  public override DateTime? Read(
    ref Utf8JsonReader reader,
    Type typeToConvert,
    JsonSerializerOptions options
  )
  {
    var value = reader.TokenType == JsonTokenType.String
      ? reader.GetString()
      : null;

    if (value is null)
      return null;

    return DateTime.TryParseExact(value, format: DateAndTimeFormat, provider: null, style: default, out var localDateAndTime)
      ? localDateAndTime
      : null;
  }

  public override void Write(
    Utf8JsonWriter writer,
    DateTime? value,
    JsonSerializerOptions options
  )
    => throw new NotImplementedException();
}
