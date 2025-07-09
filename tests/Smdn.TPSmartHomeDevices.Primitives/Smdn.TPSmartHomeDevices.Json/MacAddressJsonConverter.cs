// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Serialization;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Json;

[TestFixture]
public class MacAddressJsonConverterTests {
  private static System.Collections.IEnumerable YieldTestCases_Read()
  {
    yield return new object?[] { "{}", null };
    yield return new object?[] { /*lang=json,strict*/ @"{""mac"": null}", null }; // invalid
    yield return new object?[] { /*lang=json,strict*/ @"{""mac"": 0}", null }; // invalid
    yield return new object?[] { /*lang=json,strict*/ @"{""mac"": ""invalid""}", null }; // invalid
    yield return new object?[] { /*lang=json,strict*/ @"{""mac"": ""00:00:5E:00:53:XX""}", null }; // invalid
    yield return new object?[] { /*lang=json,strict*/ @"{""mac"": ""0x00005E005300""}", null }; // invalid
    yield return new object?[] { /*lang=json,strict*/ @"{""mac"": ""00005E005300""}", new PhysicalAddress(new byte[6] { 0x00, 0x00, 0x5E, 0x00, 0x53, 0x00 }) };
    yield return new object?[] { /*lang=json,strict*/ @"{""mac"": ""00:00:5E:00:53:00""}", new PhysicalAddress(new byte[6] { 0x00, 0x00, 0x5E, 0x00, 0x53, 0x00 }) };
    yield return new object?[] { /*lang=json,strict*/ @"{""mac"": ""00-00-5E-00-53-00""}", new PhysicalAddress(new byte[6] { 0x00, 0x00, 0x5E, 0x00, 0x53, 0x00 }) };
    yield return new object?[] { /*lang=json,strict*/ @"{""mac"": ""00:00:00:00:00:00""}", new PhysicalAddress(new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }) };
  }

  private readonly struct EndPoint {
    [JsonPropertyName("mac")]
    [JsonConverter(typeof(MacAddressJsonConverter))]
    public PhysicalAddress? MacAddress { get; init; }
  }

  [TestCaseSource(nameof(YieldTestCases_Read))]
  public void Read(string json, PhysicalAddress? expected)
  {
    var deserialized = JsonSerializer.Deserialize<EndPoint>(json)!;

    Assert.That(deserialized.MacAddress, Is.EqualTo(expected));
  }

  private static System.Collections.IEnumerable YieldTestCases_Write()
  {
    yield return new object?[] { null, /*lang=json,strict*/ @"{""mac"":null}", null };
    yield return new object?[] { PhysicalAddress.None, /*lang=json,strict*/ @"{""mac"":""""}", typeof(NotImplementedException) }; // This should be invalid?
    yield return new object?[] { new PhysicalAddress(new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }), /*lang=json,strict*/ @"{""mac"":""00:00:00:00:00:00""}", typeof(NotImplementedException) };
    yield return new object?[] { new PhysicalAddress(new byte[6] { 0x00, 0x00, 0x5E, 0x00, 0x53, 0x00 }), /*lang=json,strict*/ @"{""mac"":""00:00:5E:00:53:00""}", typeof(NotImplementedException) };
  }

  [TestCaseSource(nameof(YieldTestCases_Write))]
  public void Write(PhysicalAddress? value, string expected, Type? typeOfExpectedException)
  {
    var obj = new EndPoint() { MacAddress = value };

    if (typeOfExpectedException is null)
      Assert.That(JsonSerializer.Serialize(obj), Is.EqualTo(expected));
    else
      Assert.Throws(typeOfExpectedException, () => JsonSerializer.Serialize(obj));
  }
}
