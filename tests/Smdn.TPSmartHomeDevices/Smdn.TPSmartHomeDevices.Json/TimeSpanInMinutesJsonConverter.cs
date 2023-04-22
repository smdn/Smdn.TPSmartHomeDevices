// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Json;

[TestFixture]
public class TimeSpanInMinutesJsonConverterTests {
  private static System.Collections.IEnumerable YieldTestCases_Read()
  {
    yield return new object?[] { "{}", null };
    yield return new object?[] { @"{""time_diff"": null}", null }; // invalid
    yield return new object?[] { @"{""time_diff"": ""invalid""}", null }; // invalid
    yield return new object?[] { @"{""time_diff"": 0.0}", null }; // invalid (decimal notation)
    yield return new object?[] { @"{""time_diff"": 1E400}", null }; // invalid (exponent notation)
    yield return new object?[] { @"{""time_diff"": 0}", TimeSpan.Zero };
    yield return new object?[] { @"{""time_diff"": 540}", TimeSpan.FromHours(+9.0) };
    yield return new object?[] { @"{""time_diff"": 330}", TimeSpan.FromHours(+5.5) };
    yield return new object?[] { @"{""time_diff"": -300}", TimeSpan.FromHours(-5.0) };
  }

  private readonly struct TimeZone {
    [JsonPropertyName("time_diff")]
    [JsonConverter(typeof(TimeSpanInMinutesJsonConverter))]
    public TimeSpan? TimeZoneOffset { get; init; }
  }

  [TestCaseSource(nameof(YieldTestCases_Read))]
  public void Read(string json, TimeSpan? expected)
  {
    var deserialized = JsonSerializer.Deserialize<TimeZone>(json)!;

    Assert.AreEqual(expected, deserialized.TimeZoneOffset);
  }

  private static System.Collections.IEnumerable YieldTestCases_Write()
  {
    yield return new object?[] { null, @"{""time_diff"":null}", null };
    yield return new object?[] { TimeSpan.Zero, @"{""time_diff"":0}", typeof(NotImplementedException) };
    yield return new object?[] { TimeSpan.FromHours(+9.0), @"{""time_diff"":540}", typeof(NotImplementedException) };
    yield return new object?[] { TimeSpan.FromHours(-5.0), @"{""time_diff"":-300}", typeof(NotImplementedException) };
  }

  [TestCaseSource(nameof(YieldTestCases_Write))]
  public void Write(TimeSpan? value, string expected, Type? typeOfExpectedException)
  {
    var obj = new TimeZone() { TimeZoneOffset = value };

    if (typeOfExpectedException is null)
      Assert.AreEqual(expected, JsonSerializer.Serialize(obj));
    else
      Assert.Throws(typeOfExpectedException, () => JsonSerializer.Serialize(obj));
  }
}
