// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.Http;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

internal class TapoHttpClientFactory : IHttpClientFactory {
  private static HttpMessageHandler CreateHandler()
    =>
#if SYSTEM_NET_HTTP_SOCKETSHTTPHANDLER
      new SocketsHttpHandler()
#else
      new HttpClientHandler()
#endif
      {
        AllowAutoRedirect = false,
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        MaxConnectionsPerServer = 1,
        UseCookies = false,
      };

  private static HttpMessageHandler DefaultHandler { get; } = CreateHandler();

  public static IHttpClientFactory Default { get; } = new TapoHttpClientFactory(
    configureClient: null
  );

  /*
   * instance members
   */
  private readonly Action<HttpClient>? configureClient;

  internal TapoHttpClientFactory(
    Action<HttpClient>? configureClient
  )
  {
    this.configureClient = configureClient;
  }

  public HttpClient CreateClient(string name)
  {
    var client = new HttpClient(
      handler: DefaultHandler,
      disposeHandler: false
    ) {
      Timeout = TimeSpan.FromSeconds(20),
    };

    configureClient?.Invoke(client);

    return client;
  }
}
