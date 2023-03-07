// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;
using Smdn.TPSmartHomeDevices.Json;
using Smdn.TPSmartHomeDevices.Tapo.Json;

namespace Smdn.TPSmartHomeDevices.Tapo;

public class TapoDeviceInfo {
  [JsonIgnore]
  public DateTimeOffset TimeStamp { get; } = DateTimeOffset.Now;

  /*
   * storage for the device-specific informations
   */
  [JsonExtensionData]
  internal IDictionary<string, object>? ExtraData { get; init; }

  /*
   * properties for the informations common to the devices
   */
  [JsonPropertyName("device_id")]
  public string? Id { get; init; }

  [JsonPropertyName("type")]
  public string? TypeName { get; init; }

  [JsonPropertyName("model")]
  public string? ModelName { get; init; }

  [JsonPropertyName("fw_id")]
  public string? FirmwareId { get; init; }

  [JsonPropertyName("fw_ver")]
  public string? FirmwareVersion { get; init; }

  [JsonPropertyName("hw_id")]
  public string? HardwareId { get; init; }

  [JsonPropertyName("hw_ver")]
  public string? HardwareVersion { get; init; }

  [JsonPropertyName("oem_id")]
  public string? OemId { get; init; }

  [JsonPropertyName("mac")]
  [JsonConverter(typeof(MacAddressJsonConverter))]
  public PhysicalAddress? MacAddress { get; init; }

  [JsonPropertyName("specs")]
  public string? HardwareSpecifications { get; init; }

  [JsonPropertyName("lang")]
  public string? Language { get; init; }

  [JsonPropertyName("device_on")]
  public bool IsOn { get; init; }

  [JsonPropertyName("on_time")]
  [JsonConverter(typeof(TimeSpanInSecondsJsonConverter))]
  public TimeSpan? OnTimeDuration { get; init; }

  [JsonPropertyName("overheated")]
  public bool IsOverheated { get; init; }

  [JsonPropertyName("nickname")]
  [JsonConverter(typeof(TapoBase64StringJsonConverter))]
  public string? NickName { get; init; }

  [JsonPropertyName("avatar")]
  public string? Avatar { get; init; }

  [JsonPropertyName("time_diff")]
  [JsonConverter(typeof(TimeSpanInMinutesJsonConverter))]
  public TimeSpan? TimeZoneOffset { get; init; }

  [JsonPropertyName("region")]
  public string? TimeZoneRegion { get; init; }

  /// <summary>Gets a value that indicates a longitude in decimal degrees.</summary>
  [JsonPropertyName("longitude")]
  [JsonConverter(typeof(GeolocationInDecimalDegreesJsonConverter))]
  public decimal? GeolocationLongitude { get; init; }

  /// <summary>Gets a value that indicates a latitude in decimal degrees.</summary>
  [JsonPropertyName("latitude")]
  [JsonConverter(typeof(GeolocationInDecimalDegreesJsonConverter))]
  public decimal? GeolocationLatitude { get; init; }

  [JsonPropertyName("has_set_location_info")]
  public bool HasGeolocationInfoSet { get; init; }

  [JsonPropertyName("ip")]
  [JsonConverter(typeof(TapoIPAddressJsonConverter))]
  public IPAddress? IPAddress { get; init; }

  [JsonPropertyName("ssid")]
  [JsonConverter(typeof(TapoBase64StringJsonConverter))]
  public string? NetworkSsid { get; init; }

  [JsonPropertyName("signal_level")]
  public int? NetworkSignalLevel { get; init; }

  [JsonPropertyName("rssi")]
  public decimal? NetworkRssi { get; init; }
}
