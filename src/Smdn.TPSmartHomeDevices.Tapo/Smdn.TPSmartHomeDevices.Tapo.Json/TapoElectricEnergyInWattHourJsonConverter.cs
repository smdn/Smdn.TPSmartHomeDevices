// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Json;

public sealed class TapoElectricEnergyInWattHourJsonConverter : JsonConverter<decimal?> {
  public override decimal? Read(
    ref Utf8JsonReader reader,
    Type typeToConvert,
    JsonSerializerOptions options
  )
    => reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var value)
      ? value
      : null;

  public override void Write(
    Utf8JsonWriter writer,
    decimal? value,
    JsonSerializerOptions options
  )
    => throw new NotImplementedException();
}
