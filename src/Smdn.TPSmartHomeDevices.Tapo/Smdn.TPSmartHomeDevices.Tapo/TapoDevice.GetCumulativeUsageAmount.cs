// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

#pragma warning disable IDE0040
partial class TapoDevice {
#pragma warning restore IDE0040
  private readonly struct GetDeviceUsageResult {
    [JsonPropertyName("time_usage")]
    public TapoDeviceCumulativeTimeUsage? TimeUsage { get; init; }
    [JsonPropertyName("power_usage")]
    public TapoDeviceCumulativeEnergyUsage? EnergyUsage { get; init; }
  }

  /// <summary>
  /// Gets the usage amount of cumulative time and electric energy of Tapo device.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{TResult}"/> representing the result of method.
  /// If the device does not support retrieving the usage amount of cumulative time, <see cref="TapoDeviceCumulativeTimeUsage"/> in the return value will be <see langword="null"/>.
  /// If the device does not support retrieving the usage amount of cumulative electric energy, <see cref="TapoDeviceCumulativeEnergyUsage"/> in the return value will be <see langword="null"/>.
  /// </returns>
  /// <seealso cref="GetCumulativeTimeUsageAsync"/>
  /// <seealso cref="GetCumulativeEnergyUsageAsync"/>
  public virtual ValueTask<(TapoDeviceCumulativeTimeUsage? TimeUsage, TapoDeviceCumulativeEnergyUsage? EnergyUsage)> GetCumulativeUsageAsync(
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync<
      GetDeviceUsageRequest,
      GetDeviceUsageResponse<GetDeviceUsageResult>,
      (TapoDeviceCumulativeTimeUsage?, TapoDeviceCumulativeEnergyUsage?)
    >(
      request: default,
      composeResult: static result => (
        result.Result.TimeUsage,
        result.Result.EnergyUsage
      ),
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Gets the usage amount of cumulative time of Tapo device.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{TResult}"/> representing the result of method.
  /// If the device does not support retrieving the usage amount of cumulative time, <see cref="TapoDeviceCumulativeTimeUsage"/> in the return value will be <see langword="null"/>.
  /// </returns>
  public virtual ValueTask<TapoDeviceCumulativeTimeUsage?> GetCumulativeTimeUsageAsync(
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync<
      GetDeviceUsageRequest,
      GetDeviceUsageResponse<GetDeviceUsageResult>,
      TapoDeviceCumulativeTimeUsage?
    >(
      request: default,
      composeResult: static result => result.Result.TimeUsage,
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Gets the usage amount of cumulative electric energy of Tapo device.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{TResult}"/> representing the result of method.
  /// If the device does not support retrieving the usage amount of cumulative electric energy, <see cref="TapoDeviceCumulativeEnergyUsage"/> in the return value will be <see langword="null"/>.
  /// </returns>
  public virtual ValueTask<TapoDeviceCumulativeEnergyUsage?> GetCumulativeEnergyUsageAsync(
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync<
      GetDeviceUsageRequest,
      GetDeviceUsageResponse<GetDeviceUsageResult>,
      TapoDeviceCumulativeEnergyUsage?
    >(
      request: default,
      composeResult: static result => result.Result.EnergyUsage,
      cancellationToken: cancellationToken
    );
}
