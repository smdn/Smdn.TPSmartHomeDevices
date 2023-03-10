// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Json;

internal sealed class TimeSpanInMinutesJsonConverter : JsonConverter<TimeSpan?> {
  public override TimeSpan? Read(
    ref Utf8JsonReader reader,
    Type typeToConvert,
    JsonSerializerOptions options
  )
    => reader.TryGetInt32(out var timeDiff)
      ? TimeSpan.FromMinutes(timeDiff)
      : null;

  public override void Write(
    Utf8JsonWriter writer,
    TimeSpan? value,
    JsonSerializerOptions options
  )
    => throw new NotImplementedException();
}
