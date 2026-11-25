// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <seealso cref="PassThroughResponse"/>
internal sealed class PassThroughResponseConverter : JsonConverter<PassThroughResponse> {
  public override PassThroughResponse Read(
    ref Utf8JsonReader reader,
    Type typeToConvert,
    JsonSerializerOptions options
  )
  {
    if (reader.TokenType != JsonTokenType.StartObject)
      throw new JsonException($"Expected {nameof(JsonTokenType.StartObject)} token for 'result' property could not read.");

    byte[]? encryptedPayload = null;

    while (reader.Read()) {
      if (reader.TokenType == JsonTokenType.EndObject)
        break;

      if (reader.TokenType == JsonTokenType.PropertyName) {
        if (reader.ValueTextEquals("response"u8)) {
          reader.Read();

          if (reader.TokenType == JsonTokenType.String)
            encryptedPayload = reader.GetBytesFromBase64();
          else if (reader.TokenType == JsonTokenType.Null)
            encryptedPayload = null;
        }
        else {
          reader.Skip(); // ignore unexpected and unknown properties
        }
      }
    }

    return new PassThroughResponse(
      encryptedPayload: encryptedPayload ?? ReadOnlyMemory<byte>.Empty
    );
  }

  public override void Write(
    Utf8JsonWriter writer,
    PassThroughResponse value,
    JsonSerializerOptions options
  )
    => throw new NotSupportedException();
}
