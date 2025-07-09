// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.TPSmartHomeDevices.Kasa.Protocol;

[TestFixture]
public class KasaJsonSerializerTests {
  private static System.Collections.IEnumerable YieldTestCases_EncryptDecryptInPlace()
  {
    const byte Key = 0xAB;

    yield return new[] { new byte[0], new byte[0] };
    yield return new[] { new byte[1] { 0x01 }, new byte[1] { 0x01 ^ Key } };
    yield return new[] { new byte[2] { 0x01, 0x23 }, new byte[2] { 0x01 ^ Key, 0x23 ^ 0x01 ^ Key } };
    yield return new[] { new byte[3] { 0x01, 0x23, 0x45 }, new byte[3] { 0x01 ^ Key, 0x23 ^ 0x01 ^ Key, 0x45 ^ 0x23 ^ 0x01 ^ Key } };
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

    Assert.That(length, Is.EqualTo(expectedJsonExpression.Length), nameof(length));

    var body = new byte[length];

    buffer.WrittenSpan.Slice(4).CopyTo(body);

    KasaJsonSerializer.DecryptInPlace(body);

    Assert.That(
      Encoding.UTF8.GetString(body),
      Is.EqualTo(expectedJsonExpression),
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
        Assertion = new Action<JsonElement>(static result => {
          Assert.That(result.ValueKind, Is.EqualTo(JsonValueKind.Object));
        })
      },
      new {
        Json = @"{""module"":{""method"":{""Foo"":""Bar""}}}",
        Module = "module",
        Method = "method",
        Assertion = new Action<JsonElement>(static result => {
          Assert.That(result.GetProperty("Foo").GetString(), Is.EqualTo("Bar"));
        })
      },
      new {
        Json = @"{""module"":{""method"":{""Foo"":42}}}",
        Module = "module",
        Method = "method",
        Assertion = new Action<JsonElement>(static result => {
          Assert.That(result.GetProperty("Foo").GetInt32(), Is.EqualTo(42));
        })
      },
      new {
        Json = @"{""module"":{""method"":{""Foo"":{""Bar"":""Baz""}}}}",
        Module = "module",
        Method = "method",
        Assertion = new Action<JsonElement>(static result => {
          var foo = result.GetProperty("Foo");

          Assert.That(foo.ValueKind, Is.EqualTo(JsonValueKind.Object));
          Assert.That(foo.GetProperty("Bar").GetString(), Is.EqualTo("Baz"));
        })
      },
      new {
        Json = @"{""module"":{""method"":null}}",
        Module = "module",
        Method = "method",
        Assertion = new Action<JsonElement>(static result => {
          Assert.That(result.ValueKind, Is.EqualTo(JsonValueKind.Null));
        })
      },
      new {
        Json = @"{"""":{"""":{}}}",
        Module = "",
        Method = "",
        Assertion = new Action<JsonElement>(static result => {
          Assert.That(result.ValueKind, Is.EqualTo(JsonValueKind.Object));
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
        testCase.Assertion
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
  public void Deserialize_MessageHeaderTooShort(
    [Values(0, 1, 2, 3)] int length
  )
  {
    var buffer = new ArrayBufferWriter<byte>();

    if (0 <= length) {
      buffer.GetSpan(length);
      buffer.Advance(length);
    }

    Assert.Throws<KasaMessageHeaderTooShortException>(
      () => KasaJsonSerializer.Deserialize(
        buffer,
        JsonEncodedText.Encode("module"),
        JsonEncodedText.Encode("method")
      )
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_Deserialize_MessageBodyTooShort()
  {
    foreach (var input in new[] {
      new byte[] { 0x00, 0x00, 0x00, 0x01 }, // length = 1 byte, data = 0 bytes
      new byte[] { 0x00, 0x00, 0x00, 0x02, 0x00 }, // length = 2 byte, data = 1 bytes
    }) {
      var bodyLengthIndicatedInHeader = BinaryPrimitives.ReadInt32BigEndian(input.AsSpan(0, 4));

      yield return new object[] {
        CreateWrittenArrayBufferWriter(input),
        bodyLengthIndicatedInHeader
      };
    }

    static ArrayBufferWriter<byte> CreateWrittenArrayBufferWriter(byte[] input)
    {
      var buffer = new ArrayBufferWriter<byte>();

      var span = buffer.GetSpan(input.Length);

      input.AsSpan().CopyTo(span);

      buffer.Advance(input.Length);

      return buffer;
    }
  }

  [TestCaseSource(nameof(YieldTestCases_Deserialize_MessageBodyTooShort))]
  public void Deserialize_MessageBodyTooShort(ArrayBufferWriter<byte> buffer, int bodyLengthIndicatedInHeader)
  {
    var ex = Assert.Throws<KasaMessageBodyTooShortException>(
      () => KasaJsonSerializer.Deserialize(
        buffer,
        JsonEncodedText.Encode("module"),
        JsonEncodedText.Encode("method")
      )
    );

    Assert.That(ex!.IndicatedLength, Is.EqualTo(bodyLengthIndicatedInHeader), nameof(ex.IndicatedLength));
    Assert.That(ex.ActualLength, Is.EqualTo(buffer.WrittenCount - 4), nameof(ex.ActualLength));
  }

  private static System.Collections.IEnumerable YieldTestCases_Deserialize_MessageModuleMismatch()
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

  [TestCaseSource(nameof(YieldTestCases_Deserialize_MessageModuleMismatch))]
  public void Deserialize_MessageModuleMismatch(ArrayBufferWriter<byte> buffer, JsonEncodedText module, JsonEncodedText method)
    => Assert.Throws<KasaMessageException>(
      () => KasaJsonSerializer.Deserialize(
        buffer,
        module,
        method
      )
    );

  private static System.Collections.IEnumerable YieldTestCases_Deserialize_MessageMethodMismatch()
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

  [TestCaseSource(nameof(YieldTestCases_Deserialize_MessageMethodMismatch))]
  public void Deserialize_MessageMethodMismatch(ArrayBufferWriter<byte> buffer, JsonEncodedText module, JsonEncodedText method)
    => Assert.Throws<KasaMessageException>(
      () => KasaJsonSerializer.Deserialize(
        buffer,
        module,
        method
      )
    );
}
