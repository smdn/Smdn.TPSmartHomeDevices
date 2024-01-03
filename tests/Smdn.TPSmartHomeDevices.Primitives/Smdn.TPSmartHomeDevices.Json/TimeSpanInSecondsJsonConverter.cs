// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Json;

[TestFixture]
public class TimeSpanInSecondsJsonConverterTests {
  private static System.Collections.IEnumerable YieldTestCases_Read()
  {
    yield return new object?[] { "{}", null };
    yield return new object?[] { @"{""on_time"": null}", null }; // invalid
    yield return new object?[] { @"{""on_time"": ""invalid""}", null }; // invalid
    yield return new object?[] { @"{""on_time"": 0.0}", null }; // invalid (decimal notation)
    yield return new object?[] { @"{""on_time"": 1E400}", null }; // invalid (exponent notation)
    yield return new object?[] { @"{""on_time"": 0}", TimeSpan.Zero };
    yield return new object?[] { @"{""on_time"": 1}", TimeSpan.FromSeconds(1) };
    yield return new object?[] { @"{""on_time"": -1}", TimeSpan.FromSeconds(-1) }; // This should be invalid?
  }

  private readonly struct DeviceOnTime {
    [JsonPropertyName("on_time")]
    [JsonConverter(typeof(TimeSpanInSecondsJsonConverter))]
    public TimeSpan? OnTimeDuration { get; init; }
  }

  [TestCaseSource(nameof(YieldTestCases_Read))]
  public void Read(string json, TimeSpan? expected)
  {
    var deserialized = JsonSerializer.Deserialize<DeviceOnTime>(json)!;

    Assert.That(deserialized.OnTimeDuration, Is.EqualTo(expected));
  }

  private static System.Collections.IEnumerable YieldTestCases_Write()
  {
    yield return new object?[] { null, @"{""on_time"":null}", null };
    yield return new object?[] { TimeSpan.Zero, @"{""on_time"":0}", typeof(NotImplementedException) };
    yield return new object?[] { TimeSpan.FromSeconds(1), @"{""on_time"":1}", typeof(NotImplementedException) };
    yield return new object?[] { TimeSpan.FromSeconds(-1), @"{""on_time"":-1}", typeof(NotImplementedException) }; // This should be invalid?
  }

  [TestCaseSource(nameof(YieldTestCases_Write))]
  public void Write(TimeSpan? value, string expected, Type? typeOfExpectedException)
  {
    var obj = new DeviceOnTime() { OnTimeDuration = value };

    if (typeOfExpectedException is null)
      Assert.That(JsonSerializer.Serialize(obj), Is.EqualTo(expected));
    else
      Assert.Throws(typeOfExpectedException, () => JsonSerializer.Serialize(obj));
  }
}
