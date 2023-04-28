// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// Provides a JsonConverter implementation to (de)serialize the 'request' and 'response' properties of the
/// 'securePassthrough' method, according to the crypto method used in Tapo's communication protocol.
/// </summary>
/// <remarks>
/// This implementation is based on and ported from the following
/// Python implementation by <see href="https://github.com/fishbigger">Toby Johnson</see>:
/// <see href="https://github.com/fishbigger/TapoP100">fishbigger/TapoP100</see>, published under the MIT License.
/// </remarks>
/// <seealso cref="SecurePassThroughInvalidPaddingException"/>
public sealed class SecurePassThroughJsonConverterFactory :
  JsonConverterFactory,
  IDisposable,
  SecurePassThroughJsonConverterFactory.IPassThroughObjectJsonConverter
{
  private interface IPassThroughObjectJsonConverter {
    void WriteEncryptedValue<TValue>(
      Utf8JsonWriter writer,
      TValue value
    );
    TValue ReadDecryptedValue<TValue>(
      ref Utf8JsonReader reader,
      Type typeToConvert
    );
  }

  private ICryptoTransform? encryptorForPassThroughRequest;
  private ICryptoTransform? decryptorForPassThroughResponse;

  // used for avoiding JsonSerializer to (de)serialize recursively
  private readonly JsonSerializerOptions jsonSerializerOptionsForPassThroughMessage;
  private readonly JsonSerializerOptions? jsonSerializerOptionsForPassThroughMessageLogger;

  private readonly ILogger? logger;
  private bool disposed;

  public SecurePassThroughJsonConverterFactory(
    ITapoCredentialIdentity? identity,
    ICryptoTransform? encryptorForPassThroughRequest,
    ICryptoTransform? decryptorForPassThroughResponse,
    JsonSerializerOptions? baseJsonSerializerOptionsForPassThroughMessage,
    ILogger? logger = null
  )
  {
    this.encryptorForPassThroughRequest = encryptorForPassThroughRequest;
    this.decryptorForPassThroughResponse = decryptorForPassThroughResponse;
    this.logger = logger;

    jsonSerializerOptionsForPassThroughMessage = baseJsonSerializerOptionsForPassThroughMessage is null
      ? new()
      : new(baseJsonSerializerOptionsForPassThroughMessage);
    jsonSerializerOptionsForPassThroughMessage.Converters.Add(
      new LoginDeviceRequest.TapoCredentialJsonConverter(identity: identity)
    );

    if (logger is not null) {
      jsonSerializerOptionsForPassThroughMessageLogger = baseJsonSerializerOptionsForPassThroughMessage is null
        ? new()
        : new(baseJsonSerializerOptionsForPassThroughMessage);
      jsonSerializerOptionsForPassThroughMessageLogger.Converters.Add(
        LoginDeviceRequest.TapoCredentialMaskingJsonConverter.Instance // mask credentials for logger output
      );
    }
  }

  public void Dispose()
  {
    encryptorForPassThroughRequest?.Dispose();
    encryptorForPassThroughRequest = null;

    decryptorForPassThroughResponse?.Dispose();
    decryptorForPassThroughResponse = null;

    disposed = true;
  }

  private Stream CreateEncryptingStream(Stream stream)
    => disposed
      ? throw new ObjectDisposedException(GetType().FullName)
      : new CryptoStream(
        stream: stream ?? throw new ArgumentNullException(nameof(stream)),
        transform: encryptorForPassThroughRequest ?? throw new NotSupportedException("encryption not supported"),
        mode: CryptoStreamMode.Write,
        leaveOpen: true
      );

  private Stream CreateDecryptingStream(Stream stream)
    => disposed
      ? throw new ObjectDisposedException(GetType().FullName)
      : new CryptoStream(
        stream: stream ?? throw new ArgumentNullException(nameof(stream)),
        transform: decryptorForPassThroughResponse ?? throw new NotSupportedException("decryption not supported"),
        mode: CryptoStreamMode.Read,
        leaveOpen: true
      );

  public override bool CanConvert(Type typeToConvert)
    =>
      typeof(ITapoPassThroughRequest).IsAssignableFrom(typeToConvert) ||
      typeof(ITapoPassThroughResponse).IsAssignableFrom(typeToConvert);

  public override JsonConverter? CreateConverter(
    Type typeToConvert,
    JsonSerializerOptions options
  )
    => CanConvert(typeToConvert)
      ? (JsonConverter)Activator.CreateInstance(
          type: typeof(PassThroughObjectJsonConverter<>).MakeGenericType(typeToConvert),
          bindingAttr: BindingFlags.Instance | BindingFlags.Public,
          binder: null,
          args: new object[] { this },
          culture: null
        )!
      // should throw exception?
      //   throw new NotSupportedException($"unexpected type to convert: {typeToConvert.FullName}");
      : null;

  void IPassThroughObjectJsonConverter.WriteEncryptedValue<TValue>(
    Utf8JsonWriter writer,
    TValue value
  )
  {
    logger?.LogTrace(
      "PassThroughRequest: {RawJson} ({TypeFullName})",
      JsonSerializer.Serialize(value: value, options: jsonSerializerOptionsForPassThroughMessageLogger),
      typeof(TValue).FullName
    );

    var stream = new MemoryStream(capacity: 256); // TODO: IMemoryAllocator

    using var encryptingStream = CreateEncryptingStream(stream);

    JsonSerializer.Serialize(
      utf8Json: encryptingStream,
      value: value,
      options: jsonSerializerOptionsForPassThroughMessage
    );

    encryptingStream.Close();

    if (!stream.TryGetBuffer(out var buffer))
      throw new InvalidOperationException("cannot get buffer from MemoryStream");

    writer.WriteBase64StringValue(buffer.AsSpan());
  }

  TValue IPassThroughObjectJsonConverter.ReadDecryptedValue<TValue>(
    ref Utf8JsonReader reader,
    Type typeToConvert
  )
  {
    if (!reader.TryGetBytesFromBase64(out var base64))
      throw new JsonException("could not read base64 string");

    var cryptographicExceptionThrown = false;

    try {
      using var stream = new MemoryStream(base64, writable: false);
      using var decryptingStream = CreateDecryptingStream(stream);

      try {
        var ret = JsonSerializer.Deserialize(
          utf8Json: decryptingStream,
          returnType: typeToConvert,
          options: jsonSerializerOptionsForPassThroughMessage
        );

        return ret is null
          ? throw new InvalidOperationException($"Object {typeToConvert.FullName} was decrypted as null.")
          : (TValue)ret;
      }
      catch (CryptographicException ex) when (IsInvalidPaddingException(ex)) {
        cryptographicExceptionThrown = true;
        throw new SecurePassThroughInvalidPaddingException("Invalid padding detected in encrypted JSON", ex);
      }
      catch (CryptographicException) {
        cryptographicExceptionThrown = true;
        throw;
      }

      static bool IsInvalidPaddingException(CryptographicException ex)
      {
        // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Security.Cryptography/src/Resources/Strings.resx
        const string SR_Cryptography_InvalidPadding = "Padding is invalid and cannot be removed.";

        return SR_Cryptography_InvalidPadding.AsSpan().SequenceEqual(ex.Message); // TODO: consider localized message strings
      }
    }
    finally {
      if (!cryptographicExceptionThrown && logger is not null) {
        using var stream = new MemoryStream(base64, writable: false);
        using var decryptingStream = CreateDecryptingStream(stream);

        logger.LogTrace(
          "PassThroughResponse: {RawJson} ({TypeFullName})",
          new StreamReader(decryptingStream, Encoding.UTF8).ReadToEnd(),
          typeof(TValue).FullName
        );
      }
    }
  }

  private sealed class PassThroughObjectJsonConverter<TPassThroughObject>
    : JsonConverter<TPassThroughObject>
    // where TPassThroughObject : ITapoPassThroughRequest or ITapoPassThroughResponse
  {
    private readonly IPassThroughObjectJsonConverter converter;

    public PassThroughObjectJsonConverter(IPassThroughObjectJsonConverter converter)
    {
      this.converter = converter;
    }

    public override void Write(
      Utf8JsonWriter writer,
      TPassThroughObject value,
      JsonSerializerOptions options
    )
      => converter.WriteEncryptedValue(writer, value);

    public override TPassThroughObject? Read(
      ref Utf8JsonReader reader,
      Type typeToConvert,
      JsonSerializerOptions options
    )
      => converter.ReadDecryptedValue<TPassThroughObject>(ref reader, typeToConvert);
  }
}
