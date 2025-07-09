// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1815 // TODO: implement equality comparison

using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// The type that reflects <c>set_device_info</c> JSON request.
/// </summary>
/// <typeparam name="TParameters">A type that will be serialized to the value of the <c>params</c> JSON property.</typeparam>
public readonly struct SetDeviceInfoRequest<TParameters> : ITapoPassThroughRequest {
  [JsonPropertyName("method")]
  [JsonPropertyOrder(0)]
  public string Method => "set_device_info";

#pragma warning disable CA1822
  [JsonPropertyName("requestTimeMils")]
  public long RequestTimeMilliseconds => 0L; // DateTimeOffset.Now.ToUnixTimeMilliseconds();
#pragma warning restore CA1822

  [JsonPropertyName("terminalUUID")]
  public string TerminalUuid { get; }

  [JsonPropertyName("params")]
  public TParameters Parameters { get; }

  public SetDeviceInfoRequest(
    string terminalUuid,
    TParameters parameters
  )
  {
    TerminalUuid = terminalUuid;
    Parameters = parameters;
  }
}
