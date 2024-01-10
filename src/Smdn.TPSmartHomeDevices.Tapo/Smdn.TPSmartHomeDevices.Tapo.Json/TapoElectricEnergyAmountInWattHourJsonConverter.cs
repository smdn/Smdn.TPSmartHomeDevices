// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Json;

public sealed class TapoElectricEnergyAmountInWattHourJsonConverter : JsonConverter<ElectricEnergyAmount?> {
  public override ElectricEnergyAmount? Read(
    ref Utf8JsonReader reader,
    Type typeToConvert,
    JsonSerializerOptions options
  )
    => reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var value)
      ? new ElectricEnergyAmount(valueInWattHour: value)
      : null;

  public override void Write(
    Utf8JsonWriter writer,
    ElectricEnergyAmount? value,
    JsonSerializerOptions options
  )
    => throw new NotImplementedException();
}
