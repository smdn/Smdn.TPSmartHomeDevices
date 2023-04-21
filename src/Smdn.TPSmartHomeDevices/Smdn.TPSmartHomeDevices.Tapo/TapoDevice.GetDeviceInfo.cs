// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices.Tapo;

#pragma warning disable IDE0040
partial class TapoDevice {
#pragma warning restore IDE0040
  private readonly struct GetDeviceInfoOnOffStateResult {
    [JsonPropertyName("device_on")]
    public bool DeviceOn { get; init; }
  }

  public ValueTask<bool> GetOnOffStateAsync(
    CancellationToken cancellationToken = default
  )
    => GetDeviceInfoAsync<GetDeviceInfoOnOffStateResult, bool>(
      composeResult: static resp => resp.DeviceOn,
      cancellationToken: cancellationToken
    );
}
