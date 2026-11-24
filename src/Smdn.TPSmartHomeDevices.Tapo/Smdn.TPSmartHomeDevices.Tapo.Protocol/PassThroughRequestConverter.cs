// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <seealso cref="PassThroughRequest"/>
internal sealed class PassThroughRequestConverter : JsonConverter<PassThroughRequest> {
  public override PassThroughRequest Read(
    ref Utf8JsonReader reader,
    Type typeToConvert,
    JsonSerializerOptions options
  )
    => throw new NotSupportedException();

  public override void Write(
    Utf8JsonWriter writer,
    PassThroughRequest value,
    JsonSerializerOptions options
  )
  {
    writer.WriteStartObject();
    writer.WritePropertyName("request");
    value.WriteTo(writer);
    writer.WriteEndObject();
  }
}
