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

public class HS105 : KasaDevice {
  private static readonly JsonEncodedText MethodTextSetRelayState = JsonEncodedText.Encode(
#if LANG_VERSION_11_OR_GREATER
    "set_relay_state"u8
#else
    "set_relay_state"
#endif
  );

  public HS105(
    string hostName,
    IServiceProvider? serviceProvider = null
  )
    : base(
      hostName: hostName,
      serviceProvider: serviceProvider
    )
  {
  }

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

  public HS105(
    PhysicalAddress macAddress,
    IDeviceEndPointFactory<PhysicalAddress> endPointFactory,
    IServiceProvider? serviceProvider = null
  )
    : base(
      macAddress: macAddress,
      endPointFactory: endPointFactory,
      serviceProvider: serviceProvider
    )
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="HS105"/> class with a MAC address.
  /// </summary>
  /// <inheritdoc cref="KasaDevice" />
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

  public HS105(
    IDeviceEndPointProvider deviceEndPointProvider,
    IServiceProvider? serviceProvider = null
  )
    : base(
      deviceEndPointProvider: deviceEndPointProvider,
      serviceProvider: serviceProvider
    )
  {
  }

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

  public Task TurnOnAsync(CancellationToken cancellationToken = default)
    => SendRequestAsync(
      module: ModuleTextSystem,
      method: MethodTextSetRelayState,
      parameters: SetRelayStateParameter.SetOn,
      cancellationToken: cancellationToken
    );

  public Task TurnOffAsync(CancellationToken cancellationToken = default)
    => SendRequestAsync(
      module: ModuleTextSystem,
      method: MethodTextSetRelayState,
      parameters: SetRelayStateParameter.SetOff,
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Sets the on/off state of the device according to the parameter <see cref="newOnOffState" />.
  /// </summary>
  /// <param name="newOnOffState">
  /// The value that indicates new on/off state to be set. <see langword="true"/> for on, otherwise off.
  /// </param>
  public Task SetOnOffStateAsync(
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

  public Task<bool> GetOnOffStateAsync(
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync(
      module: ModuleTextSystem,
      method: MethodTextGetSysInfo,
      composeResult: static result => JsonSerializer.Deserialize<GetSysInfoRelayStateParameter>(result).RelayState,
      cancellationToken: cancellationToken
    );
}
