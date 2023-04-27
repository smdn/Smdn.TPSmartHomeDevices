// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo;

/// <summary>
/// Provides APIs to operate Tapo L900, Smart Wi-Fi Light Strip.
/// </summary>
/// <remarks>
/// This is an unofficial API that has no affiliation with TP-Link.
/// This API is released under the <see href="https://opensource.org/license/mit/">MIT License</see>, and as stated in the terms of the MIT License,
/// there is no warranty for the results of using this API and no responsibility is taken for those results.
/// </remarks>
/// <seealso href="https://www.tp-link.com/jp/smart-home/tapo/tapo-l900-5/">Tapo L900-5 product information (ja)</seealso>
/// <seealso href="https://www.tp-link.com/jp/smart-home/tapo/tapo-l900-10/">Tapo L900-10 product information (ja)</seealso>
/// <seealso href="https://www.tapo.com/en/product/smart-light-bulb/tapo-l900-5/">Tapo Tapo L900-5 product information (en)</seealso>
/// <seealso href="https://www.tapo.com/en/product/smart-light-bulb/tapo-l900-10/">Tapo Tapo L900-10 product information (en)</seealso>
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
  ///   cref="TapoDevice(IDeviceEndPoint, ITapoCredentialProvider?, TapoDeviceExceptionHandler?, IServiceProvider?)"
  ///   path="/exception | /param[@name='deviceEndPoint' or @name='credential' or @name='serviceProvider']"
  /// />
  public L900(
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
  public static new L900 Create<TAddress>(
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

  /// <summary>
  /// Turns the light on and sets the light brightness.
  /// </summary>
  /// <param name="brightness">
  /// The brightness in percent value, in range of 1~100[%].
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  public ValueTask SetBrightnessAsync(
    int brightness,
    CancellationToken cancellationToken = default
  )
    => SetDeviceInfoAsync(
      new SetBrightnessParameter(
        Brightness: MulticolorLightUtils.ValidateBrightnessValue(brightness, nameof(brightness)).Value
      ),
      cancellationToken
    );

  /// <summary>
  /// Turns the light on and sets the light color to the specified color represented by hue and satulation.
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
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  public ValueTask SetColorAsync(
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

  /// <summary>
  /// Turns the light on and sets the light color to the specified hue.
  /// </summary>
  /// <param name="hue">
  /// The hue of the color in degree, in range of 0~360[°].
  /// </param>
  /// <param name="brightness">
  /// The brightness in percent value, in range of 1~100[%]. If <see langword="null"/>, the current brightness will be kept.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  public ValueTask SetColorHueAsync(
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

  /// <summary>
  /// Turns the light on and sets the light color to the specified saturation.
  /// </summary>
  /// <param name="saturation">
  /// The saturation of the color in percent value, in range of 0~100[%].
  /// </param>
  /// <param name="brightness">
  /// The brightness in percent value, in range of 1~100[%]. If <see langword="null"/>, the current brightness will be kept.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  public ValueTask SetColorSaturationAsync(
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
