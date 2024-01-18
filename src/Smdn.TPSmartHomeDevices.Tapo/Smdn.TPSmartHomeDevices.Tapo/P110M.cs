// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;
using Smdn.TPSmartHomeDevices.Tapo.Json;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

/// <summary>
/// Provides APIs to operate Tapo P110M, Mini Smart Wi-Fi Plug, Energy Monitoring.
/// </summary>
/// <remarks>
/// This is an unofficial API that has no affiliation with TP-Link.
/// This API is released under the <see href="https://opensource.org/license/mit/">MIT License</see>, and as stated in the terms of the MIT License,
/// there is no warranty for the results of using this API and no responsibility is taken for those results.
/// </remarks>
/// <seealso href="https://www.tp-link.com/jp/smart-home/tapo/tapo-p110m/">Tapo P110M product information (ja)</seealso>
public class P110M : TapoDevice {
  /// <summary>
  /// Initializes a new instance of the <see cref="P110M"/> class with specifying the device endpoint by host name.
  /// </summary>
  /// <inheritdoc cref="TapoDevice(string, string, string, IServiceProvider?)" />
  public P110M(
    string host,
    string email,
    string password,
    IServiceProvider? serviceProvider = null
  )
    : base(
      host: host,
      email: email,
      password: password,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="P110M"/> class with specifying the device endpoint by host name.
  /// </summary>
  /// <inheritdoc cref="TapoDevice(string, IServiceProvider)" />
  public P110M(
    string host,
    IServiceProvider serviceProvider
  )
    : base(
      host: host,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="P110M"/> class with specifying the device endpoint by IP address.
  /// </summary>
  /// <inheritdoc cref="TapoDevice(string, string, string, IServiceProvider?)" />
  public P110M(
    IPAddress ipAddress,
    string email,
    string password,
    IServiceProvider? serviceProvider = null
  )
    : base(
      ipAddress: ipAddress,
      email: email,
      password: password,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="P110M"/> class with specifying the device endpoint by IP address.
  /// </summary>
  /// <inheritdoc cref="TapoDevice(IPAddress, IServiceProvider)" />
  public P110M(
    IPAddress ipAddress,
    IServiceProvider serviceProvider
  )
    : base(
      ipAddress: ipAddress,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="P110M"/> class with specifying the device endpoint by MAC address.
  /// </summary>
  /// <inheritdoc cref="TapoDevice(PhysicalAddress, string, string, IServiceProvider?)" />
  public P110M(
    PhysicalAddress macAddress,
    string email,
    string password,
    IServiceProvider serviceProvider
  )
    : base(
      macAddress: macAddress,
      email: email,
      password: password,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="P110M"/> class with specifying the device endpoint by MAC address.
  /// </summary>
  /// <inheritdoc cref="TapoDevice(PhysicalAddress, IServiceProvider)" />
  public P110M(
    PhysicalAddress macAddress,
    IServiceProvider serviceProvider
  )
    : base(
      macAddress: macAddress,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="P110M"/> class.
  /// </summary>
  /// <inheritdoc
  ///   cref="TapoDevice(IDeviceEndPoint, ITapoCredentialProvider?, TapoDeviceExceptionHandler?, IServiceProvider?)"
  ///   path="/exception | /param[@name='deviceEndPoint' or @name='credential' or @name='serviceProvider']"
  /// />
  public P110M(
    IDeviceEndPoint deviceEndPoint,
    ITapoCredentialProvider? credential = null,
    IServiceProvider? serviceProvider = null
  )
    : base(
      deviceEndPoint: deviceEndPoint,
      credential: credential,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <inheritdoc cref="TapoDevice.Create{TAddress}(TAddress, IServiceProvider, ITapoCredentialProvider?)" />
  public static new P110M Create<TAddress>(
    TAddress deviceAddress,
    IServiceProvider serviceProvider,
    ITapoCredentialProvider? credential = null
  ) where TAddress : notnull
    => new(
      deviceEndPoint: DeviceEndPoint.Create(
        address: deviceAddress,
        serviceProvider.GetDeviceEndPointFactory<TAddress>()
      ),
      credential: credential,
      serviceProvider: serviceProvider
    );

  private readonly struct GetCurrentPowerResult {
    [JsonPropertyName("current_power")]
    [JsonConverter(typeof(TapoElectricPowerInWattJsonConverter))]
    public decimal? CurrentPower { get; init; }
  }

  /// <summary>
  /// Gets the current power consumption for the device connected to <see cref="P110M"/>.
  /// </summary>
  /// <returns>
  /// A <see cref="ValueTask{TResult}"/> representing the result of method.
  /// If the power consumption is successfully retrieved, the <see cref="decimal"/> value that represents the power consumption in unit of watt [W], is set.
  /// If the device does not support retrieving the power consumption, the value of <see cref="ValueTask{TResult}"/> will be <see langword="null"/>.
  /// </returns>
  public virtual ValueTask<decimal?> GetCurrentPowerConsumptionAsync(
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync<
      GetCurrentPowerRequest,
      PassThroughResponse<GetCurrentPowerResult>,
      decimal?
    >(
      request: default,
      composeResult: static result => result.Result.CurrentPower,
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Gets the monitoring data report for the device connected to <see cref="P110M"/>.
  /// </summary>
  /// <returns>
  /// A <see cref="ValueTask{TResult}"/> representing the result of method.
  /// </returns>
  public virtual ValueTask<TapoPlugMonitoringData> GetMonitoringDataAsync(
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync<
      GetEnergyUsageRequest,
      PassThroughResponse<TapoPlugMonitoringData>,
      TapoPlugMonitoringData
    >(
      request: default,
      composeResult: static result => result.Result,
      cancellationToken: cancellationToken
    );
}
