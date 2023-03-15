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

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <remarks>
/// This implementation is based on and ported from the following implementation: <see href="https://github.com/fishbigger/TapoP100">fishbigger/TapoP100</see>.
/// </remarks>
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
  private readonly JsonSerializerOptions? plainTextJsonSerializerOptions;
  private readonly ILogger? logger;
  private bool disposed;

  public SecurePassThroughJsonConverterFactory(
    ICryptoTransform? encryptorForPassThroughRequest,
    ICryptoTransform? decryptorForPassThroughResponse,
    JsonSerializerOptions? plainTextJsonSerializerOptions,
    ILogger? logger = null
  )
  {
    this.encryptorForPassThroughRequest = encryptorForPassThroughRequest;
    this.decryptorForPassThroughResponse = decryptorForPassThroughResponse;
    this.plainTextJsonSerializerOptions = plainTextJsonSerializerOptions; // used for avoiding JsonSerializer to (de)serialize recursively
    this.logger = logger;
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
      JsonSerializer.Serialize(value: value, options: plainTextJsonSerializerOptions),
      typeof(TValue).FullName
    );

    var stream = new MemoryStream(capacity: 256); // TODO: IMemoryAllocator

    using var encryptingStream = CreateEncryptingStream(stream);

    JsonSerializer.Serialize(
      utf8Json: encryptingStream,
      value: value,
      options: plainTextJsonSerializerOptions
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

    try {
      using var stream = new MemoryStream(base64, writable: false);
      using var decryptingStream = CreateDecryptingStream(stream);

      try {
        return (TValue?)JsonSerializer.Deserialize(
          utf8Json: decryptingStream,
          returnType: typeToConvert,
          options: plainTextJsonSerializerOptions
        );
      }
      catch (CryptographicException ex) when (IsInvalidPaddingException(ex)) {
        throw new SecurePassThroughInvalidPaddingException("Invalid padding detected in encrypted JSON", ex);
      }

      static bool IsInvalidPaddingException(CryptographicException ex)
      {
        // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Security.Cryptography/src/Resources/Strings.resx
        const string SR_Cryptography_InvalidPadding = "Padding is invalid and cannot be removed.";

        return SR_Cryptography_InvalidPadding.AsSpan().SequenceEqual(ex.Message); // TODO: consider localized message strings
      }
    }
    finally {
      if (logger is not null) {
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

  private class PassThroughObjectJsonConverter<TPassThroughObject>
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
