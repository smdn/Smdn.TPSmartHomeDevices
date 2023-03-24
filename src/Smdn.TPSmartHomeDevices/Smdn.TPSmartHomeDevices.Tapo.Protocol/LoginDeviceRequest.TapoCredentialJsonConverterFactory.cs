// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

#pragma warning disable IDE0040
partial struct LoginDeviceRequest {
#pragma warning restore IDE0040
  public static JsonConverter CreateJsonConverter(string host)
    => new TapoCredentialJsonConverterFactory(host ?? throw new ArgumentNullException(nameof(host)));

  private sealed class TapoCredentialJsonConverterFactory : JsonConverterFactory {
    private readonly string host;

    internal TapoCredentialJsonConverterFactory(string host)
    {
      this.host = host;
    }

    public override bool CanConvert(Type typeToConvert)
      => typeof(ITapoCredentialProvider).IsAssignableFrom(typeToConvert);

    public override JsonConverter? CreateConverter(
      Type typeToConvert,
      JsonSerializerOptions options
    )
      => CanConvert(typeToConvert)
        ? new TapoCredentialJsonConverter(host)
        : null;

    private sealed class TapoCredentialJsonConverter : JsonConverter<ITapoCredentialProvider> {
      private static readonly JsonEncodedText PropertyNamePassword = JsonEncodedText.Encode(
#if LANG_VERSION_11_OR_GREATER
        "password"u8
#else
        "password"
#endif
      );
      private static readonly JsonEncodedText PropertyNameUsername = JsonEncodedText.Encode(
#if LANG_VERSION_11_OR_GREATER
        "username"u8
#else
        "username"
#endif
      );

      private readonly string host;

      public TapoCredentialJsonConverter(string host)
      {
        this.host = host;
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
        using var credential = value.GetCredential(host);

        writer.WriteStartObject();

        writer.WritePropertyName(PropertyNamePassword);

        credential.WritePasswordPropertyValue(writer);

        writer.WritePropertyName(PropertyNameUsername);

        credential.WriteUsernamePropertyValue(writer);

        writer.WriteEndObject();
      }
    }
  }
}
