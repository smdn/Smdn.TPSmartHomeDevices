// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net.NetworkInformation;

namespace Smdn.TPSmartHomeDevices;

/// <summary>
/// Represents smart device information.
/// </summary>
public interface IDeviceInfo {
  /// <summary>Gets the device's ID.</summary>
  ReadOnlySpan<byte> Id { get; }

  /// <summary>Gets the device's model name.</summary>
  string? ModelName { get; }

  /// <summary>Gets the string that represents device's firmware version.</summary>
  string? FirmwareVersion { get; }

  /// <summary>Gets the string that represents device's hardware version.</summary>
  string? HardwareVersion { get; }

  /// <summary>Gets the device's MAC address.</summary>
  PhysicalAddress? MacAddress { get; }
}
