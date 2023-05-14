// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

#pragma warning disable IDE0040
partial struct LoginDeviceRequest {
#pragma warning restore IDE0040
  internal sealed class TapoCredentialMaskingJsonConverter : JsonConverter<ITapoCredentialProvider> {
#pragma warning disable SA1114
    private static readonly JsonEncodedText PropertyValueMaskedCredential = JsonEncodedText.Encode(
#if LANG_VERSION_11_OR_GREATER
      "****"u8
#else
      "****"
#endif
    );
#pragma warning restore SA1114

    public static readonly JsonConverter<ITapoCredentialProvider> Instance = new TapoCredentialMaskingJsonConverter();

    private TapoCredentialMaskingJsonConverter()
    {
    }

    public override ITapoCredentialProvider? Read(
      ref Utf8JsonReader reader,
      Type typeToConvert,
      JsonSerializerOptions options
    )
      => throw new NotSupportedException();

    public override void Write(
      Utf8JsonWriter writer,
      ITapoCredentialProvider value,
      JsonSerializerOptions options
    )
    {
      writer.WriteStartObject();
      writer.WriteString(PropertyNamePassword, PropertyValueMaskedCredential);
      writer.WriteString(PropertyNameUsername, PropertyValueMaskedCredential);
      writer.WriteEndObject();
    }
  }
}
