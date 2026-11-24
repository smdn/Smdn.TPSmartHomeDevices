// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: GPL-3.0-or-later
#pragma warning disable CA1873

using System;
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE || SYSTEM_DIAGNOSTICS_CODEANALYSIS_UNCONDITIONALSUPPRESSMESSAGEATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

partial class TapoClient {
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE
  [RequiresDynamicCode(FeatureRequirementMessages.RequiresDynamicCodeJsonSerializeInMethodOverload)]
  [RequiresUnreferencedCode(FeatureRequirementMessages.RequiresUnreferencedCodeJsonSerializeInMethodOverload)]
#endif
  private ValueTask<TResponse> PostKlapRequestAsync<TRequest, TResponse>(
    TapoKlapSession klapSession,
    TRequest request,
    JsonSerializerOptions jsonSerializerOptions,
    CancellationToken cancellationToken
  )
    where TRequest : notnull, ITapoPassThroughRequest
    where TResponse : ITapoPassThroughResponse
    => PostKlapRequestAsyncCore<TRequest, TResponse>(
      klapSession: klapSession,
      request: request,
      requestTypeInfo: null,
      responseTypeInfo: null,
      jsonSerializerOptions: jsonSerializerOptions,
      cancellationToken: cancellationToken
    );

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_UNCONDITIONALSUPPRESSMESSAGEATTRIBUTE
  [UnconditionalSuppressMessage(category: "AOT", checkId: "IL3050", Justification = UnconditionalSuppressMessages.IL3050JustificationAotCompatibleCodePathUsingJsonTypeInfo)]
  [UnconditionalSuppressMessage(category: "Trimming", checkId: "IL2026", Justification = UnconditionalSuppressMessages.IL2026JustificationReflectionFreeCodePathUsingJsonTypeInfo)]
#endif
  private ValueTask<TResponse> PostKlapRequestAsync<TRequest, TResponse>(
    TapoKlapSession klapSession,
    TRequest request,
    JsonTypeInfo<TRequest> requestTypeInfo,
    JsonTypeInfo<TResponse> responseTypeInfo,
    CancellationToken cancellationToken
  )
    where TRequest : notnull, ITapoPassThroughRequest
    where TResponse : ITapoPassThroughResponse
    => PostKlapRequestAsyncCore(
      klapSession: klapSession,
      request: request,
      requestTypeInfo: requestTypeInfo ?? throw new ArgumentNullException(nameof(requestTypeInfo)),
      responseTypeInfo: responseTypeInfo ?? throw new ArgumentNullException(nameof(responseTypeInfo)),
      jsonSerializerOptions: null,
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// A C# implementation of transmission process with Tapo's KLAP protocol.
  /// </summary>
  /// <remarks>
  /// This implementation is based on and ported from the following
  /// Python implementation by <see href="https://github.com/petretiandrea">petretiandrea</see>:
  /// <see href="https://github.com/petretiandrea/plugp100">petretiandrea/plugp100</see>, published under the GPL-3.0 license,
  /// forked from <see href="https://github.com/K4CZP3R/tapo-p100-python">K4CZP3R/tapo-p100-python</see>.
  /// </remarks>
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE
  [RequiresDynamicCode(FeatureRequirementMessages.RequiresDynamicCodeConditionalJsonSerializeInMethodOverload)]
  [RequiresUnreferencedCode(FeatureRequirementMessages.RequiresUnreferencedCodeConditionalJsonSerializeInMethodOverload)]
#endif
  private async ValueTask<TResponse> PostKlapRequestAsyncCore<TRequest, TResponse>(
    TapoKlapSession klapSession,
    TRequest request,
    JsonTypeInfo<TRequest>? requestTypeInfo,
    JsonTypeInfo<TResponse>? responseTypeInfo,
    JsonSerializerOptions? jsonSerializerOptions,
    CancellationToken cancellationToken
  )
    where TRequest : notnull, ITapoPassThroughRequest
    where TResponse : ITapoPassThroughResponse
  {
    cancellationToken.ThrowIfCancellationRequested();

    var httpRequestContent = klapSession.GetHttpRequestContentBuffer();

    var (requestPathAndQuery, sequenceNumber) = requestTypeInfo is { } typeInfo
      ? klapSession.Encrypt(
          request: request,
          jsonTypeInfo: requestTypeInfo,
          destination: httpRequestContent
        )
      : klapSession.Encrypt(
          request: request,
          jsonSerializerOptions: jsonSerializerOptions,
          destination: httpRequestContent
        );

    logger?.LogDebug("HTTP Transaction: Session={SessionId}, SequenceNumber={SequenceNumber}", klapSession.SessionId, sequenceNumber);

    using var requestContent = new ReadOnlyMemoryContent(httpRequestContent.WrittenMemory);

    if (klapSession.SessionId is not null) {
      requestContent.Headers.Add(
        "Cookie",
        string.Concat(TapoSessionCookieUtils.HttpCookiePrefixForSessionId, klapSession.SessionId)
      );
    }

    var (requestAbsoluteUri, response) = await PostAsync(
      requestUri: requestPathAndQuery,
      requestContent: requestContent,
      processHttpResponseAsync: async httpResponse => responseTypeInfo is { } typeInfo
        ? klapSession.Decrypt(
            encryptedText: await httpResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false), // TODO: reduce allocation
            sequenceNumber: sequenceNumber,
            jsonTypeInfo: typeInfo,
            logger: logger
          )
        : klapSession.Decrypt<TResponse>(
            encryptedText: await httpResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false), // TODO: reduce allocation
            sequenceNumber: sequenceNumber,
            jsonSerializerOptions: jsonSerializerOptions,
            logger: logger
          ),
      logContentAsKlapProtocol: true,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

#if DEBUG
    if (response is null)
      throw new System.InvalidOperationException($"{nameof(response)} is null");
#endif

#if !DEBUG
#pragma warning disable CS8602
#endif
    TapoErrorResponseException.ThrowIfError(
      requestAbsoluteUri,
      request.Method,
      response.ErrorCode
    );
#if !DEBUG
#pragma warning restore CS8602
#endif

    return response;
  }
}
