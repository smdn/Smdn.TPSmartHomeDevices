// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Json;

internal sealed class TapoIPAddressJsonConverter : JsonConverter<IPAddress> {
  public override IPAddress? Read(
    ref Utf8JsonReader reader,
    Type typeToConvert,
    JsonSerializerOptions options
  )
  {
    var str = reader.GetString();

    if (str is null)
      return null;

    return IPAddress.TryParse(str, out var ret)
      ? ret
      : null;
  }

  public override void Write(
    Utf8JsonWriter writer,
    IPAddress value,
    JsonSerializerOptions options
  )
    => throw new NotImplementedException();
}
