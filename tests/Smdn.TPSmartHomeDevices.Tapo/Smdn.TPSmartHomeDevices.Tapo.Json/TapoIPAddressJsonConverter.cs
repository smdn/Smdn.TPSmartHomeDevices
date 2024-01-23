// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo.Json;

[TestFixture]
public class TapoIPAddressJsonConverterTests {
  private static System.Collections.IEnumerable YieldTestCases_Read()
  {
    yield return new object?[] { "{}", null };
    yield return new object?[] { @"{""ip"": null}", null }; // invalid
    yield return new object?[] { @"{""ip"": 0}", null }; // invalid
    yield return new object?[] { @"{""ip"": ""invalid""}", null }; // invalid
    yield return new object?[] { @"{""ip"": ""999.999.999.999""}", null }; // invalid address
    yield return new object?[] { @"{""ip"": ""00:00:5E:00:53:00""}", null }; // invalid address
    yield return new object?[] { @"{""ip"": ""192.0.2.1""}", IPAddress.Parse("192.0.2.1") };
    yield return new object?[] { @"{""ip"": ""2001:db8::0""}", IPAddress.Parse("2001:db8::0") };
    yield return new object?[] { @"{""ip"": ""2001:0db8:0000:0000:0000:0000:192.0.2.1""}", IPAddress.Parse("2001:0db8:0000:0000:0000:0000:192.0.2.1") };
  }

  private readonly struct NetworkInfo {
    [JsonPropertyName("ip")]
    [JsonConverter(typeof(TapoIPAddressJsonConverter))]
    public IPAddress? IPAddress { get; init; }
  }

  [TestCaseSource(nameof(YieldTestCases_Read))]
  public void Read(string json, IPAddress? expected)
  {
    var deserialized = JsonSerializer.Deserialize<NetworkInfo>(json)!;

    Assert.That(deserialized.IPAddress, Is.EqualTo(expected));
  }

  private static System.Collections.IEnumerable YieldTestCases_Write()
  {
    yield return new object?[] { null, @"{""ip"":null}", null };
    yield return new object?[] { IPAddress.Loopback, @"{""ip"":""127.0.0.1""}", typeof(NotImplementedException) };
    yield return new object?[] { IPAddress.IPv6Loopback, @"{""ip"":""::1""}", typeof(NotImplementedException) };
  }

  [TestCaseSource(nameof(YieldTestCases_Write))]
  public void Write(IPAddress? value, string expected, Type? typeOfExpectedException)
  {
    var obj = new NetworkInfo() { IPAddress = value };

    if (typeOfExpectedException is null)
      Assert.That(JsonSerializer.Serialize(obj), Is.EqualTo(expected));
    else
      Assert.Throws(typeOfExpectedException, () => JsonSerializer.Serialize(obj));
  }
}
