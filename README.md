[![GitHub license](https://img.shields.io/github/license/smdn/Smdn.TPSmartHomeDevices)](https://github.com/smdn/Smdn.TPSmartHomeDevices/blob/main/COPYING.txt)
[![tests/main](https://img.shields.io/github/actions/workflow/status/smdn/Smdn.TPSmartHomeDevices/test.yml?branch=main&label=tests%2Fmain)](https://github.com/smdn/Smdn.TPSmartHomeDevices/actions/workflows/test.yml)
[![CodeQL](https://github.com/smdn/Smdn.TPSmartHomeDevices/actions/workflows/codeql-analysis.yml/badge.svg?branch=main)](https://github.com/smdn/Smdn.TPSmartHomeDevices/actions/workflows/codeql-analysis.yml)

# Smdn.TPSmartHomeDevices
The .NET implementations for controlling [Kasa](https://www.kasasmart.com) and [Tapo](https://www.tapo.com/), the TP-Link smart home devices.

## Smdn.TPSmartHomeDevices.Tapo / Smdn.TPSmartHomeDevices.Kasa
- Smdn.TPSmartHomeDevices.Tapo: [![NuGet](https://img.shields.io/nuget/v/Smdn.TPSmartHomeDevices.Tapo.svg)](https://www.nuget.org/packages/Smdn.TPSmartHomeDevices.Tapo/) [![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
- Smdn.TPSmartHomeDevices.Kasa: [![NuGet](https://img.shields.io/nuget/v/Smdn.TPSmartHomeDevices.Kasa.svg)](https://www.nuget.org/packages/Smdn.TPSmartHomeDevices.Kasa/) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)|

[Smdn.TPSmartHomeDevices.Tapo](./src/Smdn.TPSmartHomeDevices.Tapo/) and [Smdn.TPSmartHomeDevices.Kasa](./src/Smdn.TPSmartHomeDevices.Kasa/) are class libraries that provide .NET APIs for operating Tapo and Kasa smart home devices.

These class libraries provide device classes such as `L530` (`Smdn.TPSmartHomeDevices.Tapo` namespace) and `KL130` (`Smdn.TPSmartHomeDevices.Kasa` namespace), which have the same name as the device's product model name.

These device classes perform operations by communicating directly with Tapo/Kasa devices in the same network. Remote operation via the Internet is not supported.

```csharp
using Smdn.TPSmartHomeDevices.Tapo;

// Creates device object for L530 multicolor light bulb
using var bulb = new L530(
  "192.0.2.1",      // IP address currently assigned to the device
  "user@mail.test", // E-mail address for your Tapo account
  "password"        // Password for your Tapo account
);

// Sets the color temperature and brightness.
// In the off state, the bulb will automatically turn on.
await bulb.SetColorTemperatureAsync(colorTemperature: 5500, brightness: 80);
```

### Supported device and functions
#### Supported devices
The following devices have been confirmed to work on the actual devices:

|Model|Device type|Hardware version|Hardware specs|Firmware version|Usage example|
|-|-|-|-|-|-|
|Tapo L530|Bulb|1.0.0<br/>1.20|JP<br/>JP|1.3.0 Build 20230831 Rel. 75926<br/>1.1.0 Build 230823 Rel.162531|[example](./examples/Smdn.TPSmartHomeDevices.Tapo/L530MulticolorBulb/)|
|Tapo L900|Light strip|1.0|-|1.1.0 Build 230905 Rel.184939|[example](./examples/Smdn.TPSmartHomeDevices.Tapo/L900MulticolorLightStrip/)|
|Tapo P105|Plug|1.0.0|JP|1.4.1 Build 20231103 Rel. 36519|[example](./examples/Smdn.TPSmartHomeDevices.Tapo/P105Plug/)|
|Tapo P110M|Plug|1.0|JP|1.1.0 Build 231009 Rel.155719|[example](./examples/Smdn.TPSmartHomeDevices.Tapo/P110MPlug/)|
|Kasa KL130|Bulb|1.0|JP|1.8.11 Build 191113 Rel.105336|[example](./examples/Smdn.TPSmartHomeDevices.Kasa/KL130MulticolorBulb/)|
|Kasa HS105|Plug|1.0|JP|1.5.8 Build 191125 Rel.135255|[example](./examples/Smdn.TPSmartHomeDevices.Kasa/HS105Plug/)|

#### Supported functions
The library supports performing the following device functions:

- Turn on/off
- Set color (color temperature)
- Set color (hue and saturation)
- Set brightness
- Get on/off stete
- Get current light color/brightness
- Get monitoring data: power consumption and cumulative energy usage [Tapo P110M]
- Get device informations ([Tapo example](./examples/Smdn.TPSmartHomeDevices.Tapo/DisplayDeviceInfo/), [Kasa example](./examples/Smdn.TPSmartHomeDevices.Kasa/DisplayDeviceInfo/))
- Get device usage: operating time and cumulative energy usage (only for Tapo devices, [example](./examples/Smdn.TPSmartHomeDevices.Tapo/DeviceUsage/))

#### Confirmed to work
The library has been tested and confirmed to work with actual devices, on the following environments:
- Windows 10
- Ubuntu 22.04 LTS
- Raspbian GNU/Linux 9.13 (stretch); Raspberry Pi 3 Model B+


### More library features
The device class such as `L530` does not simply provide methods to wrap the sending of requests to the device. `Smdn.TPSmartHomeDevices.Tapo` and `Smdn.TPSmartHomeDevices.Kasa` also provides the following features.

The following example illustrates the basic API usage as well as what happens in the background of a method.

```csharp
using Smdn.TPSmartHomeDevices.Tapo;

// Creates client for L530 multicolor light bulb
using var bulb = new L530("192.0.2.1", "user@mail.test", "password");

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

#### Automated session management
Connection and authentication to the device is performed automatically when a request is sent to the device. Reconnection and reauthentication is also performed automatically when an exception occurs or when a session expires.

#### Built-in and customizable retry and error handling
Built-in error handling is provided by default for typical errors such as device busy, session expired, or request timeout. Customized error handling is also available, allowing you to define handling for each type of exception and retries. See [Tapo example](./examples/Smdn.TPSmartHomeDevices.Tapo/CustomExceptionHandling/) and [Kasa example](./examples/Smdn.TPSmartHomeDevices.Kasa/CustomExceptionHandling/).

#### Supports default protocol (`securePassthrough`) and new protocol (`KLAP`)
`Smdn.TPSmartHomeDevices.Tapo` version 2.0.0 or later supports the new protocol `KLAP` for Tapo devices. By default, the appropriate protocol is automatically selected. You can also explicitly specify a protocol. See [this example](./examples/Smdn.TPSmartHomeDevices.Tapo/SelectProtocol/).

#### Addressing devices using MAC addresses
Supports specifying device endpoint by MAC address. This is useful in networks with variable IP addresses, such as networks using DHCP. This feature requires an extension library [Smdn.TPSmartHomeDevices.MacAddressEndPoint](#smdntpsmarthomedevicesmacaddressendpoint).

#### Other features
- Supports dependency injection (`Microsoft.Extensions.DependencyInjection`)
  - Logging (`Microsoft.Extensions.Logging`) - [Tapo example](./examples/Smdn.TPSmartHomeDevices.Tapo/Logging/), [Kasa example](./examples/Smdn.TPSmartHomeDevices.Kasa/Logging/)
  - HTTP (`Microsoft.Extensions.Http`) - [example](./examples/Smdn.TPSmartHomeDevices.Tapo/ConfigureTimeout/)
- Providing Tapo credentials via environment variables - [example](./examples/Smdn.TPSmartHomeDevices.Tapo/CredentialsEnvVar/)
- Customizable Tapo credential provider - [example](./examples/Smdn.TPSmartHomeDevices.Tapo/Credentials/)
- Configuring timeout and cancellation - [Tapo example](./examples/Smdn.TPSmartHomeDevices.Tapo/ConfigureTimeout/), [Kasa example](./examples/Smdn.TPSmartHomeDevices.Kasa/ConfigureTimeout/)

### Recommended usage on Tapo devices
Tapo devices have introduced secure authentication methods in new firmware released after summer 2023. If new firmware is installed, hashed credentials can be used for authentication. This means that it is no longer necessary to embed the username and password in plain text.

Additionally, Smdn.TPSmartHomeDevices.Tapo can retrieve hashed credentials from environment variables.

The following code shows an example of retrieving hashed credential from the `TAPO_KLAP_LOCALAUTHHASH` environment variable and using it when authenticating to a Tapo device.

```csharp
using Microsoft.Extensions.DependencyInjection;

using Smdn.TPSmartHomeDevices.Tapo;
using Smdn.TPSmartHomeDevices.Tapo.Credentials;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

var services = new ServiceCollection();

// Specifies that the device should be operated using the newer protocol.
services.AddTapoProtocolSelector(TapoSessionProtocol.Klap);

// Specifies the environment variable in which the hashed credential
// used for authentication is set.
services.AddTapoBase64EncodedKlapCredentialFromEnvironmentVariable(
  envVarBase64KlapLocalAuthHash: "TAPO_KLAP_LOCALAUTHHASH"
);

using var plug = new P105("192.0.2.1", services.BuildServiceProvider());

await plug.TurnOnAsync();
```

The environment variable `TAPO_KLAP_LOCALAUTHHASH` has to be a BASE64 string calculated by the formula `BASE64(SHA256(SHA1(username) + SHA1(password)))`. See [this example](./examples/Smdn.TPSmartHomeDevices.Tapo/CredentialsEnvVar/) for detail.

> [!NOTE]
> Although `Smdn.TPSmartHomeDevices.Tapo` still supports devices with older protocol/firmware, it is recommended that you update your Tapo device's firmware to the latest version before using the library.



### Feature Request
If you have a request that you would like library to add API for the device functions to devices currently supported, please send it as a [Feature Request](/../../issues/new?template=02_feature-request.yml) or Pull Request.

> [!NOTE]
> See also [Contribution guidelines](#for-contributers).

If you would like to request support for a device that is not currently supported, please send a Pull Request. Alternatively, please consider [supporting this project](https://github.com/sponsors/smdn?frequency=one-time) through GitHub Sponsors.

[![](https://img.shields.io/static/v1?label=Sponsor&message=%E2%9D%A4&logo=GitHub&color=%23fe8e86)](https://github.com/sponsors/smdn?frequency=one-time)

When adding support for a new device, I would like to purchase and perform testing with the actual device as much as possible, if it is available in Japan.

## Smdn.TPSmartHomeDevices.MacAddressEndPoint
[![NuGet](https://img.shields.io/nuget/v/Smdn.TPSmartHomeDevices.MacAddressEndPoint.svg)](https://www.nuget.org/packages/Smdn.TPSmartHomeDevices.MacAddressEndPoint/) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

[Smdn.TPSmartHomeDevices.MacAddressEndPoint](./src/Smdn.TPSmartHomeDevices.MacAddressEndPoint/) is an extension library that enables to use MAC addresses to specify the device endpoints, instead of IP addresses or host names. This library also enables to support following changes of the device endpoint in network where IP addresses are dynamic, such as networks using DHCP.

See [this example](./examples/Smdn.TPSmartHomeDevices.MacAddressEndPoint/MacAddressResolution/) for using MAC addresses to identify the Tapo and Kasa devices.

This library relies on [Smdn.Net.AddressResolution](https://www.nuget.org/packages/Smdn.Net.AddressResolution) [![Smdn.Net.AddressResolution](https://img.shields.io/nuget/v/Smdn.Net.AddressResolution.svg)](https://www.nuget.org/packages/Smdn.Net.AddressResolution/) for MAC address resolution. For further details such as functions and supported platforms, refer [smdn/Smdn.Net.AddressResolution](https://github.com/smdn/Smdn.Net.AddressResolution) repository.

## Smdn.TPSmartHomeDevices.Primitives
[![NuGet](https://img.shields.io/nuget/v/Smdn.TPSmartHomeDevices.Primitives.svg)](https://www.nuget.org/packages/Smdn.TPSmartHomeDevices.Primitives/) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

[Smdn.TPSmartHomeDevices.Primitives](./src/Smdn.TPSmartHomeDevices.Primitives/) provides common types for `Smdn.TPSmartHomeDevices.*`. This library includes abstraction interfaces, extension methods and custom `JsonConverter`s. This library does not provide any specific implementations to operate Kasa and Tapo devices.

More description to be added.

# For contributors
Contributions are appreciated!

If there's a feature you would like to add or a bug you would like to fix, please read [Contribution guidelines](./CONTRIBUTING.md) and create an Issue or Pull Request.

IssueやPull Requestを送る際は、[Contribution guidelines](./CONTRIBUTING.md)をご覧頂ください。　可能なら英語が望ましいですが、日本語で構いません。

# Notice
<!-- #pragma section-start NupkgReadmeFile_Notice -->
## License
This project is licensed under the [GNU GPL version 3 or later](./COPYING.txt).

This project includes source code licensed under MIT or GPLv3 as described below and produces artifacts licensed in accordance therewith.

### Smdn.TPSmartHomeDevices.Tapo (GPLv3)
The some source files in the directory under the [src/Smdn.TPSmartHomeDevices.Tapo/](./src/Smdn.TPSmartHomeDevices.Tapo/) include codes that has been ported from codes which licensed under the GPLv3, and are licensed under the [GNU GPL version 3 or later](./src/Smdn.TPSmartHomeDevices.Tapo/COPYING.txt).

Therefore, artifacts from this directory, including NuGet package [Smdn.TPSmartHomeDevices.Tapo](https://www.nuget.org/packages/Smdn.TPSmartHomeDevices.Tapo/), are released under the [GNU GPL version 3 or later](./src/Smdn.TPSmartHomeDevices.Tapo/COPYING.txt).

For the license of individual files, refer to `SPDX-License-Identifier` at the top of each file header.

### Smdn.TPSmartHomeDevices.Kasa, Smdn.TPSmartHomeDevices.MacAddressEndPoint, Smdn.TPSmartHomeDevices.Primitives (MIT)
The source files and generated artifacts from the directory under the [src/](./src/), excluding Smdn.TPSmartHomeDevices.Tapo, are licensed and released under the terms of the [MIT License](./src/LICENSE.txt).

## Disclaimer
(An English translation for the reference follows the text written in Japanese.)

本プロジェクトは、TP-Linkとは無関係の非公式なものです。

This is an unofficial project that has no affiliation with TP-Link.

本プロジェクトが提供するソフトウェアは、デバイスの設定の取得・変更等、製品仕様の範囲内での操作のみを行うものであり、ファームウェアの改変・修正および製品の改造や製品仕様の変更を引き起こさないものの、**製品使用上の許諾事項に抵触する可能性は否定できないため、使用の際はその点にご留意ください。**

The software provided by this project is intended only for operations within the scope of the product specifications, such as acquiring and changing device settings, and while it does not cause altering the product itself, product's firmware or operating specifications. **Nevertheless, please note that the possibility of violating terms of use of the product cannot be dismissed when using the software provided by this project.**

[Tapo](https://www.tapo.com/)、[Kasa](https://www.kasasmart.com/)、および各製品名の著作権は[TP-Link](https://www.tp-link.com/)に帰属します。

[Tapo](https://www.tapo.com/), [Kasa](https://www.kasasmart.com/) and all respective product names are copyright of [TP-Link](https://www.tp-link.com/).

## Credits
This project incorporates implementations partially ported from the following projects. See also [ThirdPartyNotices.md](./ThirdPartyNotices.md) for detail.

- [fishbigger/TapoP100](https://github.com/fishbigger/TapoP100)
- [europowergenerators/Tapo-plug-controller](https://github.com/europowergenerators/Tapo-plug-controller)
- [petretiandrea/plugp100](https://github.com/petretiandrea/plugp100)
- [plasticrake/tplink-smarthome-api](https://github.com/plasticrake/tplink-smarthome-api/)
- [plasticrake/tplink-smarthome-crypto](https://github.com/plasticrake/tplink-smarthome-crypto)

<!-- #pragma section-end NupkgReadmeFile_Notice -->
