// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

#pragma warning disable IDE0040
partial class TapoClient {
#pragma warning restore IDE0040
  private async ValueTask<TResponse> PostSecurePassThroughRequestAsync<TRequest, TResponse>(
    TapoSecurePassThroughSession securePassThroughSession,
    TRequest request,
    JsonSerializerOptions jsonSerializerOptions,
    CancellationToken cancellationToken
  )
    where TRequest : notnull, ITapoPassThroughRequest
    where TResponse : ITapoPassThroughResponse
  {
    cancellationToken.ThrowIfCancellationRequested();

    logger?.LogDebug("Request method: {RequestMethod}", request.Method);

    var (requestUri, securePassThroughResponse, _) = await PostPlainTextRequestAsync<
      SecurePassThroughRequest<TRequest>,
      SecurePassThroughResponse<TResponse>,
      None
    >(
      requestPathAndQuery: securePassThroughSession.RequestPathAndQuery ?? TapoSecurePassThroughSession.RequestPath,
      request: new(passThroughRequest: request),
      jsonSerializerOptions: jsonSerializerOptions,
      processHttpResponseAsync: static _ => default,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    var response = securePassThroughResponse.Result.PassThroughResponse;

    logger?.LogDebug("Respose error code: {ErrorCode} ({RequestMethod})", response.ErrorCode, request.Method);

    TapoErrorResponseException.ThrowIfError(
      requestUri,
      request.Method,
      response.ErrorCode
    );

    return response;
  }

  private async ValueTask<(
    Uri RequestUri,
    TResponse Response,
    THttpResult? HttpResult
  )>
  PostPlainTextRequestAsync<TRequest, TResponse, THttpResult>(
    Uri requestPathAndQuery,
    TRequest request,
    JsonSerializerOptions jsonSerializerOptions,
    Func<HttpResponseMessage, ValueTask<THttpResult?>> processHttpResponseAsync,
    CancellationToken cancellationToken
  )
    where TRequest : ITapoRequest
    where TResponse : ITapoResponse
  {
    cancellationToken.ThrowIfCancellationRequested();

    logger?.LogDebug("HTTP Transaction: Session={SessionId}, Token={Token}", session?.SessionId, session?.Token);

    using var requestContent = JsonContent.Create(
      inputValue: request,
      mediaType: MediaTypeJson,
      options: jsonSerializerOptions
    );

    if (session?.SessionId is not null) {
      requestContent.Headers.Add(
        "Cookie",
        string.Concat(TapoSessionCookieUtils.HttpCookiePrefixForSessionId, session.SessionId)
      );
    }

    // Disables 'chunked' transfer encoding
    //   The HTTP server inside of the Tapo devices does not seem to support 'chunked' transfer encoding.
    //   To prevent content from being transferred by 'chunked', serialize the content onto a memory buffer
    //   and ensure the HttpClient to calculate Content-Length before transferring.
    //
    // ref:
    //   https://github.com/dotnet/runtime/issues/49357
    //   https://github.com/dotnet/runtime/issues/70793
    await requestContent.LoadIntoBufferAsync().ConfigureAwait(false);

    var (requestAbsoluteUri, (response, httpResponse)) = await PostAsync(
      requestUri: requestPathAndQuery,
      requestContent: requestContent,
      processHttpResponseAsync: async httpResponse =>
        (
          await httpResponse.Content.ReadFromJsonAsync<TResponse>(
            cancellationToken: cancellationToken,
            options: jsonSerializerOptions
          ).ConfigureAwait(false),
          await processHttpResponseAsync(httpResponse).ConfigureAwait(false)
        ),
      logContentAsKlapProtocol: false,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

#if DEBUG
    if (response is null)
      throw new InvalidOperationException($"{nameof(response)} is null");
#endif

    TapoErrorResponseException.ThrowIfError(
      requestAbsoluteUri,
      request.Method,
      response.ErrorCode
    );

    return (
      requestAbsoluteUri,
      response,
      httpResponse
    );
  }
}
