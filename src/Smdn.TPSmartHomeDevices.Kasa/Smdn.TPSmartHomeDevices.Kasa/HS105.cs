// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Smdn.TPSmartHomeDevices.Kasa.Json;

namespace Smdn.TPSmartHomeDevices.Kasa;

/// <summary>
/// Provides APIs to operate Kasa HS105, Smart Wi-Fi Plug Mini.
/// </summary>
/// <remarks>
/// This is an unofficial API that has no affiliation with TP-Link.
/// This API is released under the <see href="https://opensource.org/license/mit/">MIT License</see>, and as stated in the terms of the MIT License,
/// there is no warranty for the results of using this API and no responsibility is taken for those results.
/// </remarks>
/// <seealso href="https://www.tp-link.com/jp/home-networking/smart-plug/hs105/">Kasa HS105 product information (ja)</seealso>
/// <seealso href="https://www.kasasmart.com/us/products/smart-plugs/kasa-smart-wifi-plug-mini">Kasa HS105 product information (en)</seealso>
public class HS105 : KasaDevice, ISmartDevice {
#pragma warning disable SA1114
  private static readonly JsonEncodedText MethodTextSetRelayState = JsonEncodedText.Encode(
#if LANG_VERSION_11_OR_GREATER
    "set_relay_state"u8
#else
    "set_relay_state"
#endif
  );
#pragma warning restore SA1114

  /// <summary>
  /// Initializes a new instance of the <see cref="HS105"/> class with specifying the device endpoint by host name.
  /// </summary>
  /// <inheritdoc cref="KasaDevice(string, IServiceProvider?)" />
  public HS105(
    string host,
    IServiceProvider? serviceProvider = null
  )
    : base(
      host: host,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="HS105"/> class with specifying the device endpoint by IP address.
  /// </summary>
  /// <inheritdoc cref="KasaDevice(IPAddress, IServiceProvider?)" />
  public HS105(
    IPAddress ipAddress,
    IServiceProvider? serviceProvider = null
  )
    : base(
      ipAddress: ipAddress,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="HS105"/> class with specifying the device endpoint by MAC address.
  /// </summary>
  /// <inheritdoc cref="KasaDevice(PhysicalAddress, IServiceProvider)" />
  public HS105(
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
  /// Initializes a new instance of the <see cref="HS105"/> class.
  /// </summary>
  /// <inheritdoc cref="KasaDevice(IDeviceEndPoint, IServiceProvider?)" />
  public HS105(
    IDeviceEndPoint deviceEndPoint,
    IServiceProvider? serviceProvider = null
  )
    : base(
      deviceEndPoint: deviceEndPoint,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="HS105"/> class.
  /// </summary>
  /// <inheritdoc cref="KasaDevice.Create{TAddress}(TAddress, IServiceProvider)" />
  public static new HS105 Create<TAddress>(
    TAddress deviceAddress,
    IServiceProvider serviceProvider
  ) where TAddress : notnull
    => new(
      deviceEndPoint: DeviceEndPoint.Create(
        address: deviceAddress,
        serviceProvider.GetDeviceEndPointFactory<TAddress>()
      ),
      serviceProvider: serviceProvider
    );

  private readonly struct SetRelayStateParameter {
    public static readonly SetRelayStateParameter SetOff = new(false);
    public static readonly SetRelayStateParameter SetOn = new(true);

    [JsonPropertyName("state")]
    [JsonConverter(typeof(KasaNumericalBooleanJsonConverter))]
    public bool State { get; }

    public SetRelayStateParameter(bool state)
    {
      State = state;
    }
  }

  /// <summary>
  /// Turns on the device.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  public ValueTask TurnOnAsync(CancellationToken cancellationToken = default)
    => SendRequestAsync(
      module: ModuleTextSystem,
      method: MethodTextSetRelayState,
      parameters: SetRelayStateParameter.SetOn,
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Turns off the device.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  public ValueTask TurnOffAsync(CancellationToken cancellationToken = default)
    => SendRequestAsync(
      module: ModuleTextSystem,
      method: MethodTextSetRelayState,
      parameters: SetRelayStateParameter.SetOff,
      cancellationToken: cancellationToken
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
    => SendRequestAsync(
      module: ModuleTextSystem,
      method: MethodTextSetRelayState,
      parameters: newOnOffState ? SetRelayStateParameter.SetOn : SetRelayStateParameter.SetOff,
      cancellationToken: cancellationToken
    );

  private readonly struct GetSysInfoRelayStateParameter {
    [JsonPropertyName("relay_state")]
    [JsonConverter(typeof(KasaNumericalBooleanJsonConverter))]
    public bool RelayState { get; init; }
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
    => SendRequestAsync(
      module: ModuleTextSystem,
      method: MethodTextGetSysInfo,
      composeResult: static result => JsonSerializer.Deserialize<GetSysInfoRelayStateParameter>(result).RelayState,
      cancellationToken: cancellationToken
    );
}
