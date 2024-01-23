// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo;

[TestFixture]
public class TapoDeviceInfoTests {
  [Test]
  public void DeserializeCommonProperties_FromEmptyJsonDocument()
  {
    TapoDeviceInfo? info = null;

    Assert.DoesNotThrow(
      () => info = JsonSerializer.Deserialize<TapoDeviceInfo>("{}")
    );

    Assert.That(info, Is.Not.Null);
    Assert.That(DateTimeOffset.MinValue, Is.Not.EqualTo(info!.TimeStamp), nameof(info.TimeStamp));
    Assert.That(info.Id, Is.Null, nameof(info.Id));
    Assert.That(info.IsOn, Is.False, nameof(info.IsOn));
    Assert.That(info.NetworkSignalLevel, Is.Null, nameof(info.NetworkSignalLevel));
  }

  [Test]
  public void DeserializeCommonProperties()
  {
    var info = JsonSerializer.Deserialize<TapoDeviceInfo>($@"{{
""device_id"": ""0123456789ABCDEF"",
""type"": ""<type>"",
""model"": ""<model>"",
""fw_id"": ""23456789ABCDEF01"",
""fw_ver"": ""<fw_ver>"",
""hw_id"": ""456789ABCDEF0123"",
""hw_ver"": ""<hw_ver>"",
""oem_id"": ""6789ABCDEF012345"",
""mac"": ""00:00:5E:00:53:00"",
""specs"": ""<specs>"",
""lang"": ""<lang>"",
""device_on"": true,
""on_time"": 1234,
""overheated"": true,
""nickname"": ""{Convert.ToBase64String(Encoding.UTF8.GetBytes("<nickname>"))}"",
""avatar"": ""<avatar>"",
""time_diff"": 540,
""region"": ""<region>"",
""longitude"": 12345,
""latitude"": 67890,
""has_set_location_info"": true,
""ip"": ""192.0.2.1"",
""ssid"": ""{Convert.ToBase64String(Encoding.UTF8.GetBytes("<ssid>"))}"",
""signal_level"": 999,
""rssi"": 99.999
}}");

