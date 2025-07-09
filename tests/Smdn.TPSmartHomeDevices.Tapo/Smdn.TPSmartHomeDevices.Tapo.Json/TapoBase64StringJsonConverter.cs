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
    yield return new object?[] { /*lang=json,strict*/ @"{""ssid"": null}", null }; // invalid
    yield return new object?[] { /*lang=json,strict*/ @"{""ssid"": 0}", null }; // invalid
    yield return new object?[] { /*lang=json,strict*/ @"{""ssid"": ""invalid""}", null }; // invalid
    yield return new object?[] { /*lang=json,strict*/ @"{""ssid"": ""4=""}", null }; // invalid base64
    yield return new object?[] { /*lang=json,strict*/ @"{""ssid"": """"}", string.Empty };
    yield return new object?[] { /*lang=json,strict*/ @"{""ssid"": ""SE9NRQ==""}", "HOME" };
    yield return new object?[] { /*lang=json,strict*/ @"{""ssid"": ""RlJFRS1XSUZJ""}", "FREE-WIFI" };
    yield return new object?[] { /*lang=json,strict*/ @"{""ssid"": ""6L+344GE54yr44Kq44O844OQ44O8TEFO""}", "迷い猫オーバーLAN" };
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

    Assert.That(deserialized.NetworkSsid, Is.EqualTo(expected));
  }

  private static System.Collections.IEnumerable YieldTestCases_Write()
  {
    yield return new object?[] { null, /*lang=json,strict*/ @"{""ssid"":null}", null };
    yield return new object?[] { string.Empty, /*lang=json,strict*/ @"{""ssid"":""""}", typeof(NotImplementedException) };
    yield return new object?[] { "HOME", /*lang=json,strict*/ @"{""ssid"":""SE9NRQ==""}", typeof(NotImplementedException) };
    yield return new object?[] { "FREE-WIFI", /*lang=json,strict*/ @"{""ssid"":""RlJFRS1XSUZJ""}", typeof(NotImplementedException) };
    yield return new object?[] { "迷い猫オーバーLAN", /*lang=json,strict*/ @"{""ssid"":""6L+344GE54yr44Kq44O844OQ44O8TEFO""}", typeof(NotImplementedException) };
  }

  [TestCaseSource(nameof(YieldTestCases_Write))]
  public void Write(string? value, string expected, Type? typeOfExpectedException)
  {
    var obj = new NetworkInfo() { NetworkSsid = value };

    if (typeOfExpectedException is null)
      Assert.That(JsonSerializer.Serialize(obj), Is.EqualTo(expected));
    else
      Assert.Throws(typeOfExpectedException, () => JsonSerializer.Serialize(obj));
  }
}
