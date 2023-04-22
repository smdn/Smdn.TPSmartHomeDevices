// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo.Json;

[TestFixture]
public class TapoBase64StringJsonConverterTests {
  private static System.Collections.IEnumerable YieldTestCases_Read()
  {
    yield return new object?[] { "{}", null };
    yield return new object?[] { @"{""ssid"": null}", null }; // invalid
    yield return new object?[] { @"{""ssid"": 0}", null }; // invalid
    yield return new object?[] { @"{""ssid"": ""invalid""}", null }; // invalid
    yield return new object?[] { @"{""ssid"": ""4=""}", null }; // invalid base64
    yield return new object?[] { @"{""ssid"": """"}", string.Empty };
    yield return new object?[] { @"{""ssid"": ""SE9NRQ==""}", "HOME" };
    yield return new object?[] { @"{""ssid"": ""RlJFRS1XSUZJ""}", "FREE-WIFI" };
    yield return new object?[] { @"{""ssid"": ""6L+344GE54yr44Kq44O844OQ44O8TEFO""}", "迷い猫オーバーLAN" };
  }

  private readonly struct NetworkInfo {
    [JsonPropertyName("ssid")]
    [JsonConverter(typeof(TapoBase64StringJsonConverter))]
    public string? NetworkSsid { get; init; }
  }

  [TestCaseSource(nameof(YieldTestCases_Read))]
  public void Read(string json, string? expected)
  {
    var deserialized = JsonSerializer.Deserialize<NetworkInfo>(json)!;

    Assert.AreEqual(expected, deserialized.NetworkSsid);
  }

  private static System.Collections.IEnumerable YieldTestCases_Write()
  {
    yield return new object?[] { null, @"{""ssid"":null}", null };
    yield return new object?[] { string.Empty, @"{""ssid"":""""}", typeof(NotImplementedException) };
    yield return new object?[] { "HOME", @"{""ssid"":""SE9NRQ==""}", typeof(NotImplementedException) };
    yield return new object?[] { "FREE-WIFI", @"{""ssid"":""RlJFRS1XSUZJ""}", typeof(NotImplementedException) };
    yield return new object?[] { "迷い猫オーバーLAN", @"{""ssid"":""6L+344GE54yr44Kq44O844OQ44O8TEFO""}", typeof(NotImplementedException) };
  }

  [TestCaseSource(nameof(YieldTestCases_Write))]
  public void Write(string? value, string expected, Type? typeOfExpectedException)
  {
    var obj = new NetworkInfo() { NetworkSsid = value };

    if (typeOfExpectedException is null)
      Assert.AreEqual(expected, JsonSerializer.Serialize(obj));
    else
      Assert.Throws(typeOfExpectedException, () => JsonSerializer.Serialize(obj));
  }
}
