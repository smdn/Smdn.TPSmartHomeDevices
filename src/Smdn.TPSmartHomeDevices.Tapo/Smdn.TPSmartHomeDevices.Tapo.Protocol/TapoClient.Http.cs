// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

#pragma warning disable IDE0040
partial class TapoClient {
#pragma warning restore IDE0040
  /// <summary>
  /// Gets a default implementation of <see cref="IHttpClientFactory"/> that creates an <see cref="HttpClient"/> configured for the <see cref="TapoClient"/>.
  /// </summary>
  public static IHttpClientFactory DefaultHttpClientFactory => new TapoHttpClientFactory(
    configureClient: null
  );

  private static readonly MediaTypeHeaderValue mediaTypeJson = new(mediaType: "application/json");

  public TimeSpan? Timeout { get; set; }

  private async ValueTask<(Uri RequestAbsoluteUri, THttpResult? Result)> PostAsync<THttpResult>(
    Uri requestUri,
    HttpContent requestContent,
    Func<HttpResponseMessage, ValueTask<THttpResult?>> processHttpResponseAsync,
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();

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

    return (
      new Uri(httpClient.BaseAddress, requestUri),
      await processHttpResponseAsync(httpResponse).ConfigureAwait(false)
    );
  }
}
