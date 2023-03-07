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
- Other features
  - Finding the device by MAC address, for DHCP networks
  - Supports dependency injection (Microsoft.Extensions.DependencyInjection)
    - Logging (Microsoft.Extensions.Logging)
    - HTTP (Microsoft.Extensions.Http)
