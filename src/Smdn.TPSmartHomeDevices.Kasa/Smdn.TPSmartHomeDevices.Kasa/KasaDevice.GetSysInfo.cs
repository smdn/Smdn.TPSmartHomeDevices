// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
// cSpell:ignore sysinfo
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices.Kasa;

partial class KasaDevice {
  protected static readonly JsonEncodedText ModuleTextSystem = JsonEncodedText.Encode(
    "system"u8
  );
  protected static readonly JsonEncodedText MethodTextGetSysInfo = JsonEncodedText.Encode(
    "get_sysinfo"u8
  );

  /// <summary>
  /// Gets the system info of the device.
  /// </summary>
  /// <param name="composeResult">
  /// The <see cref="Func{JsonElement, TSysInfo}"/> delegate that composes or converts the JSON returned
  /// as a result of the 'get_sysinfo' method into the type <typeparamref name="TSysInfo"/>.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  protected ValueTask<TSysInfo> GetSysInfoAsync<TSysInfo>(
    Func<JsonElement, TSysInfo> composeResult,
    CancellationToken cancellationToken = default
  )
    => SendRequestAsync(
      module: ModuleTextSystem,
      method: MethodTextGetSysInfo,
      composeResult: composeResult,
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Gets the Kasa device information.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken" /> to monitor for cancellation requests.
  /// The default value is <see langword="default" />.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{KasaDeviceInfo}"/> representing the result of method.
  /// </returns>
  /// <seealso cref="KasaDeviceInfo"/>
  public ValueTask<KasaDeviceInfo?> GetDeviceInfoAsync(
    CancellationToken cancellationToken = default
  )
    => GetSysInfoAsync(
      composeResult: static result => JsonSerializer.Deserialize(
        element: result,
        jsonTypeInfo: KasaDeviceJsonSerializerContext.Default.KasaDeviceInfo
      ),
      cancellationToken: cancellationToken
    );
}
