// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Json;

[TestFixture]
public class GeolocationInDecimalDegreesJsonConverterTests {
  private static System.Collections.IEnumerable YieldTestCases_Read()
  {
    yield return new object?[] { "{}", null };
    yield return new object?[] { @"{""longitude"": null}", null }; // invalid
    yield return new object?[] { @"{""longitude"": ""invalid""}", null }; // invalid
    yield return new object?[] { @"{""longitude"": ""0""}", null }; // invalid
    yield return new object?[] { @"{""longitude"": 0}", 0.0m };
    yield return new object?[] { @"{""longitude"": 1}", 0.0001m };
    yield return new object?[] { @"{""longitude"": -1}", -0.0001m };
    yield return new object?[] { @"{""longitude"": 12345}", 1.2345m };
    yield return new object?[] { @"{""longitude"": 12345.6}", 1.23456m };
    yield return new object?[] { @"{""longitude"": 67890}", 6.789m };
  }

  private readonly struct Geolocation {
    [JsonPropertyName("longitude")]
    [JsonConverter(typeof(GeolocationInDecimalDegreesJsonConverter))]
    public decimal? Longitude { get; init; }
  }

  [TestCaseSource(nameof(YieldTestCases_Read))]
  public void Read(string json, decimal? expected)
  {
    var deserialized = JsonSerializer.Deserialize<Geolocation>(json)!;

    Assert.That(deserialized.Longitude, Is.EqualTo(expected));
  }

  private static System.Collections.IEnumerable YieldTestCases_Write()
  {
    yield return new object?[] { null, @"{""longitude"":null}", null };
    yield return new object?[] { 0.0m, @"{""longitude"":0}", typeof(NotImplementedException) };
    yield return new object?[] { 0.1m, @"{""longitude"":0.1}", typeof(NotImplementedException) };
    yield return new object?[] { -1m, @"{""longitude"":-11}", typeof(NotImplementedException) };
    yield return new object?[] { 12345m, @"{""longitude"":12345}", typeof(NotImplementedException) };
  }

  [TestCaseSource(nameof(YieldTestCases_Write))]
  public void Write(decimal? value, string expected, Type? typeOfExpectedException)
  {
    var obj = new Geolocation() { Longitude = value };

    if (typeOfExpectedException is null)
      Assert.That(JsonSerializer.Serialize(obj), Is.EqualTo(expected));
    else
      Assert.Throws(typeOfExpectedException, () => JsonSerializer.Serialize(obj));
  }
}
