// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Json;

public sealed class GeolocationInDecimalDegreesJsonConverter : JsonConverter<decimal?> {
  private const decimal Scaler = 10000m;

  public override decimal? Read(
    ref Utf8JsonReader reader,
    Type typeToConvert,
    JsonSerializerOptions options
  )
    => reader.TokenType == JsonTokenType.Number && reader.TryGetDecimal(out var scaledDecimalDegrees)
      ? scaledDecimalDegrees / Scaler
      : null;

  public override void Write(
    Utf8JsonWriter writer,
    decimal? value,
    JsonSerializerOptions options
  )
    => throw new NotImplementedException();
}
