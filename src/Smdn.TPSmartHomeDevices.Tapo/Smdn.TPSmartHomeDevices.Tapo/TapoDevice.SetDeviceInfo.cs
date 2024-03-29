// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices.Tapo;

#pragma warning disable IDE0040
partial class TapoDevice {
#pragma warning restore IDE0040
#pragma warning disable CA1822
  private readonly struct TurnOnParameter {
    [JsonPropertyName("device_on")]
    public bool DeviceOn => true;
  }

  private readonly struct TurnOffParameter {
    [JsonPropertyName("device_on")]
    public bool DeviceOn => false;
  }
#pragma warning restore CA1822

  /// <summary>
  /// Turns on the device.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  public ValueTask TurnOnAsync(CancellationToken cancellationToken = default)
    => SetDeviceInfoAsync(
      default(TurnOnParameter),
      cancellationToken
    );

  /// <summary>
  /// Turns off the device.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  public ValueTask TurnOffAsync(CancellationToken cancellationToken = default)
    => SetDeviceInfoAsync(
      default(TurnOffParameter),
      cancellationToken
    );

  /// <summary>
  /// Sets the on/off state of the device according to the parameter <paramref name="newOnOffState" />.
  /// </summary>
  /// <param name="newOnOffState">
  /// The value that indicates new on/off state to be set. <see langword="true"/> for on, otherwise off.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  public ValueTask SetOnOffStateAsync(
    bool newOnOffState,
    CancellationToken cancellationToken = default
  )
    => newOnOffState
      ? TurnOnAsync(cancellationToken)
      : TurnOffAsync(cancellationToken);
}
