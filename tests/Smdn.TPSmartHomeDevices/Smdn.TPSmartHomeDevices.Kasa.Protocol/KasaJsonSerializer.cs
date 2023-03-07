// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using System.Text.Json;
using NUnit.Framework;
using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.TPSmartHomeDevices.Kasa.Protocol;

[TestFixture]
public class KasaJsonSerializerTests {
  private static System.Collections.IEnumerable YieldTestCases_EncryptDecryptInPlace()
  {
    const byte key = 0xAB;

    yield return new[] { new byte[0], new byte[0] };
    yield return new[] { new byte[1] { 0x01 }, new byte[1] { 0x01 ^ key } };
    yield return new[] { new byte[2] { 0x01, 0x23 }, new byte[2] { 0x01 ^ key, 0x23 ^ 0x01 ^ key } };
    yield return new[] { new byte[3] { 0x01, 0x23, 0x45 }, new byte[3] { 0x01 ^ key, 0x23 ^ 0x01 ^ key, 0x45 ^ 0x23 ^ 0x01 ^ key } };
  }

  [TestCaseSource(nameof(YieldTestCases_EncryptDecryptInPlace))]
  public void EncryptInPlace(byte[] raw, byte[] encrypted)
  {
    KasaJsonSerializer.EncryptInPlace(raw);

    Assert.That(raw, SequenceIs.EqualTo(encrypted));
  }

  [TestCaseSource(nameof(YieldTestCases_EncryptDecryptInPlace))]
  public void DecryptInPlace(byte[] raw, byte[] encrypted)
  {
    KasaJsonSerializer.DecryptInPlace(encrypted);

    Assert.That(encrypted, SequenceIs.EqualTo(raw));
  }

  private static System.Collections.IEnumerable YieldTestCases_Serialize()
  {
    yield return new object[] { "module", "method", new { }, @"{""module"":{""method"":{}}}" };
    yield return new object[] { "module", "method", new { Foo = (string)null! }, @"{""module"":{""method"":{}}}" };
    yield return new object[] { "module", "method", new { Foo = (string)null!, Bar = "Baz" }, @"{""module"":{""method"":{""Bar"":""Baz""}}}" };
    yield return new object[] { "module", "method", new { Foo = "Bar" }, @"{""module"":{""method"":{""Foo"":""Bar""}}}" };
    yield return new object[] { "module", "method", new { Foo = 42 }, @"{""module"":{""method"":{""Foo"":42}}}" };
    yield return new object[] { "module", "method", new { Foo = new { Bar = "Baz" } }, @"{""module"":{""method"":{""Foo"":{""Bar"":""Baz""}}}}" };
    yield return new object[] { "module", "method", null!, @"{""module"":{""method"":null}}" };
    yield return new object[] { "", "", new { }, @"{"""":{"""":{}}}" };
  }

  [TestCaseSource(nameof(YieldTestCases_Serialize))]
  public void Encrypt(string module, string method, object parameters, string expectedJsonExpression)
  {
    var buffer = new ArrayBufferWriter<byte>();

    KasaJsonSerializer.Serialize(
      buffer,
      JsonEncodedText.Encode(module),
      JsonEncodedText.Encode(method),
      parameters
    );

    var length = BinaryPrimitives.ReadInt32BigEndian(buffer.WrittenSpan.Slice(0, 4));

    Assert.AreEqual(expectedJsonExpression.Length, length, nameof(length));

    var body = new byte[length];

    buffer.WrittenSpan.Slice(4).CopyTo(body);

    KasaJsonSerializer.DecryptInPlace(body);

    Assert.AreEqual(
      expectedJsonExpression,
      Encoding.UTF8.GetString(body),
      nameof(body)
    );
  }

  [Test]
  public void Serialize_ArgumentNull_Buffer()
    => Assert.Throws<ArgumentNullException>(
      () => KasaJsonSerializer.Serialize(
        buffer: null!,
        JsonEncodedText.Encode("module"),
        JsonEncodedText.Encode("method"),
        (object)null!
      )
    );

