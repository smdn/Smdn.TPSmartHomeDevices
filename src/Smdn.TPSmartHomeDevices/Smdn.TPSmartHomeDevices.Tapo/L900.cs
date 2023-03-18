// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices.Tapo;

public class L900 : TapoDevice {
  /// <summary>
  /// Initializes a new instance of the <see cref="L900"/> class with specifying the device endpoint by host name.
  /// </summary>
  /// <inheritdoc cref="TapoDevice(string, string, string, IServiceProvider?)" />
  public L900(
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
  /// Initializes a new instance of the <see cref="L900"/> class with specifying the device endpoint by host name.
  /// </summary>
  /// <inheritdoc cref="TapoDevice(string, IServiceProvider)" />
  public L900(
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
  /// Initializes a new instance of the <see cref="L900"/> class with specifying the device endpoint by IP address.
  /// </summary>
  /// <inheritdoc cref="TapoDevice(string, string, string, IServiceProvider?)" />
  public L900(
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
  /// Initializes a new instance of the <see cref="L900"/> class with specifying the device endpoint by IP address.
  /// </summary>
  /// <inheritdoc cref="TapoDevice(IPAddress, IServiceProvider)" />
  public L900(
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
  /// Initializes a new instance of the <see cref="L900"/> class with specifying the device endpoint by MAC address.
  /// </summary>
  /// <inheritdoc cref="TapoDevice(PhysicalAddress, string, string, IServiceProvider?)" />
  public L900(
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
  /// Initializes a new instance of the <see cref="L900"/> class with specifying the device endpoint by MAC address.
  /// </summary>
  /// <inheritdoc cref="TapoDevice(PhysicalAddress, IServiceProvider)" />
  public L900(
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
  /// Initializes a new instance of the <see cref="L900"/> class.
  /// </summary>
  /// <inheritdoc
  ///   cref="TapoDevice(IDeviceEndPointProvider, ITapoCredentialProvider?, Protocol.TapoClientExceptionHandler?, IServiceProvider?)"
  ///   path="/exception | /param[@name='deviceEndPointProvider' or @name='credentialProvider' or @name='serviceProvider']"
  /// />
  public L900(
    IDeviceEndPointProvider deviceEndPointProvider,
    ITapoCredentialProvider? credentialProvider = null,
    IServiceProvider? serviceProvider = null
  )
    : base(
      deviceEndPointProvider: deviceEndPointProvider,
      credentialProvider: credentialProvider,
      serviceProvider: serviceProvider
    )
  {
  }

#pragma warning disable SA1313, CA1822
  internal readonly record struct LightingEffectParameter(
    [property: JsonPropertyName("enable")] int Enable
  ) {
    public static readonly LightingEffectParameter Disabled = new(0);
  }

  private readonly record struct SetBrightnessParameter(
    [property: JsonPropertyName("brightness")] int Brightness
  ) {
    [JsonPropertyName("device_on")]
    public bool DeviceOn => true;

    [JsonPropertyName("lighting_effect")]
    public LightingEffectParameter LightingEffect => LightingEffectParameter.Disabled;
  }

  private readonly record struct SetColorParameter(
    [property: JsonPropertyName("hue")] int? Hue,
    [property: JsonPropertyName("saturation")] int? Saturation,
    [property: JsonPropertyName("brightness")] int? Brightness
  ) {
    [JsonPropertyName("device_on")]
    public bool DeviceOn => true;

    [JsonPropertyName("lighting_effect")]
    public LightingEffectParameter LightingEffect => LightingEffectParameter.Disabled;
  }
#pragma warning restore SA1313, CA1822

  /// <param name="brightness">
  /// The brightness in percent value, in range of 1~100[%].
  /// </param>
  public Task SetBrightnessAsync(
    int brightness,
    CancellationToken cancellationToken = default
  )
    => SetDeviceInfoAsync(
      new SetBrightnessParameter(
        Brightness: MulticolorLightUtils.ValidateBrightnessValue(brightness, nameof(brightness)).Value
      ),
      cancellationToken
    );

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
    CancellationToken cancellationToken = default
  )
    => SetDeviceInfoAsync(
      new SetColorParameter(
        Hue: MulticolorLightUtils.ValidateHueValue(hue, nameof(hue)),
        Saturation: MulticolorLightUtils.ValidateSaturationValue(saturation, nameof(saturation)),
        Brightness: MulticolorLightUtils.ValidateBrightnessValue(brightness, nameof(brightness))
      ),
      cancellationToken
    );

  /// <param name="hue">
  /// The hue of the color in degree, in range of 0~360[°].
  /// </param>
  /// <param name="brightness">
  /// The brightness in percent value, in range of 1~100[%]. If <see langword="null"/>, the current brightness will be kept.
  /// </param>
  public Task SetColorHueAsync(
    int hue,
    int? brightness,
    CancellationToken cancellationToken = default
  )
    => SetDeviceInfoAsync(
      new SetColorParameter(
        Hue: MulticolorLightUtils.ValidateHueValue(hue, nameof(hue)),
        Saturation: null,
        Brightness: MulticolorLightUtils.ValidateBrightnessValue(brightness, nameof(brightness))
      ),
      cancellationToken
    );

  /// <param name="saturation">
  /// The saturation of the color in percent value, in range of 0~100[%].
  /// </param>
  /// <param name="brightness">
  /// The brightness in percent value, in range of 1~100[%]. If <see langword="null"/>, the current brightness will be kept.
  /// </param>
  public Task SetColorSaturationAsync(
    int saturation,
    int? brightness = null,
    CancellationToken cancellationToken = default
  )
    => SetDeviceInfoAsync(
      new SetColorParameter(
        Hue: null,
        Saturation: MulticolorLightUtils.ValidateSaturationValue(saturation, nameof(saturation)),
        Brightness: MulticolorLightUtils.ValidateBrightnessValue(brightness, nameof(brightness))
      ),
      cancellationToken
    );
}
