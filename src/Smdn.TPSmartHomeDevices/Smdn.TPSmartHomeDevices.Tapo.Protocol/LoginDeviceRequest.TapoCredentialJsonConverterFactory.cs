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
  internal sealed class TapoCredentialJsonConverterFactory : JsonConverterFactory {
    public static readonly JsonConverter Mask = new TapoCredentialMaskingJsonConverter();

    private protected static readonly JsonEncodedText PropertyNamePassword = JsonEncodedText.Encode(
#if LANG_VERSION_11_OR_GREATER
      "password"u8
#else
      "password"
#endif
    );
    private protected static readonly JsonEncodedText PropertyNameUsername = JsonEncodedText.Encode(
#if LANG_VERSION_11_OR_GREATER
      "username"u8
#else
      "username"
#endif
    );

    /*
     * insteance members
     */
    private readonly ITapoCredentialIdentity? identity;

    internal TapoCredentialJsonConverterFactory(ITapoCredentialIdentity? identity)
    {
      this.identity = identity;
    }

    public override bool CanConvert(Type typeToConvert)
      => typeof(ITapoCredentialProvider).IsAssignableFrom(typeToConvert);

    public override JsonConverter? CreateConverter(
      Type typeToConvert,
      JsonSerializerOptions options
    )
      => CanConvert(typeToConvert)
        ? new TapoCredentialJsonConverter(identity)
        : null;

    private sealed class TapoCredentialJsonConverter : JsonConverter<ITapoCredentialProvider> {
      private readonly ITapoCredentialIdentity? identity;

      public TapoCredentialJsonConverter(ITapoCredentialIdentity? identity)
      {
        this.identity = identity;
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
        using var credential = value.GetCredential(identity);

        if (credential is null)
          throw new InvalidOperationException($"Could not get a credential for an identity '{identity?.Name ?? "(null)"}'");

        writer.WriteStartObject();

        writer.WritePropertyName(PropertyNamePassword);

        credential.WritePasswordPropertyValue(writer);

        writer.WritePropertyName(PropertyNameUsername);

        credential.WriteUsernamePropertyValue(writer);

        writer.WriteEndObject();
      }
    }

    private sealed class TapoCredentialMaskingJsonConverter : JsonConverter<ITapoCredentialProvider> {
      private static readonly JsonEncodedText PropertyValueMaskedCredential = JsonEncodedText.Encode(
#if LANG_VERSION_11_OR_GREATER
        "****"u8
#else
        "****"
#endif
      );

      public TapoCredentialMaskingJsonConverter()
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
}
