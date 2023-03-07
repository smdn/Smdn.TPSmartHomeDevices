// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

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
