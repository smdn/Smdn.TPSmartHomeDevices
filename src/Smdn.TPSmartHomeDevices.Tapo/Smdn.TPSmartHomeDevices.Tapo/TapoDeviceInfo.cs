// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;
using Smdn.TPSmartHomeDevices.Json;
using Smdn.TPSmartHomeDevices.Tapo.Json;

namespace Smdn.TPSmartHomeDevices.Tapo;

/// <summary>
/// Represents Tapo device information, including current status.
/// </summary>
/// <seealso cref="TapoDevice.GetDeviceInfoAsync(System.Threading.CancellationToken)"/>
public class TapoDeviceInfo {
  /// <summary>
  /// Gets the <see cref="DateTimeOffset"/> that represents the time at which this instance was acquired.
  /// </summary>
  [JsonIgnore]
  public DateTimeOffset TimeStamp { get; } = DateTimeOffset.Now;

  /*
   * properties for the informations common to the devices
   */

#pragma warning disable CA1819
  /// <summary>Gets the Tapo device's ID.</summary>
  /// <remarks>The value will be a <see cref="string"/> representing a HEX-encoded(Base16) byte array.</remarks>
  [JsonPropertyName("device_id")]
  [JsonConverter(typeof(TapoBase16ByteArrayJsonConverter))]
  public byte[]? Id { get; init; }
#pragma warning restore CA1819

  /// <summary>Gets the Tapo device's type name.</summary>
  /// <remarks>The value would be like <c>SMART.TAPOBULB</c> for Tapo L530, as an example.</remarks>
  [JsonPropertyName("type")]
  public string? TypeName { get; init; }

  /// <summary>Gets the Tapo device's model name.</summary>
  /// <remarks>The value would be like <c>L530 Series</c> for Tapo L530, as an example.</remarks>
  [JsonPropertyName("model")]
  public string? ModelName { get; init; }

#pragma warning disable CA1819
  /// <summary>Gets the Tapo device's current firmware ID.</summary>
  /// <remarks>The value will be a <see cref="string"/> representing a HEX-encoded(Base16) byte array.</remarks>
  [JsonPropertyName("fw_id")]
  [JsonConverter(typeof(TapoBase16ByteArrayJsonConverter))]
  public byte[]? FirmwareId { get; init; }
#pragma warning restore CA1819

  /// <summary>Gets the Tapo device's current firmware version string.</summary>
  /// <remarks>The value would be like <c>x.y.z Build yyyyMMdd Rel. XXXXX</c>, as an example.</remarks>
  [JsonPropertyName("fw_ver")]
  public string? FirmwareVersion { get; init; }

#pragma warning disable CA1819
  /// <summary>Gets the Tapo device's hardware ID.</summary>
  /// <remarks>The value will be a <see cref="string"/> representing a HEX-encoded(Base16) byte array.</remarks>
  [JsonPropertyName("hw_id")]
  [JsonConverter(typeof(TapoBase16ByteArrayJsonConverter))]
  public byte[]? HardwareId { get; init; }
#pragma warning restore CA1819

  /// <summary>Gets the Tapo device's hardware version string.</summary>
  /// <remarks>The value would be like <c>x.y.z</c>, as an example.</remarks>
  [JsonPropertyName("hw_ver")]
  public string? HardwareVersion { get; init; }

#pragma warning disable CA1819
  /// <summary>Gets the Tapo device's OEM ID.</summary>
  /// <remarks>The value will be a <see cref="string"/> representing a HEX-encoded(Base16) byte array.</remarks>
  [JsonPropertyName("oem_id")]
  [JsonConverter(typeof(TapoBase16ByteArrayJsonConverter))]
  public byte[]? OemId { get; init; }
#pragma warning restore CA1819

  /// <summary>Gets the <see cref="PhysicalAddress"/> that represents the Tapo device's MAC address.</summary>
  [JsonPropertyName("mac")]
  [JsonConverter(typeof(MacAddressJsonConverter))]
  public PhysicalAddress? MacAddress { get; init; }

  /// <summary>Gets the string that represents the Tapo device's hardware specifications.</summary>
  /// <remarks>The value would be like <c>JP</c> for L530 sold for Japan, as an example.</remarks>
  [JsonPropertyName("specs")]
  public string? HardwareSpecifications { get; init; }

  /// <summary>Gets the language currently set for the Tapo device.</summary>
  /// <remarks>The value would be like <c>ja_JP</c>, as an example.</remarks>
  [JsonPropertyName("lang")]
  public string? Language { get; init; }

  /// <summary>Gets a value indicating whether the Tapo device is on state currently or not.</summary>
  [JsonPropertyName("device_on")]
  public bool IsOn { get; init; }

