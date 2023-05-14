// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Kasa.Json;

public sealed class KasaNumericalBooleanJsonConverter : JsonConverter<bool> {
  public override bool Read(
    ref Utf8JsonReader reader,
    Type typeToConvert,
    JsonSerializerOptions options
  )
    =>
      reader.TokenType == JsonTokenType.Number &&
      reader.TryGetInt32(out var numericalBool) &&
      numericalBool != 0;

  public override void Write(
    Utf8JsonWriter writer,
    bool value,
    JsonSerializerOptions options
  )
    => (writer ?? throw new ArgumentNullException(nameof(writer))).WriteNumberValue(value ? 1 : 0);
}
