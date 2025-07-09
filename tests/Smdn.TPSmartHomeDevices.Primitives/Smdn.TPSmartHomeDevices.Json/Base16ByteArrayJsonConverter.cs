// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Json;

[TestFixture]
public class Base16ByteArrayJsonConverterTests {
  private static System.Collections.IEnumerable YieldTestCases_Read()
  {
    yield return new object?[] { "{}", null };
    yield return new object?[] { /*lang=json,strict*/ @"{""id"": null}", null };
    yield return new object?[] { /*lang=json,strict*/ @"{""id"": ""0""}", null }; // invalid
    yield return new object?[] { /*lang=json,strict*/ @"{""id"": ""XX""}", null }; // invalid
    yield return new object?[] { /*lang=json,strict*/ @"{""id"": ""000""}", null }; // invalid
    yield return new object?[] { /*lang=json,strict*/ @"{""id"": ""000X""}", null }; // invalid
    yield return new object?[] { /*lang=json,strict*/ @"{""id"": 1234}", null }; // invalid
    yield return new object?[] { /*lang=json,strict*/ @"{""id"": """"}", Array.Empty<byte>() };
    yield return new object?[] { /*lang=json,strict*/ @"{""id"": ""0123456789ABCDEF""}", new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF } };
    yield return new object?[] { /*lang=json,strict*/ @"{""id"": ""0123456789abcdef""}", new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF } };
  }

  private readonly struct FirmwareInfo {
    [JsonPropertyName("id")]
    [JsonConverter(typeof(Base16ByteArrayJsonConverter))]
    public byte[]? Id { get; init; }
  }

  [TestCaseSource(nameof(YieldTestCases_Read))]
  public void Read(string json, byte[]? expected)
  {
    var deserialized = JsonSerializer.Deserialize<FirmwareInfo>(json)!;

    Assert.That(deserialized.Id, Is.EqualTo(expected));
  }

  private static System.Collections.IEnumerable YieldTestCases_Write()
  {
    yield return new object?[] { null, null };
    yield return new object?[] { Array.Empty<byte>(), typeof(NotImplementedException) };
    yield return new object?[] { new byte[] { 0x01 }, typeof(NotImplementedException) };
    yield return new object?[] { new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF }, typeof(NotImplementedException) };
  }

  [TestCaseSource(nameof(YieldTestCases_Write))]
  public void Write(byte[]? value, Type? typeOfExpectedException)
  {
    var obj = new FirmwareInfo() { Id = value };

    Assert.That(
      () => JsonSerializer.Serialize(obj),
      typeOfExpectedException is null ? Throws.Nothing : Throws.TypeOf(typeOfExpectedException)
    );
  }
}
