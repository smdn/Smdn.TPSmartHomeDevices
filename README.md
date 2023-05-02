[![GitHub license](https://img.shields.io/github/license/smdn/Smdn.TPSmartHomeDevices)](https://github.com/smdn/Smdn.TPSmartHomeDevices/blob/main/LICENSE.txt)
[![tests/main](https://img.shields.io/github/actions/workflow/status/smdn/Smdn.TPSmartHomeDevices/test.yml?branch=main&label=tests%2Fmain)](https://github.com/smdn/Smdn.TPSmartHomeDevices/actions/workflows/test.yml)
[![CodeQL](https://github.com/smdn/Smdn.TPSmartHomeDevices/actions/workflows/codeql-analysis.yml/badge.svg?branch=main)](https://github.com/smdn/Smdn.TPSmartHomeDevices/actions/workflows/codeql-analysis.yml)

# Smdn.TPSmartHomeDevices
The .NET implementations for [Kasa](https://www.kasasmart.com) and [Tapo](https://www.tapo.com/), the TP-Link smart home devices.

## Smdn.TPSmartHomeDevices.Tapo / Smdn.TPSmartHomeDevices.Kasa
|Library|NuGet package|View code|
|-|-|-|
|`Smdn.TPSmartHomeDevices.Tapo`|[![NuGet](https://img.shields.io/nuget/v/Smdn.TPSmartHomeDevices.Tapo.svg)](https://www.nuget.org/packages/Smdn.TPSmartHomeDevices.Tapo/)|[Smdn.TPSmartHomeDevices.Tapo](./src/Smdn.TPSmartHomeDevices.Tapo/)|
|`Smdn.TPSmartHomeDevices.Kasa`|[![NuGet](https://img.shields.io/nuget/v/Smdn.TPSmartHomeDevices.Kasa.svg)](https://www.nuget.org/packages/Smdn.TPSmartHomeDevices.Kasa/)|[Smdn.TPSmartHomeDevices.Kasa](./src/Smdn.TPSmartHomeDevices.Kasa/)|

[Smdn.TPSmartHomeDevices.Tapo](./src/Smdn.TPSmartHomeDevices.Tapo/) and [Smdn.TPSmartHomeDevices.Kasa](./src/Smdn.TPSmartHomeDevices.Kasa/) are class libraries that provide .NET APIs for operating Tapo and Kasa smart home devices. These libraries perform operations by communicating directly with Tapo/Kasa devices in the same network.

These class libraries provide classes such as [L530](./examples/Smdn.TPSmartHomeDevices.Tapo/L530MulticolorBulb/) and [KL130](./examples/Smdn.TPSmartHomeDevices.Kasa/KL130MulticolorBulb/), which have the same name as the device's product name. These classes do not simply provide methods to wrap the sending of requests to the device, but also provide following features:

- Automatic connection/authentication/session management, including reconnection and re-authentication.
- Built-in/customizable error handling for typical errors and retries (like device busy, session expired, request timeout)
- Using MAC address and following IP address change in DHCP networks (requires [Smdn.TPSmartHomeDevices.MacAddressEndPoint](#smdntpsmarthomedevicesmacaddressendpoint)).
- `async` operation and cancellation.

The following is an example of code to operate L530. This example illustrates the basic API usage as well as what happens in the background of a method.

```csharp
using Smdn.TPSmartHomeDevices.Tapo;

// Creates client for L530 multicolor light bulb
using var bulb = new L530("192.0.2.255", "user@mail.test", "password");

// Turn on the bulb, and set the color temperature and brightness.
await bulb.SetColorTemperatureAsync(colorTemperature: 5500, brightness: 80);
//    Here, connections and sessions are established automatically.
//    Also attempts retry automatically when recoverable errors
//    such as device busy or timeout occur.

// Suppose a few minutes, hours or days passes here.
await Task.Delay(TimeSpan.FromHours(...));

// Then, sets the color and brightness of the bulb.
await bulb.SetColorTemperatureAsync(colorTemperature: 4000, brightness: 40);
//    At this time, if the connection or session has expired,
//    it will attempt to reconnect/re-authenticate automatically.
//    Also, if the connection is established using a MAC address and
//    the resolved endpoint is unreachable, it will attempt to
//    resolve it again. (requires Smdn.TPSmartHomeDevices.MacAddressEndPoint)
```

### Supported devices and functions
The following devices and functions are currently supported. (as of version 1.0.0)

- Tapo devices
  - L530 multicolor bulb - [example](./examples/Smdn.TPSmartHomeDevices.Tapo/L530MulticolorBulb/)
  - L900 multicolor light strip - [example](./examples/Smdn.TPSmartHomeDevices.Tapo/L900MulticolorLightStrip/)
  - P105 smart plug - [example](./examples/Smdn.TPSmartHomeDevices.Tapo/P105Plug/)
- Kasa devices
  - HS105 smart plug - [example](./examples/Smdn.TPSmartHomeDevices.Kasa/HS105Plug/)
  - KL130 multicolor bulb - [example](./examples/Smdn.TPSmartHomeDevices.Kasa/KL130MulticolorBulb/)
- Functions
  - Turn on/off
  - Set color (color temperature)
  - Set color (hue and saturation)
  - Set brightness
  - Get device informations (Tapo)
  - Get light color/brightness
  - Get on/off stete

### Library features
- Specifying devices by MAC address - [example](./examples/Smdn.TPSmartHomeDevices.MacAddressEndPoint/MacAddressResolution/), see also: [Smdn.TPSmartHomeDevices.MacAddressEndPoint](#smdntpsmarthomedevicesmacaddressendpoint)
- Built-in and customizable error handling
    - [Tapo example](./examples/Smdn.TPSmartHomeDevices.Tapo/CustomExceptionHandling/)
    - [Kasa example](./examples/Smdn.TPSmartHomeDevices.Kasa/CustomExceptionHandling/)
- Supports dependency injection (`Microsoft.Extensions.DependencyInjection`)
  - Logging (`Microsoft.Extensions.Logging`)
    - [Tapo example](./examples/Smdn.TPSmartHomeDevices.Tapo/Logging/)
    - [Kasa example](./examples/Smdn.TPSmartHomeDevices.Kasa/Logging/)
  - HTTP (`Microsoft.Extensions.Http`) - [example](./examples/Smdn.TPSmartHomeDevices.Tapo/ConfigureTimeout/)
- Customizable Tapo credential provider - [example](./examples/Smdn.TPSmartHomeDevices.Tapo/Credentials/)
- Configuring timeout and cancellation
  - [Tapo example](./examples/Smdn.TPSmartHomeDevices.Tapo/ConfigureTimeout/)
  - [Kasa example](./examples/Smdn.TPSmartHomeDevices.Kasa/ConfigureTimeout/)

### Testing
- Tested with pseudo devices and actual devices
- Confirmed to work on:
  - Windows 10
  - Ubuntu 22.04 LTS
  - Raspbian GNU/Linux 9.13 (stretch); Raspberry Pi 3 Model B+

### Feature Request
(See also [Contribution guidelines](#for-contributers))

If you have a request that you would like library to add API for the device functions to devices currently supported, please send it as a [Feature Request](/../../issues/new?template=02_feature-request.yml) or Pull Request.

If you would like to request support for a device that is not currently supported, please send a Pull Request. Alternatively, please consider [supporting this project](https://github.com/sponsors/smdn?frequency=one-time) through GitHub Sponsors.

[![](https://img.shields.io/static/v1?label=Sponsor&message=%E2%9D%A4&logo=GitHub&color=%23fe8e86)](https://github.com/sponsors/smdn?frequency=one-time)

When adding support for a new device, I would like to purchase and perform testing with the actual device as much as possible, if it is available in Japan.

## Smdn.TPSmartHomeDevices.MacAddressEndPoint
|NuGet package|View code|
|-|-|
|[![NuGet](https://img.shields.io/nuget/v/Smdn.TPSmartHomeDevices.MacAddressEndPoint.svg)](https://www.nuget.org/packages/Smdn.TPSmartHomeDevices.MacAddressEndPoint/)|[Smdn.TPSmartHomeDevices.MacAddressEndPoint](./src/Smdn.TPSmartHomeDevices.MacAddressEndPoint/)|

`Smdn.TPSmartHomeDevices.MacAddressEndPoint` is an extension library that enables to use MAC addresses to specify the device endpoints, instead of IP addresses or host names. This library also enables to support following changes of the device endpoint in network where IP addresses are dynamic, such as networks using DHCP.

See [this example](./examples/Smdn.TPSmartHomeDevices.MacAddressEndPoint/MacAddressResolution/) for using MAC addresses to identify the Tapo and Kasa devices.

This library relies on [Smdn.Net.AddressResolution](https://www.nuget.org/packages/Smdn.Net.AddressResolution) [![Smdn.Net.AddressResolution](https://img.shields.io/nuget/v/Smdn.Net.AddressResolution.svg)](https://www.nuget.org/packages/Smdn.Net.AddressResolution/) for MAC address resolution. For further details such as functions and supported platforms, refer [smdn/Smdn.Net.AddressResolution](https://github.com/smdn/Smdn.Net.AddressResolution) repository.

## Smdn.TPSmartHomeDevices.Primitives
|NuGet package|View code|
|-|-|
|[![NuGet](https://img.shields.io/nuget/v/Smdn.TPSmartHomeDevices.Primitives.svg)](https://www.nuget.org/packages/Smdn.TPSmartHomeDevices.Primitives/)|[Smdn.TPSmartHomeDevices.Primitives](./src/Smdn.TPSmartHomeDevices.Primitives/)|

`Smdn.TPSmartHomeDevices.Primitives` provides common types for `Smdn.TPSmartHomeDevices.*`. This library includes abstraction interfaces, extension methods and custom `JsonConverter`s. This library does not provide any specific implementations to operate Kasa and Tapo devices.

More description to be added.

# For contributers
Contributions are appreciated!

If there's a feature you would like to add or a bug you would like to fix, please read [Contribution guidelines](./CONTRIBUTING.md) and create an Issue or Pull Request.

IssueやPull Requestを送る際は、[Contribution guidelines](./CONTRIBUTING.md)をご覧頂ください。　可能なら英語が望ましいですが、日本語で構いません。

# Notice
<!-- #pragma section-start NupkgReadmeFile_Notice -->
## License
This project is licensed under the terms of the [MIT License](./LICENSE.txt).

## Disclaimer
(An English translation for the reference follows the text written in Japanese.)

本プロジェクトは、TP-Linkとは無関係の非公式なものです。

This is an unofficial project that has no affiliation with TP-Link.

本プロジェクトが提供するソフトウェアは、デバイスの設定の取得・変更等、製品仕様の範囲内での操作のみを行うものであり、ファームウェアの改変・修正および製品の改造や製品仕様の変更を引き起こさないものの、**製品使用上の許諾事項に抵触する可能性は否定できないため、使用の際はその点にご留意ください。**

The software provided by this project is intended only for operations within the scope of the product specifications, such as acquiring and changing device settings, and while it does not cause altering the product itself, product's firmware or operating specifications. **Nevertheless, please note that the possibility of violating terms of use of the product cannot be dismissed when using the software provided by this project.**

[Tapo](https://www.tapo.com/)、[Kasa](https://www.kasasmart.com/)、および各製品名の著作権は[TP-Link](https://www.tp-link.com/)に帰属します。

[Tapo](https://www.tapo.com/), [Kasa](https://www.kasasmart.com/) and all respective product names are copyright of [TP-Link](https://www.tp-link.com/).

## Credit
This project incorporates implementations partially ported from the following projects. See also [ThirdPartyNotices.md](./ThirdPartyNotices.md) for detail.

- [fishbigger/TapoP100](https://github.com/fishbigger/TapoP100)
- [europowergenerators/Tapo-plug-controller](https://github.com/europowergenerators/Tapo-plug-controller)
- [plasticrake/tplink-smarthome-api](https://github.com/plasticrake/tplink-smarthome-api/)
- [plasticrake/tplink-smarthome-crypto](https://github.com/plasticrake/tplink-smarthome-crypto)
<!-- #pragma section-end NupkgReadmeFile_Notice -->