  private static System.Collections.IEnumerable YieldTestCases_Deserialize()
  {
    foreach (var testCase in new[] {
      new {
        Json = @"{""module"":{""method"":{}}}",
        Module = "module",
        Method = "method",
        Assersion = new Action<JsonElement>(static result => {
          Assert.AreEqual(result.ValueKind, JsonValueKind.Object);
        })
      },
      new {
        Json = @"{""module"":{""method"":{""Foo"":""Bar""}}}",
        Module = "module",
        Method = "method",
        Assersion = new Action<JsonElement>(static result => {
          Assert.AreEqual("Bar", result.GetProperty("Foo").GetString());
        })
      },
      new {
        Json = @"{""module"":{""method"":{""Foo"":42}}}",
        Module = "module",
        Method = "method",
        Assersion = new Action<JsonElement>(static result => {
          Assert.AreEqual(42, result.GetProperty("Foo").GetInt32());
        })
      },
      new {
        Json = @"{""module"":{""method"":{""Foo"":{""Bar"":""Baz""}}}}",
        Module = "module",
        Method = "method",
        Assersion = new Action<JsonElement>(static result => {
          var foo = result.GetProperty("Foo");

          Assert.AreEqual(foo.ValueKind, JsonValueKind.Object);
          Assert.AreEqual("Baz", foo.GetProperty("Bar").GetString());
        })
      },
      new {
        Json = @"{""module"":{""method"":null}}",
        Module = "module",
        Method = "method",
        Assersion = new Action<JsonElement>(static result => {
          Assert.AreEqual(result.ValueKind, JsonValueKind.Null);
        })
      },
      new {
        Json = @"{"""":{"""":{}}}",
        Module = "",
        Method = "",
        Assersion = new Action<JsonElement>(static result => {
          Assert.AreEqual(result.ValueKind, JsonValueKind.Object);
        })
      },
    }) {
      var buffer = new ArrayBufferWriter<byte>();
      var body = Encoding.UTF8.GetBytes(testCase.Json);

      KasaJsonSerializer.EncryptInPlace(body);

      var header = new byte[4];
      BinaryPrimitives.WriteInt32BigEndian(header, body.Length);

      buffer.Write(header);
      buffer.Write(body);

      yield return new object[] {
        buffer,
        JsonEncodedText.Encode(testCase.Module),
        JsonEncodedText.Encode(testCase.Method),
        testCase.Assersion
      };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_Deserialize))]
  public void Decrypt(
    ArrayBufferWriter<byte> buffer,
    JsonEncodedText module,
    JsonEncodedText method,
    Action<JsonElement> assertDeserializedElement
  )
    => assertDeserializedElement(KasaJsonSerializer.Deserialize(buffer, module, method));

  [Test]
  public void Deserialize_ArgumentNull_Buffer()
    => Assert.Throws<ArgumentNullException>(
      () => KasaJsonSerializer.Deserialize(
        buffer: null!,
        JsonEncodedText.Encode("module"),
        JsonEncodedText.Encode("method")
      )
    );

  [Test]
  public void Deserialize_InvalidData_LengthHeaderTooShort(
    [Values(0, 1, 2, 3)] int length
  )
  {
    var buffer = new ArrayBufferWriter<byte>();

    if (0 <= length) {
      buffer.GetSpan(length);
      buffer.Advance(length);
    }

    Assert.Throws<InvalidDataException>(
      () => KasaJsonSerializer.Deserialize(
        buffer,
        JsonEncodedText.Encode("module"),
        JsonEncodedText.Encode("method")
      )
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_Deserialize_InvalidData_InputTooShort()
  {
    foreach (var input in new[] {
      new byte[] { 0x00, 0x00, 0x00, 0x01 }, // length = 1 byte, data = 0 bytes
      new byte[] { 0x00, 0x00, 0x00, 0x02, 0x00 }, // length = 2 byte, data = 1 bytes
    }) {
      yield return new object[] { CreateWrittenArrayBufferWritter(input) };
    }

    static ArrayBufferWriter<byte> CreateWrittenArrayBufferWritter(byte[] input)
    {
      var buffer = new ArrayBufferWriter<byte>();

      var span = buffer.GetSpan(input.Length);

      input.AsSpan().CopyTo(span);

      buffer.Advance(input.Length);

      return buffer;
    }
  }

  [TestCaseSource(nameof(YieldTestCases_Deserialize_InvalidData_InputTooShort))]
  public void Deserialize_InvalidData_InputTooShort(ArrayBufferWriter<byte> buffer)
    => Assert.Throws<InvalidDataException>(
      () => KasaJsonSerializer.Deserialize(
        buffer,
        JsonEncodedText.Encode("module"),
        JsonEncodedText.Encode("method")
      )
    );

  private static System.Collections.IEnumerable YieldTestCases_Deserialize_InvalidData_ModuleUnmatch()
  {
    foreach (var testCase in new[] {
      new { SerializingModuleName = "module", DeserializingModuleName = "Module" },
      new { SerializingModuleName = "module", DeserializingModuleName = "modulE" },
    }) {
      var buffer = new ArrayBufferWriter<byte>();
      var methodName = JsonEncodedText.Encode("method");

      KasaJsonSerializer.Serialize(
        buffer,
        JsonEncodedText.Encode(testCase.SerializingModuleName),
        methodName,
        parameter: new { }
      );

      yield return new object[] { buffer, JsonEncodedText.Encode(testCase.DeserializingModuleName), methodName };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_Deserialize_InvalidData_ModuleUnmatch))]
  public void Deserialize_InvalidData_ModuleUnmatch(ArrayBufferWriter<byte> buffer, JsonEncodedText module, JsonEncodedText method)
    => Assert.Throws<InvalidDataException>(
      () => KasaJsonSerializer.Deserialize(
        buffer,
        module,
        method
      )
    );

  private static System.Collections.IEnumerable YieldTestCases_Deserialize_InvalidData_MethodUnmatch()
  {
    foreach (var testCase in new[] {
      new { SerializingMethodName = "method", DeserializingMethodName = "Method" },
      new { SerializingMethodName = "method", DeserializingMethodName = "methoD" },
    }) {
      var buffer = new ArrayBufferWriter<byte>();
      var moduleName = JsonEncodedText.Encode("module");

      KasaJsonSerializer.Serialize(
        buffer,
        moduleName,
        JsonEncodedText.Encode(testCase.SerializingMethodName),
        parameter: new { }
      );

      yield return new object[] { buffer, moduleName, JsonEncodedText.Encode(testCase.DeserializingMethodName) };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_Deserialize_InvalidData_MethodUnmatch))]
  public void Deserialize_InvalidData_MethodUnmatch(ArrayBufferWriter<byte> buffer, JsonEncodedText module, JsonEncodedText method)
    => Assert.Throws<InvalidDataException>(
      () => KasaJsonSerializer.Deserialize(
        buffer,
        module,
        method
      )
    );
}
