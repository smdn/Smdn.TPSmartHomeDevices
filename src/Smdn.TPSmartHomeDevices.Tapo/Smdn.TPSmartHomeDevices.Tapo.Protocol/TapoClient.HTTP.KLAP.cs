// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: GPL-3.0-or-later
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

#pragma warning disable IDE0040
partial class TapoClient {
#pragma warning restore IDE0040
  /// <summary>
  /// A C# implementation of transmission process with Tapo's KLAP protocol.
  /// </summary>
  /// <remarks>
  /// This implementation is based on and ported from the following
  /// Python implementation by <see href="https://github.com/petretiandrea">petretiandrea</see>:
  /// <see href="https://github.com/petretiandrea/plugp100">petretiandrea/plugp100</see>, published under the GPL-3.0 license,
  /// forked from <see href="https://github.com/K4CZP3R/tapo-p100-python">K4CZP3R/tapo-p100-python</see>.
  /// </remarks>
  private async ValueTask<TResponse> PostKlapRequestAsync<TRequest, TResponse>(
    TapoKlapSession klapSession,
    TRequest request,
    JsonSerializerOptions jsonSerializerOptions,
    CancellationToken cancellationToken
  )
    where TRequest : notnull, ITapoPassThroughRequest
    where TResponse : ITapoPassThroughResponse
  {
    cancellationToken.ThrowIfCancellationRequested();

    var httpRequestContent = klapSession.GetHttpRequestContentBuffer();

    var (requestPathAndQuery, sequenceNumber) = klapSession.Encrypt(
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
      processHttpResponseAsync: async httpResponse => klapSession.Decrypt<TResponse>(
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