    Assert.That(info, Is.Not.Null);
    Assert.That(DateTimeOffset.MinValue, Is.Not.EqualTo(info!.TimeStamp), nameof(info.TimeStamp));
    Assert.That(info!.Id, Is.EqualTo(new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF }).AsCollection, nameof(info.Id));
    Assert.That(info!.TypeName, Is.EqualTo("<type>"), nameof(info.TypeName));
    Assert.That(info!.ModelName, Is.EqualTo("<model>"), nameof(info.ModelName));
    Assert.That(info!.FirmwareId, Is.EqualTo(new byte[] { 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01 }).AsCollection, nameof(info.FirmwareId));
    Assert.That(info!.FirmwareVersion, Is.EqualTo("<fw_ver>"), nameof(info.FirmwareVersion));
    Assert.That(info!.HardwareId, Is.EqualTo(new byte[] { 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23 }).AsCollection, nameof(info.HardwareId));
    Assert.That(info!.HardwareVersion, Is.EqualTo("<hw_ver>"), nameof(info.HardwareVersion));
    Assert.That(info!.OemId, Is.EqualTo(new byte[] { 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45 }).AsCollection, nameof(info.OemId));
    Assert.That(info!.MacAddress, Is.EqualTo(PhysicalAddress.Parse("00:00:5E:00:53:00")), nameof(info.MacAddress));
    Assert.That(info!.HardwareSpecifications, Is.EqualTo("<specs>"), nameof(info.HardwareSpecifications));
    Assert.That(info!.Language, Is.EqualTo("<lang>"), nameof(info.Language));
    Assert.That(info!.IsOn, Is.True, nameof(info.IsOn));
    Assert.That(info!.OnTimeDuration, Is.EqualTo(TimeSpan.FromSeconds(1234)), nameof(info.OnTimeDuration));
    Assert.That(info!.IsOverheated, Is.True, nameof(info.IsOverheated));
    Assert.That(info!.NickName, Is.EqualTo("<nickname>"), nameof(info.NickName));
    Assert.That(info!.Avatar, Is.EqualTo("<avatar>"), nameof(info.Avatar));
    Assert.That(info!.TimeZoneOffset, Is.EqualTo(TimeSpan.FromHours(+9.0)), nameof(info.TimeZoneOffset));
    Assert.That(info!.TimeZoneRegion, Is.EqualTo("<region>"), nameof(info.TimeZoneOffset));
    Assert.That(info!.GeolocationLongitude, Is.EqualTo(1.2345m), nameof(info.GeolocationLongitude));
    Assert.That(info!.GeolocationLatitude, Is.EqualTo(6.7890m), nameof(info.GeolocationLatitude));
    Assert.That(info!.HasGeolocationInfoSet, Is.True, nameof(info.HasGeolocationInfoSet));
    Assert.That(info!.IPAddress, Is.EqualTo(System.Net.IPAddress.Parse("192.0.2.1")), nameof(info.IPAddress));
    Assert.That(info!.NetworkSsid, Is.EqualTo("<ssid>"), nameof(info.NetworkSsid));
    Assert.That(info!.NetworkSignalLevel, Is.EqualTo(999), nameof(info.NetworkSignalLevel));
    Assert.That(info!.NetworkRssi, Is.EqualTo(99.999m), nameof(info.NetworkRssi));
  }

  private static System.Collections.Generic.IEnumerable<(string, byte[]?)> YieldTestCases_IdProperties()
  {
    yield return ("{}", null);
    yield return (@"{""<ID>"": ""invalid""}", null);
    yield return (@"{""<ID>"": ""0""}", null); // invalid
    yield return (@"{""<ID>"": ""012""}", null); // invalid
    yield return (@"{""<ID>"": """"}", Array.Empty<byte>());
    yield return (@"{""<ID>"": ""01""}", new byte[] { 0x01 });
    yield return (@"{""<ID>"": ""0123456789ABCDEF""}", new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF });
  }

  private static System.Collections.IEnumerable YieldTestCases_Id()
    => YieldTestCases_IdProperties()
      .Select(static ((string Json, byte[]? Expected) testCase) => new object?[] { testCase.Json.Replace("<ID>", "device_id"), testCase.Expected });

  [TestCaseSource(nameof(YieldTestCases_Id))]
  public void Id(string json, byte[]? expected)
  {
    var info = JsonSerializer.Deserialize<TapoDeviceInfo>(json)!;

    Assert.That(info, Is.Not.Null);
    Assert.That(info.Id, Is.EqualTo(expected).AsCollection);
  }

  private static System.Collections.IEnumerable YieldTestCases_HardwareId()
    => YieldTestCases_IdProperties()
      .Select(static ((string Json, byte[]? Expected) testCase) => new object?[] { testCase.Json.Replace("<ID>", "hw_id"), testCase.Expected });

  [TestCaseSource(nameof(YieldTestCases_HardwareId))]
  public void HardwareId(string json, byte[]? expected)
  {
    var info = JsonSerializer.Deserialize<TapoDeviceInfo>(json)!;

    Assert.That(info, Is.Not.Null);
    Assert.That(info.HardwareId, Is.EqualTo(expected).AsCollection);
  }

  private static System.Collections.IEnumerable YieldTestCases_FirmwareId()
    => YieldTestCases_IdProperties()
      .Select(static ((string Json, byte[]? Expected) testCase) => new object?[] { testCase.Json.Replace("<ID>", "fw_id"), testCase.Expected });

  [TestCaseSource(nameof(YieldTestCases_FirmwareId))]
  public void FirmwareId(string json, byte[]? expected)
  {
    var info = JsonSerializer.Deserialize<TapoDeviceInfo>(json)!;

    Assert.That(info, Is.Not.Null);
    Assert.That(info.FirmwareId, Is.EqualTo(expected).AsCollection);
  }

  private static System.Collections.IEnumerable YieldTestCases_OemId()
    => YieldTestCases_IdProperties()
      .Select(static ((string Json, byte[]? Expected) testCase) => new object?[] { testCase.Json.Replace("<ID>", "oem_id"), testCase.Expected });

  [TestCaseSource(nameof(YieldTestCases_OemId))]
  public void OemId(string json, byte[]? expected)
  {
    var info = JsonSerializer.Deserialize<TapoDeviceInfo>(json)!;

    Assert.That(info, Is.Not.Null);
    Assert.That(info.OemId, Is.EqualTo(expected).AsCollection);
  }

  private static System.Collections.IEnumerable YieldTestCases_MacAddress()
  {
    yield return new object?[] { "{}", null };
    yield return new object?[] { @"{""mac"": ""invalid""}", null };
    yield return new object?[] { @"{""mac"": ""00:00:5E:00:53:XX""}", null }; // invalid
    yield return new object?[] { @"{""mac"": ""00:00:5E:00:53:00""}", new PhysicalAddress(new byte[6] { 0x00, 0x00, 0x5E, 0x00, 0x53, 0x00 }) };
    yield return new object?[] { @"{""mac"": ""00-00-5E-00-53-00""}", new PhysicalAddress(new byte[6] { 0x00, 0x00, 0x5E, 0x00, 0x53, 0x00 }) };
    yield return new object?[] { @"{""mac"": ""00:00:00:00:00:00""}", new PhysicalAddress(new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }) };
  }

  [TestCaseSource(nameof(YieldTestCases_MacAddress))]
  public void MacAddress(string json, PhysicalAddress? expected)
  {
    var info = JsonSerializer.Deserialize<TapoDeviceInfo>(json)!;

    Assert.That(info, Is.Not.Null);
    Assert.That(info.MacAddress, Is.EqualTo(expected));
  }

  [TestCase("{}", false)]
  [TestCase(@"{""device_on"": true}", true)]
  [TestCase(@"{""device_on"": false}", false)]
  public void IsOn(string json, bool expected)
  {
    var info = JsonSerializer.Deserialize<TapoDeviceInfo>(json)!;

    Assert.That(info, Is.Not.Null);
    Assert.That(info.IsOn, Is.EqualTo(expected));
  }

  private static System.Collections.IEnumerable YieldTestCases_OnTimeDuration()
  {
    yield return new object?[] { "{}", null };
    yield return new object?[] { @"{""on_time"": null}", null };
    yield return new object?[] { @"{""on_time"": 0.0}", null }; // invalid (decimal notation)
    yield return new object?[] { @"{""on_time"": 1E400}", null }; // invalid (exponent notation)
    yield return new object?[] { @"{""on_time"": 0}", TimeSpan.Zero };
    yield return new object?[] { @"{""on_time"": 1}", TimeSpan.FromSeconds(1) };
    yield return new object?[] { @"{""on_time"": -1}", TimeSpan.FromSeconds(-1) };
  }

  [TestCaseSource(nameof(YieldTestCases_OnTimeDuration))]
  public void OnTimeDuration(string json, TimeSpan? expected)
  {
    var info = JsonSerializer.Deserialize<TapoDeviceInfo>(json)!;

    Assert.That(info, Is.Not.Null);
    Assert.That(info.OnTimeDuration, Is.EqualTo(expected));
  }

  [TestCase("{}", false)]
  [TestCase(@"{""overheated"": true}", true)]
  [TestCase(@"{""overheated"": false}", false)]
  public void IsOverheated(string json, bool expected)
  {
    var info = JsonSerializer.Deserialize<TapoDeviceInfo>(json)!;

    Assert.That(info, Is.Not.Null);
    Assert.That(info.IsOverheated, Is.EqualTo(expected));
  }

  [TestCase("{}", null)]
  [TestCase(@"{""nickname"": null}", null)]
  [TestCase(@"{""nickname"": ""invalid""}", null)]
  [TestCase(@"{""nickname"": ""4=""}", null)] // invalid
  [TestCase(@"{""nickname"": """"}", "")]
  [TestCase(@"{""nickname"": ""TklDS05BTUU=""}", "NICKNAME")]
  [TestCase(@"{""nickname"": ""44OL44OD44Kv44ON44O844Og""}", "ニックネーム")]
  public void NickName(string json, string? expected)
  {
    var info = JsonSerializer.Deserialize<TapoDeviceInfo>(json)!;

    Assert.That(info, Is.Not.Null);
    Assert.That(info.NickName, Is.EqualTo(expected));
  }

  private static System.Collections.IEnumerable YieldTestCases_TimeZoneOffset()
  {
    yield return new object?[] { "{}", null };
    yield return new object?[] { @"{""time_diff"": null}", null };
    yield return new object?[] { @"{""time_diff"": 0.0}", null }; // invalid (decimal notation)
    yield return new object?[] { @"{""time_diff"": 1E400}", null }; // invalid (exponent notation)
    yield return new object?[] { @"{""time_diff"": 0}", TimeSpan.Zero };
    yield return new object?[] { @"{""time_diff"": 540}", TimeSpan.FromHours(+9.0) };
    yield return new object?[] { @"{""time_diff"": 330}", TimeSpan.FromHours(+5.5) };
    yield return new object?[] { @"{""time_diff"": -300}", TimeSpan.FromHours(-5.0) };
  }

  [TestCaseSource(nameof(YieldTestCases_TimeZoneOffset))]
  public void TimeZoneOffset(string json, TimeSpan? expected)
  {
    var info = JsonSerializer.Deserialize<TapoDeviceInfo>(json)!;

    Assert.That(info, Is.Not.Null);
    Assert.That(info.TimeZoneOffset, Is.EqualTo(expected));
  }

  private static System.Collections.IEnumerable YieldTestCases_GeolocationLongitude()
  {
    yield return new object?[] { "{}", null };
    yield return new object?[] { @"{""longitude"": null}", null };
    yield return new object?[] { @"{""longitude"": 0}", 0m };
    yield return new object?[] { @"{""longitude"": 0.0}", 0m };
    yield return new object?[] { @"{""longitude"": 1397666}", 139.7666m };
    yield return new object?[] { @"{""longitude"": 225414}", 22.5414m };
  }

  [TestCaseSource(nameof(YieldTestCases_GeolocationLongitude))]
  public void GeolocationLongitude(string json, decimal? expected)
  {
    var info = JsonSerializer.Deserialize<TapoDeviceInfo>(json)!;

    Assert.That(info, Is.Not.Null);
    Assert.That(info.GeolocationLongitude, Is.EqualTo(expected));
  }

  private static System.Collections.IEnumerable YieldTestCases_GeolocationLatitude()
  {
    yield return new object?[] { "{}", null };
    yield return new object?[] { @"{""latitude"": null}", null };
    yield return new object?[] { @"{""latitude"": 0}", 0m };
    yield return new object?[] { @"{""latitude"": 0.0}", 0m };
    yield return new object?[] { @"{""latitude"": 356813}", 35.6813m };
    yield return new object?[] { @"{""latitude"": -762705}", -76.2705m };
  }

  [TestCaseSource(nameof(YieldTestCases_GeolocationLatitude))]
  public void GeolocationLatitude(string json, decimal? expected)
  {
    var info = JsonSerializer.Deserialize<TapoDeviceInfo>(json)!;

    Assert.That(info, Is.Not.Null);
    Assert.That(info.GeolocationLatitude, Is.EqualTo(expected));
  }

  [TestCase("{}", false)]
  [TestCase(@"{""has_set_location_info"": true}", true)]
  [TestCase(@"{""has_set_location_info"": false}", false)]
  public void HasGeolocationInfoSet(string json, bool expected)
  {
    var info = JsonSerializer.Deserialize<TapoDeviceInfo>(json)!;

    Assert.That(info, Is.Not.Null);
    Assert.That(info.HasGeolocationInfoSet, Is.EqualTo(expected));
  }

  private static System.Collections.IEnumerable YieldTestCases_IPAddress()
  {
    yield return new object?[] { "{}", null };
    yield return new object?[] { @"{""ip"": ""invalid""}", null };
    yield return new object?[] { @"{""ip"": ""999.999.999.999""}", null }; // invalid
    yield return new object?[] { @"{""ip"": ""00:00:5E:00:53:00""}", null }; // invalid
    yield return new object?[] { @"{""ip"": ""192.0.2.1""}", System.Net.IPAddress.Parse("192.0.2.1") };
    yield return new object?[] { @"{""ip"": ""2001:db8::0""}", System.Net.IPAddress.Parse("2001:db8::0") };
    yield return new object?[] { @"{""ip"": ""2001:0db8:0000:0000:0000:0000:192.0.2.1""}", System.Net.IPAddress.Parse("2001:0db8:0000:0000:0000:0000:192.0.2.1") };
  }

  [TestCaseSource(nameof(YieldTestCases_IPAddress))]
  public void IPAddress(string json, IPAddress? expected)
  {
    var info = JsonSerializer.Deserialize<TapoDeviceInfo>(json)!;

    Assert.That(info, Is.Not.Null);
    Assert.That(info.IPAddress, Is.EqualTo(expected));
  }

  [TestCase("{}", null)]
  [TestCase(@"{""ssid"": null}", null)]
  [TestCase(@"{""ssid"": ""invalid""}", null)]
  [TestCase(@"{""ssid"": ""4=""}", null)] // invalid
  [TestCase(@"{""ssid"": """"}", "")]
  [TestCase(@"{""ssid"": ""RlJFRS1XSUZJ""}", "FREE-WIFI")]
  [TestCase(@"{""ssid"": ""6L+344GE54yr44Kq44O844OQ44O8TEFO""}", "迷い猫オーバーLAN")]
  public void NetworkSsid(string json, string? expected)
  {
    var info = JsonSerializer.Deserialize<TapoDeviceInfo>(json)!;

    Assert.That(info, Is.Not.Null);
    Assert.That(info.NetworkSsid, Is.EqualTo(expected));
  }

  private static System.Collections.IEnumerable YieldTestCases_NetworkRssi()
  {
    yield return new object?[] { "{}", null };
    yield return new object?[] { @"{""rssi"": null}", null };
    yield return new object?[] { @"{""rssi"": 0}", 0m };
    yield return new object?[] { @"{""rssi"": 0.0}", 0m };
    yield return new object?[] { @"{""rssi"": -30.0}", -30.0m };
    yield return new object?[] { @"{""rssi"": -30.1234567890123456789}", -30.1234567890123456789m };
    yield return new object?[] { @"{""rssi"": 999}", 999m };
  }

  [TestCaseSource(nameof(YieldTestCases_NetworkRssi))]
  public void NetworkRssi(string json, decimal? expected)
  {
    var info = JsonSerializer.Deserialize<TapoDeviceInfo>(json)!;

    Assert.That(info, Is.Not.Null);
    Assert.That(info.NetworkRssi, Is.EqualTo(expected));
  }
}
