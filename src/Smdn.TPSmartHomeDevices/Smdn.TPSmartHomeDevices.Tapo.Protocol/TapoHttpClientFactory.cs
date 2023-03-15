// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using System.Net.Http;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

internal class TapoHttpClientFactory : IHttpClientFactory {
  private static HttpMessageHandler CreateHandler()
    =>
#if NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER
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

  public static IHttpClientFactory Instance { get; } = new TapoHttpClientFactory();

  public HttpClient CreateClient(string name)
    => new(
      handler: DefaultHandler,
      disposeHandler: false
    );
}
