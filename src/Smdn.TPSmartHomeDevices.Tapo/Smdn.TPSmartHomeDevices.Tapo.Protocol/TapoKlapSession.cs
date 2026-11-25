// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: GPL-3.0-or-later
#pragma warning disable CA1873

using System;
using System.Buffers;
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE || SYSTEM_DIAGNOSTICS_CODEANALYSIS_UNCONDITIONALSUPPRESSMESSAGEATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

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
  public override TapoSessionProtocol Protocol => TapoSessionProtocol.Klap;

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

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE
  [RequiresDynamicCode(FeatureRequirementMessages.RequiresDynamicCodeJsonSerializeInMethodOverload)]
  [RequiresUnreferencedCode(FeatureRequirementMessages.RequiresUnreferencedCodeJsonSerializeInMethodOverload)]
#endif
  internal (Uri RequestPathAndQuery, int SequenceNumber) Encrypt<TRequest>(
    TRequest request,
    JsonSerializerOptions? jsonSerializerOptions,
    IBufferWriter<byte> destination
  ) where TRequest : notnull, ITapoPassThroughRequest
    => EncryptCore(
      request: request,
      jsonTypeInfo: null,
      jsonSerializerOptions: jsonSerializerOptions,
      destination: destination
    );

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_UNCONDITIONALSUPPRESSMESSAGEATTRIBUTE
  [UnconditionalSuppressMessage(category: "AOT", checkId: "IL3050", Justification = UnconditionalSuppressMessages.IL3050JustificationAotCompatibleCodePathUsingJsonTypeInfo)]
  [UnconditionalSuppressMessage(category: "Trimming", checkId: "IL2026", Justification = UnconditionalSuppressMessages.IL2026JustificationReflectionFreeCodePathUsingJsonTypeInfo)]
#endif
  internal (Uri RequestPathAndQuery, int SequenceNumber) Encrypt<TRequest>(
    TRequest request,
    JsonTypeInfo<TRequest> jsonTypeInfo,
    IBufferWriter<byte> destination
  ) where TRequest : notnull, ITapoPassThroughRequest
    => EncryptCore(
      request: request,
      jsonTypeInfo: jsonTypeInfo ?? throw new ArgumentNullException(nameof(jsonTypeInfo)),
      jsonSerializerOptions: null,
      destination: destination
    );

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE
  [RequiresDynamicCode(FeatureRequirementMessages.RequiresDynamicCodeConditionalJsonSerializeInMethodOverload)]
  [RequiresUnreferencedCode(FeatureRequirementMessages.RequiresUnreferencedCodeConditionalJsonSerializeInMethodOverload)]
#endif
  private (Uri RequestPathAndQuery, int SequenceNumber) EncryptCore<TRequest>(
    TRequest request,
    JsonTypeInfo<TRequest>? jsonTypeInfo,
    JsonSerializerOptions? jsonSerializerOptions,
    IBufferWriter<byte> destination
  ) where TRequest : notnull, ITapoPassThroughRequest
  {
    ThrowIfDisposed();

    jsonWriterBuffer.Clear();

    using var rawTextWriter = new Utf8JsonWriter(jsonWriterBuffer, options: default);

    if (jsonTypeInfo is { } typeInfo) {
      JsonSerializer.Serialize(
        rawTextWriter,
        request,
        typeInfo
      );
    }
    else {
      JsonSerializer.Serialize(
        rawTextWriter,
        request,
        jsonSerializerOptions
      );
    }

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

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE
  [RequiresDynamicCode(FeatureRequirementMessages.RequiresDynamicCodeJsonSerializeInMethodOverload)]
  [RequiresUnreferencedCode(FeatureRequirementMessages.RequiresUnreferencedCodeJsonSerializeInMethodOverload)]
#endif
  internal TResponse? Decrypt<TResponse>(
    ReadOnlySpan<byte> encryptedText,
    int sequenceNumber,
    JsonSerializerOptions? jsonSerializerOptions,
    ILogger? logger
  ) where TResponse : ITapoPassThroughResponse
    => DecryptCore<TResponse>(
      encryptedText: encryptedText,
      sequenceNumber: sequenceNumber,
      jsonTypeInfo: null,
      jsonSerializerOptions: jsonSerializerOptions,
      logger: logger
    );

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_UNCONDITIONALSUPPRESSMESSAGEATTRIBUTE
  [UnconditionalSuppressMessage(category: "AOT", checkId: "IL3050", Justification = UnconditionalSuppressMessages.IL3050JustificationAotCompatibleCodePathUsingJsonTypeInfo)]
  [UnconditionalSuppressMessage(category: "Trimming", checkId: "IL2026", Justification = UnconditionalSuppressMessages.IL2026JustificationReflectionFreeCodePathUsingJsonTypeInfo)]
#endif
  internal TResponse? Decrypt<TResponse>(
    ReadOnlySpan<byte> encryptedText,
    int sequenceNumber,
    JsonTypeInfo<TResponse> jsonTypeInfo,
    ILogger? logger
  ) where TResponse : ITapoPassThroughResponse
    => DecryptCore(
      encryptedText: encryptedText,
      sequenceNumber: sequenceNumber,
      jsonTypeInfo: jsonTypeInfo ?? throw new ArgumentNullException(nameof(jsonTypeInfo)),
      jsonSerializerOptions: null,
      logger: logger
    );

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE
  [RequiresDynamicCode(FeatureRequirementMessages.RequiresDynamicCodeConditionalJsonSerializeInMethodOverload)]
  [RequiresUnreferencedCode(FeatureRequirementMessages.RequiresUnreferencedCodeConditionalJsonSerializeInMethodOverload)]
#endif
  private TResponse? DecryptCore<TResponse>(
    ReadOnlySpan<byte> encryptedText,
    int sequenceNumber,
    JsonTypeInfo<TResponse>? jsonTypeInfo,
    JsonSerializerOptions? jsonSerializerOptions,
    ILogger? logger
  ) where TResponse : ITapoPassThroughResponse
  {
    ThrowIfDisposed();

    decryptionBuffer.Clear();

    klap.Decrypt(
      encryptedText,
      sequenceNumber,
      decryptionBuffer
    );

    logger?.LogTrace(
      "KLAP Decrypted text (seq={SequenceNumber}): {DecryptedText}",
      sequenceNumber,
      Encoding.UTF8.GetString(decryptionBuffer.WrittenSpan)
    );

    return jsonTypeInfo is { } typeInfo
      ? JsonSerializer.Deserialize(
          decryptionBuffer.WrittenSpan,
          typeInfo
        )
      : JsonSerializer.Deserialize<TResponse>(
          decryptionBuffer.WrittenSpan,
          jsonSerializerOptions
        );
  }
}
