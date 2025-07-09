// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
// cSpell:ignore sysinfo

using System;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading.Tasks;

using NUnit.Framework;

using Smdn.TPSmartHomeDevices.Kasa.Protocol;

namespace Smdn.TPSmartHomeDevices.Kasa;

public partial class KasaDeviceTests {
  private static System.Collections.IEnumerable YieldTestCases_GetDeviceInfoAsync()
  {
    // Kasa KL130(JP)
    yield return new object?[] {
      /*lang=json,strict*/
@"{
  ""system"":{
    ""get_sysinfo"":{
      ""sw_ver"":""<sw_ver>"",
      ""hw_ver"":""<hw_ver>"",
      ""model"":""<model>"",
      ""description"":""<description>"",
      ""mic_type"":""<mic_type>"",
      ""mic_mac"":""00005E005300"",
      ""deviceId"":""0123456789ABCDEF0123456789ABCDEF01234567"",
      ""oemId"":""ABCDEF0123456789ABCDEF0123456789"",
      ""hwId"":""0123456789ABCDEF0123456789ABCDEF"",
      ""rssi"":-30,
      ""err_code"":0
    }
  }
}",
      new Action<KasaDeviceInfo>(static info => {
        Assert.That(info.Id, Is.EqualTo(new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67 }).AsCollection, nameof(info.Id));
        Assert.That(info.Description, Is.EqualTo("<description>"), nameof(info.TypeName));
        Assert.That(info.TypeName, Is.EqualTo("<mic_type>"), nameof(info.TypeName));
        Assert.That(info.ModelName, Is.EqualTo("<model>"), nameof(info.ModelName));
        Assert.That(info.FirmwareId, Is.Null, nameof(info.FirmwareId));
        Assert.That(info.FirmwareVersion, Is.EqualTo("<sw_ver>"), nameof(info.FirmwareVersion));
        Assert.That(info.HardwareId, Is.EqualTo(new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF }).AsCollection, nameof(info.HardwareId));
        Assert.That(info.HardwareVersion, Is.EqualTo("<hw_ver>"), nameof(info.HardwareVersion));
        Assert.That(info.OemId, Is.EqualTo(new byte[] { 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89 }).AsCollection, nameof(info.OemId));
        Assert.That(info.MacAddress, Is.EqualTo(PhysicalAddress.Parse("00:00:5E:00:53:00")), nameof(info.MacAddress));
        Assert.That(info.NetworkRssi, Is.EqualTo(-30m), nameof(info.NetworkRssi));
      }),
    };

    // Kasa HS105(JP)
    yield return new object?[] {
      /*lang=json,strict*/
@"{
  ""system"":{
    ""get_sysinfo"":{
      ""sw_ver"":""<sw_ver>"",
      ""hw_ver"":""<hw_ver>"",
      ""type"":""<type>"",
      ""model"":""<model>"",
      ""mac"":""00:00:5E:00:53:00"",
      ""dev_name"":""<dev_name>"",
      ""rssi"":-30,
      ""hwId"":""0123456789ABCDEF0123456789ABCDEF"",
      ""fwId"":""6789ABCDEF0123456789ABCDEF012345"",
      ""deviceId"":""0123456789ABCDEF0123456789ABCDEF01234567"",
      ""oemId"":""ABCDEF0123456789ABCDEF0123456789"",
      ""err_code"":0
    }
  }
}",
      new Action<KasaDeviceInfo>(static info => {
        Assert.That(info.Id, Is.EqualTo(new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67 }).AsCollection, nameof(info.Id));
        Assert.That(info.Description, Is.EqualTo("<dev_name>"), nameof(info.TypeName));
        Assert.That(info.TypeName, Is.EqualTo("<type>"), nameof(info.TypeName));
        Assert.That(info.ModelName, Is.EqualTo("<model>"), nameof(info.ModelName));
        Assert.That(info.FirmwareId, Is.EqualTo(new byte[] { 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45 }).AsCollection, nameof(info.FirmwareId));
        Assert.That(info.FirmwareVersion, Is.EqualTo("<sw_ver>"), nameof(info.FirmwareVersion));
        Assert.That(info.HardwareId, Is.EqualTo(new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF }).AsCollection, nameof(info.HardwareId));
        Assert.That(info.HardwareVersion, Is.EqualTo("<hw_ver>"), nameof(info.HardwareVersion));
        Assert.That(info.OemId, Is.EqualTo(new byte[] { 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89 }).AsCollection, nameof(info.OemId));
        Assert.That(info.MacAddress, Is.EqualTo(PhysicalAddress.Parse("00:00:5E:00:53:00")), nameof(info.MacAddress));
        Assert.That(info.NetworkRssi, Is.EqualTo(-30m), nameof(info.NetworkRssi));
      }),
    };
  }

  [TestCaseSource(nameof(YieldTestCases_GetDeviceInfoAsync))]
  public async Task GetDeviceInfoAsync(string responseJson, Action<KasaDeviceInfo> assertDeviceInfo)
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, request) => {
        Assert.That(
          JsonSerializer.Serialize(request),
          Is.EqualTo(/*lang=json,strict*/ @"{""system"":{""get_sysinfo"":{}}}"),
          nameof(request)
        );

        return JsonDocument.Parse(responseJson);
      }
    };

    pseudoDevice.Start();

    using var device = new KL130(
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    KasaDeviceInfo? info = null;

    Assert.DoesNotThrowAsync(async () => info = await device.GetDeviceInfoAsync());

    Assert.That(info, Is.Not.Null);

    assertDeviceInfo(info);
  }
}
