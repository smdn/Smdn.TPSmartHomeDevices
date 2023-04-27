// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices.Tapo;

#pragma warning disable IDE0040
partial class TapoDevice {
#pragma warning restore IDE0040
  /// <summary>
  /// Gets the Tapo device information including current device status.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{TapoDeviceInfo}"/> representing the result of method.
  /// </returns>
  /// <seealso cref="TapoDeviceInfo"/>
  public ValueTask<TapoDeviceInfo> GetDeviceInfoAsync(
    CancellationToken cancellationToken = default
  )
    => GetDeviceInfoAsync<TapoDeviceInfo>(
      cancellationToken: cancellationToken
    );

  private readonly struct GetDeviceInfoOnOffStateResult {
    [JsonPropertyName("device_on")]
    public bool DeviceOn { get; init; }
  }

  /// <summary>
  /// Gets the on/off state of the device.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  public ValueTask<bool> GetOnOffStateAsync(
    CancellationToken cancellationToken = default
  )
    => GetDeviceInfoAsync<GetDeviceInfoOnOffStateResult, bool>(
      composeResult: static resp => resp.DeviceOn,
      cancellationToken: cancellationToken
    );
}
