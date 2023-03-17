// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Smdn.TPSmartHomeDevices.Kasa.Json;

namespace Smdn.TPSmartHomeDevices.Kasa;

public class KL130 : KasaDevice {
  private static readonly JsonEncodedText ModuleTextLightingService = JsonEncodedText.Encode(
#if LANG_VERSION_11_OR_GREATER
    "smartlife.iot.smartbulb.lightingservice"u8
#else
    "smartlife.iot.smartbulb.lightingservice"
#endif
  );
  private static readonly JsonEncodedText MethodTextTransitionLightState = JsonEncodedText.Encode(
#if LANG_VERSION_11_OR_GREATER
    "transition_light_state"u8
#else
    "transition_light_state"
#endif
  );

  public KL130(
    string hostName,
    IServiceProvider? serviceProvider = null
  )
    : base(
      hostName: hostName ?? throw new ArgumentNullException(nameof(hostName)),
      serviceProvider: serviceProvider
    )
  {
  }

  public KL130(
    IPAddress ipAddress,
    IServiceProvider? serviceProvider = null
  )
    : base(
      ipAddress: ipAddress,
      serviceProvider: serviceProvider
    )
  {
  }

  public KL130(
    IDeviceEndPointProvider deviceEndPointProvider,
    IServiceProvider? serviceProvider = null
  )
    : base(
      deviceEndPointProvider: deviceEndPointProvider,
      serviceProvider: serviceProvider
    )
  {
  }

  private static TimeSpan ValidateTransitionPeriod(TimeSpan? value, string paramName)
  {
    if (value == null)
      return TimeSpan.Zero;

    if (value.Value < TimeSpan.Zero)
      throw new ArgumentOutOfRangeException(paramName: paramName, actualValue: value, message: "The value for transition period must be zero or positive value.");

    return value.Value;
  }

  private readonly record struct SetOnOffStateParameter(
    [property: JsonPropertyName("on_off")]
    [property: JsonConverter(typeof(KasaNumericalBooleanJsonConverter))]
    bool OnOff,
    [property: JsonPropertyName("transition_period")]
    int TransitionPeriodInMilliseconds
  ) {
    [JsonPropertyName("ignore_default")]
    public int IgnoreDefault => 0; // turn on/off with the default configuration
  }

  public Task TurnOnAsync(
    TimeSpan? transitionPeriod = null,
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync(
      module: ModuleTextLightingService,
      method: MethodTextTransitionLightState,
      parameters: new SetOnOffStateParameter(
        OnOff: true, // on
        TransitionPeriodInMilliseconds: (int)ValidateTransitionPeriod(transitionPeriod, nameof(transitionPeriod)).TotalMilliseconds
      ),
      cancellationToken: cancellationToken
    );

  public Task TurnOffAsync(
    TimeSpan? transitionPeriod = null,
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync(
      module: ModuleTextLightingService,
      method: MethodTextTransitionLightState,
      parameters: new SetOnOffStateParameter(
        OnOff: false, // off
        TransitionPeriodInMilliseconds: (int)ValidateTransitionPeriod(transitionPeriod, nameof(transitionPeriod)).TotalMilliseconds
      ),
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
    TimeSpan? transitionPeriod = null,
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync(
      module: ModuleTextLightingService,
      method: MethodTextTransitionLightState,
      parameters: new SetOnOffStateParameter(
        OnOff: newOnOffState,
        TransitionPeriodInMilliseconds: (int)ValidateTransitionPeriod(transitionPeriod, nameof(transitionPeriod)).TotalMilliseconds
      ),
      cancellationToken: cancellationToken
    );

  private readonly struct GetSysInfoLightStateParameter {
    [JsonPropertyName("light_state")]
    public KL130LightState LightState { get; init; }
  }

  public Task<KL130LightState> GetLightStateAsync(
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync(
      module: ModuleTextSystem,
      method: MethodTextGetSysInfo,
      composeResult: static result => JsonSerializer.Deserialize<GetSysInfoLightStateParameter>(result).LightState,
      cancellationToken: cancellationToken
    );

  public Task<bool> GetOnOffStateAsync(
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync(
      module: ModuleTextSystem,
      method: MethodTextGetSysInfo,
      composeResult: static result => JsonSerializer.Deserialize<GetSysInfoLightStateParameter>(result).LightState.IsOn,
      cancellationToken: cancellationToken
    );

  private readonly record struct SetColorTemperatureParameter(
    [property: JsonPropertyName("color_temp")]
    int ColorTemperature,
    [property: JsonPropertyName("brightness")]
    int? Brightness,
    [property: JsonPropertyName("transition_period")]
    int TransitionPeriodInMilliseconds
  ) {
    [JsonPropertyName("on_off")]
    [JsonConverter(typeof(KasaNumericalBooleanJsonConverter))]
    public bool OnOff => true; // on

    [JsonPropertyName("ignore_default")]
    public int IgnoreDefault => 1; // ignore the default configuration
  }

  /// <param name="colorTemperature">
  /// The color temperature in kelvin, in range of 2500~9000[K].
  /// </param>
  /// <param name="brightness">
  /// The brightness in percent value, in range of 1~100[%]. If <see langword="null"/>, the current brightness will be kept.
  /// </param>
  public Task SetColorTemperatureAsync(
    int colorTemperature,
    int? brightness = null,
    TimeSpan? transitionPeriod = null,
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync(
      module: ModuleTextLightingService,
      method: MethodTextTransitionLightState,
      new SetColorTemperatureParameter(
        ColorTemperature: colorTemperature, // TODO: validation
        Brightness: brightness, // TODO: validation
        TransitionPeriodInMilliseconds: (int)ValidateTransitionPeriod(transitionPeriod, nameof(transitionPeriod)).TotalMilliseconds
      ),
      cancellationToken
    );

  private readonly record struct SetColorParameter(
    [property: JsonPropertyName("hue")]
    int? Hue,
    [property: JsonPropertyName("saturation")]
    int? Saturation,
    [property: JsonPropertyName("brightness")]
    int? Brightness,
    [property: JsonPropertyName("transition_period")]
    int TransitionPeriodInMilliseconds
  ) {
    [JsonPropertyName("color_temp")]
    public int ColorTemperature => 0; // set color with hue and saturation

    [JsonPropertyName("on_off")]
    [JsonConverter(typeof(KasaNumericalBooleanJsonConverter))]
    public bool OnOff => true; // on

    [JsonPropertyName("ignore_default")]
    public int IgnoreDefault => 1; // ignore the default configuration
  }

  /// <param name="hue">
  /// The hue of the color in degree, in range of 0~360[°].
  /// </param>
  /// <param name="saturation">
  /// The saturation of the color in percent value, in range of 0~100[%].
  /// </param>
  /// <param name="brightness">
  /// The brightness in percent value, in range of 1~100[%]. If <see langword="null"/>, the current brightness will be kept.
  /// </param>
  public Task SetColorAsync(
    int hue,
    int saturation,
    int? brightness = null,
    TimeSpan? transitionPeriod = null,
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync(
      module: ModuleTextLightingService,
      method: MethodTextTransitionLightState,
      new SetColorParameter(
        Hue: hue, // TODO: validation
        Saturation: saturation, // TODO: validation
        Brightness: brightness, // TODO: validation
        TransitionPeriodInMilliseconds: (int)ValidateTransitionPeriod(transitionPeriod, nameof(transitionPeriod)).TotalMilliseconds
      ),
      cancellationToken
    );
}
