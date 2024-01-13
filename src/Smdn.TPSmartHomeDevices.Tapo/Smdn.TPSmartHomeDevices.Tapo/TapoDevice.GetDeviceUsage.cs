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
    public TapoDeviceOperatingTime? TimeUsage { get; init; }
    [JsonPropertyName("power_usage")]
    public TapoDeviceEnergyUsage? EnergyUsage { get; init; }
  }

  /// <summary>
  /// Gets the total operating time and usage amount of cumulative electric energy of Tapo device.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{TResult}"/> representing the result of method.
  /// If the device does not support retrieving the total operating time, <see cref="TapoDeviceOperatingTime"/> in the return value will be <see langword="null"/>.
  /// If the device does not support retrieving the usage amount of cumulative electric energy, <see cref="TapoDeviceEnergyUsage"/> in the return value will be <see langword="null"/>.
  /// </returns>
  /// <seealso cref="GetTotalOperatingTimeAsync"/>
  /// <seealso cref="GetCumulativeEnergyUsageAsync"/>
  public virtual
  ValueTask<(TapoDeviceOperatingTime? TotalOperatingTime, TapoDeviceEnergyUsage? CumulativeEnergyUsage)>
  GetDeviceUsageAsync(
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync<
      GetDeviceUsageRequest,
      GetDeviceUsageResponse<GetDeviceUsageResult>,
      (TapoDeviceOperatingTime?, TapoDeviceEnergyUsage?)
    >(
      request: default,
      composeResult: static result => (
        result.Result.TimeUsage,
        result.Result.EnergyUsage
      ),
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Gets the total operating time of Tapo device.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{TResult}"/> representing the result of method.
  /// If the device does not support retrieving the total operating time, <see cref="TapoDeviceOperatingTime"/> in the return value will be <see langword="null"/>.
  /// </returns>
  /// <seealso cref="GetDeviceUsageAsync(CancellationToken)"/>
  public virtual ValueTask<TapoDeviceOperatingTime?> GetTotalOperatingTimeAsync(
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync<
      GetDeviceUsageRequest,
      GetDeviceUsageResponse<GetDeviceUsageResult>,
      TapoDeviceOperatingTime?
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
  /// If the device does not support retrieving the usage amount of cumulative electric energy, <see cref="TapoDeviceEnergyUsage"/> in the return value will be <see langword="null"/>.
  /// </returns>
  /// <seealso cref="GetDeviceUsageAsync(CancellationToken)"/>
  public virtual ValueTask<TapoDeviceEnergyUsage?> GetCumulativeEnergyUsageAsync(
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync<
      GetDeviceUsageRequest,
      GetDeviceUsageResponse<GetDeviceUsageResult>,
      TapoDeviceEnergyUsage?
    >(
      request: default,
      composeResult: static result => result.Result.EnergyUsage,
      cancellationToken: cancellationToken
    );
}
