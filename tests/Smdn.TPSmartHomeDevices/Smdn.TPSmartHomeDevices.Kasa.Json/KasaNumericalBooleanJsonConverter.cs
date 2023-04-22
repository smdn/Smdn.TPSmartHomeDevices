// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json;
using System.Text.Json.Serialization;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Kasa.Json;

[TestFixture]
public class KasaNumericalBooleanJsonConverterTests {
  private readonly struct OnOffState {
    [JsonPropertyName("on_off")]
    [JsonConverter(typeof(KasaNumericalBooleanJsonConverter))]
    public bool IsOn { get; init; }
  }

  private static System.Collections.IEnumerable YieldTestCases_Read()
  {
    yield return new object?[] { "{}", false };
    yield return new object?[] { @"{""on_off"": null}", false }; // invalid
    yield return new object?[] { @"{""on_off"": ""invalid""}", false }; // invalid
    yield return new object?[] { @"{""on_off"": ""0""}", false }; // invalid
    yield return new object?[] { @"{""on_off"": 0}", false };
    yield return new object?[] { @"{""on_off"": 1}", true };
    yield return new object?[] { @"{""on_off"": 2}", true };
    yield return new object?[] { @"{""on_off"": -1}", true };
  }

  [TestCaseSource(nameof(YieldTestCases_Read))]
  public void Read(string json, bool expected)
  {
    var deserialized = JsonSerializer.Deserialize<OnOffState>(json)!;

    Assert.AreEqual(expected, deserialized.IsOn);
  }

  [TestCase(true, @"{""on_off"":1}")]
  [TestCase(false, @"{""on_off"":0}")]
  public void Write(bool isOn, string expected)
    => Assert.AreEqual(
      expected,
      JsonSerializer.Serialize(new OnOffState() { IsOn = isOn } )
    );
}