  /// <summary>Gets the <see cref="TimeSpan"/> value that represents the Tapo device's current on-time duration.</summary>
  [JsonPropertyName("on_time")]
  [JsonConverter(typeof(TimeSpanInSecondsJsonConverter))]
  public TimeSpan? OnTimeDuration { get; init; }

  /// <summary>Gets a value indicating whether the Tapo device is currently in the overheated state or not.</summary>
  [JsonPropertyName("overheated")]
  public bool IsOverheated { get; init; }

  /// <summary>Gets the nickname currently set for the Tapo device.</summary>
  [JsonPropertyName("nickname")]
  [JsonConverter(typeof(TapoBase64StringJsonConverter))]
  public string? NickName { get; init; }

  /// <summary>Gets the avatar name currently set for the Tapo device.</summary>
  [JsonPropertyName("avatar")]
  public string? Avatar { get; init; }

  /// <summary>Gets the <see cref="TimeSpan"/> value that represents the time zone currently set for the Tapo device.</summary>
  /// <seealso cref="TimeZoneRegion"/>
  /// <seealso cref="HasGeolocationInfoSet"/>
  [JsonPropertyName("time_diff")]
  [JsonConverter(typeof(TimeSpanInMinutesJsonConverter))]
  public TimeSpan? TimeZoneOffset { get; init; }

  /// <summary>Gets the <see cref="string"/> value that represents the time zone region currently set for the Tapo device.</summary>
  /// <remarks>The value would be like <c>Asia/Tokyo</c>, as an example.</remarks>
  /// <seealso cref="TimeZoneOffset"/>
  /// <seealso cref="HasGeolocationInfoSet"/>
  [JsonPropertyName("region")]
  public string? TimeZoneRegion { get; init; }

  /// <summary>Gets the <see cref="decimal"/> value that represents the longitude currently set for the Tapo device, in the decimal degrees (DD) notation.</summary>
  /// <remarks>The value would be like <c>139.7666</c>, as an example.</remarks>
  /// <seealso cref="GeolocationLatitude"/>
  /// <seealso cref="HasGeolocationInfoSet"/>
  [JsonPropertyName("longitude")]
  [JsonConverter(typeof(GeolocationInDecimalDegreesJsonConverter))]
  public decimal? GeolocationLongitude { get; init; }

  /// <summary>Gets the <see cref="decimal"/> value that represents the latitude currently set for the Tapo device, in the decimal degrees (DD) notation.</summary>
  /// <remarks>The value would be like <c>35.6813</c>, as an example.</remarks>
  /// <seealso cref="GeolocationLongitude"/>
  /// <seealso cref="HasGeolocationInfoSet"/>
  [JsonPropertyName("latitude")]
  [JsonConverter(typeof(GeolocationInDecimalDegreesJsonConverter))]
  public decimal? GeolocationLatitude { get; init; }

  /// <summary>Gets a value indicating whether the geolocation information has been set for the Tapo device currently or not.</summary>
  /// <seealso cref="TimeZoneOffset"/>
  /// <seealso cref="TimeZoneRegion"/>
  /// <seealso cref="GeolocationLatitude"/>
  /// <seealso cref="GeolocationLongitude"/>
  [JsonPropertyName("has_set_location_info")]
  public bool HasGeolocationInfoSet { get; init; }

  /// <summary>Gets the <see cref="System.Net.IPAddress"/> that represents the IP address on the network to which the Tapo device is currently connected.</summary>
  [JsonPropertyName("ip")]
  [JsonConverter(typeof(TapoIPAddressJsonConverter))]
  public IPAddress? IPAddress { get; init; }

  /// <summary>Gets the <see cref="string"/> that represents the SSID of the network to which the Tapo device is currently connected.</summary>
  [JsonPropertyName("ssid")]
  [JsonConverter(typeof(TapoBase64StringJsonConverter))]
  public string? NetworkSsid { get; init; }

  /// <summary>Gets the <see cref="int"/> that represents the signal level of the network to which the Tapo device is currently connected.</summary>
  /// <remarks>The value would be 0🔇, 1🔈, 2🔉 or 3🔊.</remarks>
  [JsonPropertyName("signal_level")]
  public int? NetworkSignalLevel { get; init; }

  /// <summary>Gets the <see cref="decimal"/> that represents the RSSI (Received Signal Strength Indicator) of the network to which the Tapo device is currently connected, in dB.</summary>
  /// <remarks>The value would be like <c>-30</c>, as an example.</remarks>
  [JsonPropertyName("rssi")]
  public decimal? NetworkRssi { get; init; }
}
