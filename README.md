[![GitHub license](https://img.shields.io/github/license/smdn/Smdn.TPSmartHomeDevices)](https://github.com/smdn/Smdn.TPSmartHomeDevices/blob/main/LICENSE.txt)
[![tests/main](https://img.shields.io/github/actions/workflow/status/smdn/Smdn.TPSmartHomeDevices/test.yml?branch=main&label=tests%2Fmain)](https://github.com/smdn/Smdn.TPSmartHomeDevices/actions/workflows/test.yml)
[![CodeQL](https://github.com/smdn/Smdn.TPSmartHomeDevices/actions/workflows/codeql-analysis.yml/badge.svg?branch=main)](https://github.com/smdn/Smdn.TPSmartHomeDevices/actions/workflows/codeql-analysis.yml)
[![NuGet](https://img.shields.io/nuget/v/Smdn.TPSmartHomeDevices.svg)](https://www.nuget.org/packages/Smdn.TPSmartHomeDevices/)

# Smdn.TPSmartHomeDevices
The .NET implementations for [Kasa](https://www.kasasmart.com) and [Tapo](https://www.tapo.com/), the TP-Link smart home devices.

- Supported devices
  - Kasa
    - HS105 smart plug
    - KL130 multicolor bulb
  - Tapo
    - L530 multicolor bulb
    - L900 multicolor light strip
    - P105 smart plug
- Supported device features
  - Turn on/off
  - Set color (color temperature)
  - Set color (hue and saturation)
  - Set brightness
  - Get device informations (Tapo)
  - Get light color/brightness
  - Get on/off stete
- Other features
  - Finding the device by MAC address, for DHCP networks
  - Resilience from errors
    - Automatic session management, re-authentication and reconnection
    - Supports recovery processing for major error codes
      - Customizable error handling
    - Support for endpoint changes in dynamic IP networks
  - Supports dependency injection (`Microsoft.Extensions.DependencyInjection`)
    - Logging (`Microsoft.Extensions.Logging`)
    - HTTP (`Microsoft.Extensions.Http`)
  - Tested with pseudo devices and actual devices
  - Confirmed to work on Windows, Ubuntu, Raspberry Pi 3 Model B+

# Notice
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
