// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// Provides a implementation to (de)serialize the 'request' and 'response' properties of the
/// 'securePassthrough' method, according to the crypto method used in Tapo's communication protocol.
/// </summary>
/// <remarks>
/// <para>
/// This implementation is based on and ported from the following
/// Python implementation by <see href="https://github.com/fishbigger">Toby Johnson</see>:
/// <see href="https://github.com/fishbigger/TapoP100">fishbigger/TapoP100</see>, published under the MIT License.
/// </para>
/// </remarks>
/// <seealso cref="SecurePassThroughInvalidPaddingException"/>
/// <seealso cref="SecurePassThroughJsonConverterFactory"/>
internal sealed class PassThroughMessageTransform : IDisposable {
  private ICryptoTransform? encryptorForPassThroughRequest;
  private ICryptoTransform? decryptorForPassThroughResponse;

  // used for avoiding JsonSerializer to (de)serialize recursively
  private readonly JsonSerializerOptions jsonSerializerOptionsForPassThroughMessage;
  private readonly JsonSerializerOptions? jsonSerializerOptionsForPassThroughMessageLogger;

  private readonly ILogger? logger;
  private bool disposed;

  public PassThroughMessageTransform(
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

  private CryptoStream CreateEncryptingStream(Stream stream)
    => disposed
      ? throw new ObjectDisposedException(GetType().FullName)
      : new CryptoStream(
          stream: stream ?? throw new ArgumentNullException(nameof(stream)),
          transform: encryptorForPassThroughRequest ?? throw new NotSupportedException("encryption not supported"),
          mode: CryptoStreamMode.Write,
          leaveOpen: true
        );

  private CryptoStream CreateDecryptingStream(Stream stream)
    => disposed
      ? throw new ObjectDisposedException(GetType().FullName)
      : new CryptoStream(
          stream: stream ?? throw new ArgumentNullException(nameof(stream)),
          transform: decryptorForPassThroughResponse ?? throw new NotSupportedException("decryption not supported"),
          mode: CryptoStreamMode.Read,
          leaveOpen: true
        );

  public void WriteEncryptedValue<TValue>(
    Utf8JsonWriter writer,
    TValue value
  )
  {
#pragma warning disable CA1873
    logger?.LogTrace(
      "PassThroughRequest: {RawJson} ({TypeFullName})",
      JsonSerializer.Serialize(value: value, options: jsonSerializerOptionsForPassThroughMessageLogger),
      typeof(TValue).FullName
    );
#pragma warning restore CA1873

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

  public TValue ReadDecryptedValue<TValue>(
    Stream encryptedValueStream,
    Type typeToConvert
  ) where TValue : notnull
  {
    var cryptographicExceptionThrown = false;

#if DEBUG
    if (!encryptedValueStream.CanSeek)
      throw new InvalidOperationException("This method expects a seekable stream as an argument.");
#endif

    try {
      using var decryptingStream = CreateDecryptingStream(encryptedValueStream);

      try {
        return (TValue)(
          JsonSerializer.Deserialize(
            utf8Json: decryptingStream,
            returnType: typeToConvert ?? throw new ArgumentNullException(nameof(typeToConvert)),
            options: jsonSerializerOptionsForPassThroughMessage
          ) ?? throw new InvalidOperationException($"Object {typeToConvert.FullName} was decrypted as null.")
        );
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
        encryptedValueStream.Position = 0L;

        using var decryptingStream = CreateDecryptingStream(encryptedValueStream);
        using var decryptingReader = new StreamReader(decryptingStream, Encoding.UTF8);

#pragma warning disable CA1873
        logger.LogTrace(
          "PassThroughResponse: {RawJson} ({TypeFullName})",
          decryptingReader.ReadToEnd(),
          typeof(TValue).FullName
        );
#pragma warning restore CA1873
      }
    }
  }
}
