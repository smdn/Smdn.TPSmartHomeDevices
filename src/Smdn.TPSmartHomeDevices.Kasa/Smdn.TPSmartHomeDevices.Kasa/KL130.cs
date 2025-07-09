// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
// cSpell:ignore smartlife,smartbulb,lightingservice
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
/// Provides APIs to operate Kasa KL130, Smart Light Bulb, Multicolor.
/// </summary>
/// <remarks>
/// This is an unofficial API that has no affiliation with TP-Link.
/// This API is released under the <see href="https://opensource.org/license/mit/">MIT License</see>, and as stated in the terms of the MIT License,
/// there is no warranty for the results of using this API and no responsibility is taken for those results.
/// </remarks>
/// <seealso href="https://www.tp-link.com/jp/home-networking/smart-bulb/kl130/">Kasa KL130 product information (ja)</seealso>
/// <seealso href="https://www.kasasmart.com/us/products/smart-lighting/kasa-smart-light-bulb-multicolor-kl130">Kasa KL130 product information (en)</seealso>
public class KL130 : KasaDevice, IMulticolorSmartLight {
#pragma warning disable SA1114
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
#pragma warning restore SA1114

  /// <summary>
  /// Initializes a new instance of the <see cref="KL130"/> class with specifying the device endpoint by host name.
  /// </summary>
  /// <inheritdoc cref="KasaDevice(string, IServiceProvider?)" />
  public KL130(
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
  /// Initializes a new instance of the <see cref="KL130"/> class with specifying the device endpoint by IP address.
  /// </summary>
  /// <inheritdoc cref="KasaDevice(IPAddress, IServiceProvider?)" />
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

  /// <summary>
  /// Initializes a new instance of the <see cref="KL130"/> class with specifying the device endpoint by MAC address.
  /// </summary>
  /// <inheritdoc cref="KasaDevice(PhysicalAddress, IServiceProvider)" />
  public KL130(
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
  /// Initializes a new instance of the <see cref="KL130"/> class.
  /// </summary>
  /// <inheritdoc cref="KasaDevice(IDeviceEndPoint, IServiceProvider?)" />
  public KL130(
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
  /// Initializes a new instance of the <see cref="KL130"/> class.
  /// </summary>
  /// <inheritdoc cref="KasaDevice.Create{TAddress}(TAddress, IServiceProvider)" />
  public static new KL130 Create<TAddress>(
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

  private static TimeSpan ValidateTransitionPeriod(TimeSpan value, string paramName)
  {
    if (value < TimeSpan.Zero)
      throw new ArgumentOutOfRangeException(paramName: paramName, actualValue: value, message: "The value for transition period must be zero or positive value.");

    return value;
  }

  async ValueTask<IDeviceInfo> ISmartDevice.GetDeviceInfoAsync(CancellationToken cancellationToken)
    => await GetDeviceInfoAsync(cancellationToken).ConfigureAwait(false) ?? throw new InvalidOperationException("Could not retrieve device information since it was null.");

#pragma warning disable SA1313
  private readonly record struct SetOnOffStateParameter(
    [property: JsonPropertyName("on_off")]
    [property: JsonConverter(typeof(KasaNumericalBooleanJsonConverter))]
    bool OnOff,
    [property: JsonPropertyName("transition_period")]
    int TransitionPeriodInMilliseconds
  ) {
#pragma warning restore SA1313
#pragma warning disable CA1822
    [JsonPropertyName("ignore_default")]
    public int IgnoreDefault => 0; // turn on/off with the default configuration
#pragma warning restore CA1822
  }

  /// <summary>
  /// Turns on the device.
  /// </summary>
  /// <param name="transitionPeriod">
  /// The value that indicates the time interval between completion of gradual state transition.
  /// If <see cref="TimeSpan.Zero"/>, the state transition will be performed immediately rather than gradual change.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  public ValueTask TurnOnAsync(
    TimeSpan transitionPeriod = default,
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

  /// <summary>
  /// Turns off the device.
  /// </summary>
  /// <param name="transitionPeriod">
  /// The value that indicates the time interval between completion of gradual state transition.
  /// If <see cref="TimeSpan.Zero"/>, the state transition will be performed immediately rather than gradual change.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  public ValueTask TurnOffAsync(
    TimeSpan transitionPeriod = default,
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
  /// Sets the on/off state of the device according to the parameter <paramref name="newOnOffState" />.
  /// </summary>
  /// <param name="newOnOffState">
  /// The value that indicates new on/off state to be set. <see langword="true"/> for on, otherwise off.
  /// </param>
  /// <param name="transitionPeriod">
  /// The value that indicates the time interval between completion of gradual state transition.
  /// If <see cref="TimeSpan.Zero"/>, the state transition will be performed immediately rather than gradual change.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  public ValueTask SetOnOffStateAsync(
    bool newOnOffState,
    TimeSpan transitionPeriod = default,
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

  ValueTask ISmartDevice.SetOnOffStateAsync(
    bool newOnOffState,
    CancellationToken cancellationToken
  )
    => SetOnOffStateAsync(
      newOnOffState: newOnOffState,
      transitionPeriod: default,
      cancellationToken: cancellationToken
    );

  private readonly struct GetSysInfoLightStateParameter {
    [JsonPropertyName("light_state")]
    public KL130LightState LightState { get; init; }
  }

  /// <summary>
  /// Gets the current light state of the device.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  public ValueTask<KL130LightState> GetLightStateAsync(
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync(
      module: ModuleTextSystem,
      method: MethodTextGetSysInfo,
      composeResult: static result => JsonSerializer.Deserialize<GetSysInfoLightStateParameter>(result).LightState,
      cancellationToken: cancellationToken
    );

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
      composeResult: static result => JsonSerializer.Deserialize<GetSysInfoLightStateParameter>(result).LightState.IsOn,
      cancellationToken: cancellationToken
    );

#pragma warning disable SA1313
  private readonly record struct SetColorTemperatureParameter(
    [property: JsonPropertyName("color_temp")]
    int ColorTemperature,
    [property: JsonPropertyName("brightness")]
    int? Brightness,
    [property: JsonPropertyName("transition_period")]
    int TransitionPeriodInMilliseconds
  ) {
#pragma warning restore SA1313
#pragma warning disable CA1822
    [JsonPropertyName("on_off")]
    [JsonConverter(typeof(KasaNumericalBooleanJsonConverter))]
    public bool OnOff => true; // on

    [JsonPropertyName("ignore_default")]
    public int IgnoreDefault => 1; // ignore the default configuration
#pragma warning restore CA1822
  }

  /// <summary>
  /// Turns the light on and sets the light color to the specified color temperature.
  /// </summary>
  /// <param name="colorTemperature">
  /// The color temperature in kelvin, in range of 2500~9000[K].
  /// </param>
  /// <param name="brightness">
  /// The brightness in percent value, in range of 1~100[%]. If <see langword="null"/>, the current brightness will be kept.
  /// </param>
  /// <param name="transitionPeriod">
  /// The value that indicates the time interval between completion of gradual state transition.
  /// If <see cref="TimeSpan.Zero"/>, the state transition will be performed immediately rather than gradual change.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  public ValueTask SetColorTemperatureAsync(
    int colorTemperature,
    int? brightness = null,
    TimeSpan transitionPeriod = default,
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

#pragma warning disable SA1313
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
#pragma warning restore SA1313
#pragma warning disable CA1822
    [JsonPropertyName("color_temp")]
    public int ColorTemperature => 0; // set color with hue and saturation

    [JsonPropertyName("on_off")]
    [JsonConverter(typeof(KasaNumericalBooleanJsonConverter))]
    public bool OnOff => true; // on

    [JsonPropertyName("ignore_default")]
    public int IgnoreDefault => 1; // ignore the default configuration
#pragma warning restore CA1822
  }

  /// <summary>
  /// Turns the light on and sets the light color to the specified color represented by hue and saturation.
  /// </summary>
  /// <param name="hue">
  /// The hue of the color in degree, in range of 0~360[°].
  /// </param>
  /// <param name="saturation">
  /// The saturation of the color in percent value, in range of 0~100[%].
  /// </param>
  /// <param name="brightness">
  /// The brightness in percent value, in range of 1~100[%]. If <see langword="null"/>, the current brightness will be kept.
  /// </param>
  /// <param name="transitionPeriod">
  /// The value that indicates the time interval between completion of gradual state transition.
  /// If <see cref="TimeSpan.Zero"/>, the state transition will be performed immediately rather than gradual change.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  public ValueTask SetColorAsync(
    int hue,
    int saturation,
    int? brightness = null,
    TimeSpan transitionPeriod = default,
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

  /// <summary>
  /// Turns the light on and sets the light brightness.
  /// </summary>
  /// <param name="brightness">
  /// The brightness in percent value, in range of 1~100[%].
  /// </param>
  /// <param name="transitionPeriod">
  /// The value that indicates the time interval between completion of gradual state transition.
  /// If <see cref="TimeSpan.Zero"/>, the state transition will be performed immediately rather than gradual change.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  public ValueTask SetBrightnessAsync(
    int brightness,
    TimeSpan transitionPeriod = default,
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync(
      module: ModuleTextLightingService,
      method: MethodTextTransitionLightState,
      new SetColorParameter(
        Hue: null,
        Saturation: null,
        Brightness: brightness, // TODO: validation
        TransitionPeriodInMilliseconds: (int)ValidateTransitionPeriod(transitionPeriod, nameof(transitionPeriod)).TotalMilliseconds
      ),
      cancellationToken
    );
}
