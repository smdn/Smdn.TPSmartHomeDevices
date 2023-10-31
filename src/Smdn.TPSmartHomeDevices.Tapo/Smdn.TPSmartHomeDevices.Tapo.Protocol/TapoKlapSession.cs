// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: GPL-3.0-or-later
using System;
using System.Buffers;
using System.Text.Json;

using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// A C# implementation of session information for Tapo's KLAP protocol.
/// </summary>
/// <remarks>
/// This implementation is based on and ported from the following
/// Python implementation by <see href="https://github.com/petretiandrea">petretiandrea</see>:
/// <see href="https://github.com/petretiandrea/plugp100">petretiandrea/plugp100</see>, published under the GPL-3.0 license,
/// forked from <see href="https://github.com/K4CZP3R/tapo-p100-python">K4CZP3R/tapo-p100-python</see>.
/// </remarks>
internal sealed class TapoKlapSession : TapoSession {
  private const string RequestPath = "/app/request";

  public override string? Token => null;

  private KlapEncryptionAlgorithm klap;

  private readonly ArrayBufferWriter<byte> httpRequestContentBuffer = new(1024);
  private readonly ArrayBufferWriter<byte> jsonWriterBuffer = new(1024);
  private readonly ArrayBufferWriter<byte> decryptionBuffer = new(256);

  internal TapoKlapSession(
    string? sessionId,
    DateTime expiresOn,
    ReadOnlySpan<byte> localSeed,
    ReadOnlySpan<byte> remoteSeed,
    ReadOnlySpan<byte> userHash,
    ILogger? logger
  )
    : base(
      sessionId: sessionId,
      expiresOn: expiresOn
    )
  {
    klap = new(
      localSeed,
      remoteSeed,
      userHash
    );

    logger?.LogTrace(
      "[KLAP] LocalSeed: {LocalSeed}",
#if SYSTEM_CONVERT_TOHEXSTRING
      Convert.ToHexString(localSeed)
#else
      Smdn.Formats.Hexadecimal.ToUpperCaseString(localSeed)
#endif
    );
    logger?.LogTrace(
      "[KLAP] RemoteSeed: {RemoteSeed}",
#if SYSTEM_CONVERT_TOHEXSTRING
      Convert.ToHexString(remoteSeed)
#else
      Smdn.Formats.Hexadecimal.ToUpperCaseString(remoteSeed)
#endif
    );
    logger?.LogTrace(
      "[KLAP] UserHash: {UserHash}",
#if SYSTEM_CONVERT_TOHEXSTRING
      Convert.ToHexString(userHash)
#else
      Smdn.Formats.Hexadecimal.ToUpperCaseString(userHash)
#endif
    );

    logger?.LogTrace(
      "[KLAP] Key: {Key}",
#if SYSTEM_CONVERT_TOHEXSTRING
      Convert.ToHexString(klap.Key)
#else
      Smdn.Formats.Hexadecimal.ToUpperCaseString(klap.Key)
#endif
    );
    logger?.LogTrace(
      "[KLAP] IV: {IV}",
#if SYSTEM_CONVERT_TOHEXSTRING
      Convert.ToHexString(klap.IV)
#else
      Smdn.Formats.Hexadecimal.ToUpperCaseString(klap.IV)
#endif
    );
    logger?.LogTrace("[KLAP] SequenceNumber: {SequenceNumber}", klap.SequenceNumber);
    logger?.LogTrace(
      "[KLAP] Signature: {Signature}",
#if SYSTEM_CONVERT_TOHEXSTRING
      Convert.ToHexString(klap.Signature)
#else
      Smdn.Formats.Hexadecimal.ToUpperCaseString(klap.Signature)
#endif
    );
  }

  protected override void Dispose(bool disposing)
  {
    if (!disposing)
      klap = null!;

    base.Dispose(disposing);
  }

  internal ArrayBufferWriter<byte> GetHttpRequestContentBuffer()
  {
    httpRequestContentBuffer.Clear();

    return httpRequestContentBuffer;
  }

  internal (Uri RequestPathAndQuery, int SequenceNumber) Encrypt<TRequest>(
    TRequest request,
    JsonSerializerOptions jsonSerializerOptions,
    IBufferWriter<byte> destination
  ) where TRequest : notnull, ITapoPassThroughRequest
  {
    ThrowIfDisposed();

    jsonWriterBuffer.Clear();

    using var rawTextWriter = new Utf8JsonWriter(jsonWriterBuffer, options: default);

    JsonSerializer.Serialize(
      rawTextWriter,
      request,
      jsonSerializerOptions
    );

    var seq = klap.Encrypt(jsonWriterBuffer.WrittenSpan, destination);

    return (
      new(
        string.Concat(
          RequestPath,
          "?seq=",
          seq
        ),
        UriKind.Relative
      ),
      seq
    );
  }

  internal TResponse? Decrypt<TResponse>(
    ReadOnlySpan<byte> encryptedText,
    int sequenceNumber,
    JsonSerializerOptions jsonSerializerOptions
  ) where TResponse : ITapoPassThroughResponse
  {
    ThrowIfDisposed();

    decryptionBuffer.Clear();

    klap.Decrypt(
      encryptedText,
      sequenceNumber,
      decryptionBuffer
    );

    return JsonSerializer.Deserialize<TResponse>(
      decryptionBuffer.WrittenSpan,
      jsonSerializerOptions
    );
  }
}
