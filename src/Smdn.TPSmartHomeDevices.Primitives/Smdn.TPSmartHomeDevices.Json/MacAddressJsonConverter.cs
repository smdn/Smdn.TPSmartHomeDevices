// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Json;

public sealed class MacAddressJsonConverter : JsonConverter<PhysicalAddress> {
  public override PhysicalAddress? Read(
    ref Utf8JsonReader reader,
    Type typeToConvert,
    JsonSerializerOptions options
  )
  {
    var str = reader.TokenType switch {
      JsonTokenType.Null => null,
      JsonTokenType.String => reader.GetString(),
      _ => null,
    };

    if (str is null)
      return null;

#if SYSTEM_NET_NETWORKINFORMATION_PHYSICALADDRESS_TRYPARSE
    return PhysicalAddress.TryParse(str, out var ret)
      ? ret
      : null;
#else
    try {
      return PhysicalAddress.Parse(str);
    }
    catch (FormatException) {
      return null;
    }
#endif
  }

  public override void Write(
    Utf8JsonWriter writer,
    PhysicalAddress value,
    JsonSerializerOptions options
  )
    => throw new NotImplementedException();
}
