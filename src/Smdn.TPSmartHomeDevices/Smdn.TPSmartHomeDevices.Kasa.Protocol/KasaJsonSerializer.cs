// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Kasa.Protocol;

/// <remarks>
/// This implementation is based on and ported from the following implementation: <see href="https://github.com/plasticrake/tplink-smarthome-api">plasticrake/tplink-smarthome-api</see>.
/// </remarks>
public static class KasaJsonSerializer {
  public const byte InitialKey = 0xAB;
  private const int SizeOfHeaderInBytes = 4;

  private static readonly JsonSerializerOptions serializerOptions = new() {
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
  };

  public static void Serialize<TMethodParameter>(
    ArrayBufferWriter<byte> buffer,
    JsonEncodedText module,
    JsonEncodedText method,
    TMethodParameter? parameter,
    ILogger? logger = null
  )
  {
    if (buffer is null)
      throw new ArgumentNullException(nameof(buffer));

    // reserve placeholder for header
    buffer.GetSpan(SizeOfHeaderInBytes);
    buffer.Advance(SizeOfHeaderInBytes);

    // write json body after header placeholder
    using var writer = new Utf8JsonWriter(buffer);

    writer.WriteStartObject();
    writer.WriteStartObject(module);
    writer.WritePropertyName(method);

    JsonSerializer.Serialize(
      writer,
      parameter,
      options: serializerOptions
    );

    writer.WriteEndObject(); // method
    writer.WriteEndObject(); // module
    writer.Flush();

    if (!MemoryMarshal.TryGetArray(buffer.WrittenMemory, out var writtenArraySegment))
      throw new NotSupportedException("cannot get underlying array segment");

    // write header
    BinaryPrimitives.WriteInt32BigEndian(
      writtenArraySegment.AsSpan(0, SizeOfHeaderInBytes),
      (int)writer.BytesCommitted
    );

    logger?.LogTrace(
      "Request: {Request}",
      Encoding.UTF8.GetString(writtenArraySegment.AsSpan(SizeOfHeaderInBytes))
    );

    // encrypt body
    EncryptInPlace(writtenArraySegment.AsSpan(SizeOfHeaderInBytes));
  }

  public static void EncryptInPlace(Span<byte> body)
  {
    var key = InitialKey;

    for (var i = 0; i < body.Length; i++) {
      key = body[i] = (byte)(body[i] ^ key);
    }
  }

  public static JsonElement Deserialize(
    ArrayBufferWriter<byte> buffer,
    JsonEncodedText module,
    JsonEncodedText method,
    ILogger? logger = null
  )
  {
    if (buffer is null)
      throw new ArgumentNullException(nameof(buffer));

    var buf = buffer.WrittenMemory;

    if (buf.Length < 4)
      throw new InvalidDataException("input too short (expects at least 4 bytes of header)");

    var length = BinaryPrimitives.ReadInt32BigEndian(buf.Slice(0, 4).Span);
    var body = buf.Slice(4);

    if (body.Length < length)
      throw new InvalidDataException($"input too short (expects at least {length} bytes of data body, but is {body.Length} bytes)");

    body = body.Slice(0, length);

    if (!MemoryMarshal.TryGetArray(body, out var bodyArraySegment))
      throw new NotSupportedException("cannot get underlying array segment");

    DecryptInPlace(bodyArraySegment.AsSpan());

    logger?.LogTrace(
      "Response: {Response}",
      Encoding.UTF8.GetString(bodyArraySegment.AsSpan())
    );

    var doc = JsonDocument.Parse(bodyArraySegment.AsMemory());

    if (!doc.RootElement.TryGetProperty(module.EncodedUtf8Bytes, out var propModule))
      throw new InvalidDataException($"The response JSON does not contain the expected property for the module '{module}'.");

    if (!propModule.TryGetProperty(method.EncodedUtf8Bytes, out var propMethod))
      throw new InvalidDataException($"The response JSON does not contain the expected property for the method '{method}'.");

    return propMethod;
  }

  public static void DecryptInPlace(Span<byte> body)
  {
    var key = InitialKey;

    for (var i = 0; i < body.Length; i++) {
      (key, body[i]) = (body[i], (byte)(body[i] ^ key));
    }
  }
}
