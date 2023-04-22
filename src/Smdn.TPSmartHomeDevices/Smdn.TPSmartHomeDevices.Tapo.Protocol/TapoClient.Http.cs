// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#if NET5_0_OR_GREATER
#define SYSTEM_NET_HTTP_HTTPCONTENT_READASSTRINGASYNC_CANCELLATIONTOKEN
#endif
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

#pragma warning disable IDE0040
partial class TapoClient {
#pragma warning restore IDE0040
  private static readonly MediaTypeHeaderValue mediaTypeJson = new(mediaType: "application/json");

  public TimeSpan? Timeout { get; set; }

  private async ValueTask<TResponse> PostSecurePassThroughRequestAsync<TRequest, TResponse>(
    TRequest request,
    JsonSerializerOptions jsonSerializerOptions,
    CancellationToken cancellationToken
  )
    where TRequest : ITapoPassThroughRequest
    where TResponse : ITapoPassThroughResponse
  {
    cancellationToken.ThrowIfCancellationRequested();

    logger?.LogDebug("Request: {Request}", JsonSerializer.Serialize(request, jsonSerializerOptions));

    var (securePassThroughResponse, requestUri) = await PostPlainTextRequestAsync<
      SecurePassThroughRequest<TRequest>,
      SecurePassThroughResponse<TResponse>,
      Uri
    >(
      request: new(passThroughRequest: request),
      jsonSerializerOptions: jsonSerializerOptions,
      processHttpResponse: static httpResponse => httpResponse.RequestMessage.RequestUri,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    var response = securePassThroughResponse.Result.PassThroughResponse;

    logger?.LogDebug("Respose error code: {ErrorCode}", response.ErrorCode);

    TapoErrorResponseException.ThrowIfError(
      requestUri,
      request.Method,
      response.ErrorCode
    );

    return response;
  }

  private async ValueTask<(
    TResponse? Response,
    THttpResult? HttpResult
  )>
  PostPlainTextRequestAsync<TRequest, TResponse, THttpResult>(
    TRequest request,
    JsonSerializerOptions jsonSerializerOptions,
    Func<HttpResponseMessage, THttpResult?>? processHttpResponse,
    CancellationToken cancellationToken
  )
    where TRequest : ITapoRequest
    where TResponse : ITapoResponse
  {
    cancellationToken.ThrowIfCancellationRequested();

    logger?.LogDebug("HTTP Transaction: Session={SessionId}, Token={Token}", session?.SessionId, session?.Token);

    using var requestContent = JsonContent.Create(
      inputValue: request,
      mediaType: mediaTypeJson,
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

    var requestUri = session is null ? TapoSession.RequestPath : session.RequestPathAndQuery;

    logger?.LogTrace("HTTP Request URI: {RequestUri}", requestUri);
    logger?.LogTrace(
      "HTTP Request headers: {RequestHeaders}",
      string.Join(" ", requestContent.Headers.Select(static header => string.Concat(header.Key, ": ", string.Join("; ", header.Value))))
    );
    logger?.LogTrace(
      "HTTP Request content: {RequestContent}",
#if SYSTEM_NET_HTTP_HTTPCONTENT_READASSTRINGASYNC_CANCELLATIONTOKEN
      await requestContent.ReadAsStringAsync(cancellationToken).ConfigureAwait(false)
#else
      await requestContent.ReadAsStringAsync().ConfigureAwait(false)
#endif
    );

    using var httpClient = httpClientFactory.CreateClient(
      name: string.Concat(nameof(TapoClient), " (", endPointUri, ")")
    );

    httpClient.BaseAddress = endPointUri;

    if (Timeout.HasValue)
      // override timeout value configured by IHttpClientFactory
      httpClient.Timeout = Timeout.Value;

    using var httpResponse = await httpClient.PostAsync(
      requestUri: requestUri,
      content: requestContent,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    var httpResult = processHttpResponse is null
      ? default
      : processHttpResponse(httpResponse);

    logger?.LogTrace(
      "HTTP Response status: {ResponseStatusCode} {ResponseReasonPhrase}",
      (int)httpResponse.StatusCode,
      httpResponse.ReasonPhrase
    );
    logger?.LogTrace(
      "HTTP Response headers: {ResponseHeaders}",
      string.Join(" ", httpResponse.Content.Headers.Concat(httpResponse.Headers).Select(static header => string.Concat(header.Key, ": ", string.Join("; ", header.Value))))
    );
    logger?.LogTrace(
      "HTTP Response content: {ResponseContent}",
#if SYSTEM_NET_HTTP_HTTPCONTENT_READASSTRINGASYNC_CANCELLATIONTOKEN
      await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false)
#else
      await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false)
#endif
    );

    httpResponse.EnsureSuccessStatusCode();

    var response = await httpResponse.Content.ReadFromJsonAsync<TResponse>(
      cancellationToken: cancellationToken,
      options: jsonSerializerOptions
    ).ConfigureAwait(false);

    TapoErrorResponseException.ThrowIfError(
      httpResponse.RequestMessage.RequestUri,
      request.Method,
      response.ErrorCode
    );

    return (response, httpResult);
  }
}
