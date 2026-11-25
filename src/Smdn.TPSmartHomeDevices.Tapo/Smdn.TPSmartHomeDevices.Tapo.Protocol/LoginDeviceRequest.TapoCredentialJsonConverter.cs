// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

partial struct LoginDeviceRequest {
  private static readonly JsonEncodedText PropertyNamePassword = JsonEncodedText.Encode(
    "password"u8
  );
  private static readonly JsonEncodedText PropertyNameUsername = JsonEncodedText.Encode(
    "username"u8
  );

  /// <seealso cref="LoginDevicePlainTextJsonSerializerContext"/>
  internal sealed class TapoCredentialJsonConverter : JsonConverter<ITapoCredentialProvider> {
    private readonly ITapoCredentialIdentity? identity;

    internal TapoCredentialJsonConverter(ITapoCredentialIdentity? identity)
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
      using var credential = value.GetCredential(identity)
        ?? throw TapoCredentials.CreateExceptionNoCredentialForIdentity(identity);

      writer.WriteStartObject();

      writer.WritePropertyName(PropertyNamePassword);

      credential.WritePasswordPropertyValue(writer);

      writer.WritePropertyName(PropertyNameUsername);

      credential.WriteUsernamePropertyValue(writer);

      writer.WriteEndObject();
    }
  }
}
