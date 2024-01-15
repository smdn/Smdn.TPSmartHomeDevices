// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#if NET8_0_OR_GREATER
// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/immutability#non-public-members-and-property-accessors
#define SYSTEM_JSON_TEXT_SERIALIZE_NON_PUBLIC_ACCESSORS
#endif

using System;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;

using Smdn.TPSmartHomeDevices.Json;

namespace Smdn.TPSmartHomeDevices.Kasa;

/// <summary>
/// Represents Kasa device information, including current status.
/// </summary>
/// <seealso cref="KasaDevice.GetDeviceInfoAsync(System.Threading.CancellationToken)"/>
public class KasaDeviceInfo : IDeviceInfo {
  /// <summary>
  /// Gets the <see cref="DateTimeOffset"/> that represents the time at which this instance was acquired.
  /// </summary>
  [JsonIgnore]
  public DateTimeOffset TimeStamp { get; } = DateTimeOffset.Now;

  /*
   * properties for the informations common to the devices
   */
#pragma warning disable CA1819
  /// <summary>Gets the Kasa device's ID.</summary>
  /// <remarks>The value will be a <see cref="string"/> representing a HEX-encoded(Base16) byte array.</remarks>
  [JsonPropertyName("deviceId")]
  [JsonConverter(typeof(Base16ByteArrayJsonConverter))]
  public byte[]? Id { get; init; }
#pragma warning restore CA1819

  ReadOnlySpan<byte> IDeviceInfo.Id => Id is null ? ReadOnlySpan<byte>.Empty : Id.AsSpan();

  /// <summary>Gets the Kasa device's description.</summary>
  public string? Description => JsonPropertyDescription ?? JsonPropertyDevName;

  [JsonPropertyName("description")]
#if SYSTEM_JSON_TEXT_SERIALIZE_NON_PUBLIC_ACCESSORS
  [JsonInclude]
  private
#else
  public
#endif
  string? JsonPropertyDescription { get; set; }

  [JsonPropertyName("dev_name")]
#if SYSTEM_JSON_TEXT_SERIALIZE_NON_PUBLIC_ACCESSORS
  [JsonInclude]
  private
#else
  public
#endif
  string? JsonPropertyDevName { get; set; }

  /// <summary>Gets the Kasa device's type name.</summary>
  /// <remarks>The value would be like <c>IOT.SMARTBULB</c> for Kasa KL130, as an example.</remarks>
  public string? TypeName => JsonPropertyMicType ?? JsonPropertyType;

  [JsonPropertyName("mic_type")]
#if SYSTEM_JSON_TEXT_SERIALIZE_NON_PUBLIC_ACCESSORS
  [JsonInclude]
  private
#else
  public
#endif
  string? JsonPropertyMicType { get; set; }

  [JsonPropertyName("type")]
#if SYSTEM_JSON_TEXT_SERIALIZE_NON_PUBLIC_ACCESSORS
  [JsonInclude]
  private
#else
  public
#endif
  string? JsonPropertyType { get; set; }

  /// <summary>Gets the Kasa device's model name.</summary>
  /// <remarks>The value would be like <c>KL130(JP)</c> for Kasa KL130, as an example.</remarks>
  [JsonPropertyName("model")]
  public string? ModelName { get; init; }

#pragma warning disable CA1819
  /// <summary>Gets the Kasa device's current firmware ID.</summary>
  /// <remarks>The value will be a <see cref="string"/> representing a HEX-encoded(Base16) byte array.</remarks>
  [JsonPropertyName("fwId")]
  [JsonConverter(typeof(Base16ByteArrayJsonConverter))]
  public byte[]? FirmwareId { get; init; }
#pragma warning restore CA1819

  /// <summary>Gets the Kasa device's current firmware version string.</summary>
  /// <remarks>The value would be like <c>x.y.z Build yyyyMMdd Rel. XXXXX</c>, as an example.</remarks>
  [JsonPropertyName("sw_ver")]
  public string? FirmwareVersion { get; init; }

#pragma warning disable CA1819
  /// <summary>Gets the Kasa device's hardware ID.</summary>
  /// <remarks>The value will be a <see cref="string"/> representing a HEX-encoded(Base16) byte array.</remarks>
  [JsonPropertyName("hwId")]
  [JsonConverter(typeof(Base16ByteArrayJsonConverter))]
  public byte[]? HardwareId { get; init; }
#pragma warning restore CA1819

  /// <summary>Gets the Kasa device's hardware version string.</summary>
  /// <remarks>The value would be like <c>x.y</c>, as an example.</remarks>
  [JsonPropertyName("hw_ver")]
  public string? HardwareVersion { get; init; }

#pragma warning disable CA1819
  /// <summary>Gets the Kasa device's OEM ID.</summary>
  /// <remarks>The value will be a <see cref="string"/> representing a HEX-encoded(Base16) byte array.</remarks>
  [JsonPropertyName("oemId")]
  [JsonConverter(typeof(Base16ByteArrayJsonConverter))]
  public byte[]? OemId { get; init; }
#pragma warning restore CA1819

  /// <summary>Gets the <see cref="PhysicalAddress"/> that represents the Kasa device's MAC address.</summary>
  public PhysicalAddress? MacAddress => JsonPropertyMicMac ?? JsonPropertyMac;

  [JsonPropertyName("mic_mac")]
  [JsonConverter(typeof(MacAddressJsonConverter))]
#if SYSTEM_JSON_TEXT_SERIALIZE_NON_PUBLIC_ACCESSORS
  [JsonInclude]
  private
#else
  public
#endif
  PhysicalAddress? JsonPropertyMicMac { get; init; }

  [JsonPropertyName("mac")]
  [JsonConverter(typeof(MacAddressJsonConverter))]
#if SYSTEM_JSON_TEXT_SERIALIZE_NON_PUBLIC_ACCESSORS
  [JsonInclude]
  private
#else
  public
#endif
  PhysicalAddress? JsonPropertyMac { get; init; }

  /// <summary>Gets the <see cref="decimal"/> that represents the RSSI (Received Signal Strength Indicator) of the network to which the Kasa device is currently connected, in dB.</summary>
  /// <remarks>The value would be like <c>-30</c>, as an example.</remarks>
  [JsonPropertyName("rssi")]
  public decimal? NetworkRssi { get; init; }
}
