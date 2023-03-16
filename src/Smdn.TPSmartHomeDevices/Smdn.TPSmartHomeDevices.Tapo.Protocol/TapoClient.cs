// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <remarks>
/// This implementation is based on and ported from the following implementation: <see href="https://github.com/fishbigger/TapoP100">fishbigger/TapoP100</see>.
/// </remarks>
public sealed partial class TapoClient : IDisposable {
  public const int DefaultPort = 80; // HTTP

  internal static Uri GetEndPointUri(EndPoint endPoint)
  {
    // 'http://<endPoint.Host>:<endPoint.Port>/'
    var uriBuilder = endPoint switch {
      null => throw new ArgumentNullException(nameof(endPoint)),
      DnsEndPoint dnsEndPoint => new UriBuilder() {
        Host = dnsEndPoint.Host,
        Port = dnsEndPoint.Port == 0 ? -1 : dnsEndPoint.Port,
      },
      IPEndPoint ipEndPoint => new UriBuilder() {
        Host = ipEndPoint.Address.ToString(),
        Port = ipEndPoint.Port == 0 ? -1 : ipEndPoint.Port,
      },
      _ => new UriBuilder() {
        Host = endPoint.ToString(), // XXX
        Port = -1,
      },
    };

    uriBuilder.Scheme = Uri.UriSchemeHttp;

    return uriBuilder.Uri;
  }

  private bool IsDisposed => httpClient is null;

  private ITapoCredentialProvider? credentialProvider;
  private HttpClient? httpClient; // if null, it indicates a 'disposed' state.
  private readonly EndPoint endPoint;
  private readonly IServiceProvider? serviceProvider;
  private readonly ILogger? logger;
  private TapoSession? session;

  public TapoSession? Session {
    get {
      ThrowIfDisposed();
      return session;
    }
  }

  public Uri EndPointUri {
    get {
      ThrowIfDisposed();
      return httpClient.BaseAddress;
    }
  }

  internal EndPoint EndPoint {
    get {
      ThrowIfDisposed();
      return endPoint;
    }
  }

  internal ILogger? Logger {
    get {
      ThrowIfDisposed();
      return logger;
    }
  }

  public TapoClient(
    EndPoint endPoint,
    ITapoCredentialProvider? credentialProvider = null,
    IServiceProvider? serviceProvider = null
  )
  {
    this.endPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
    this.credentialProvider =
      credentialProvider
      ?? serviceProvider?.GetService<ITapoCredentialProvider>()
      ?? throw new ArgumentException("No credential provider supplied.", nameof(credentialProvider));

    this.serviceProvider = serviceProvider;

    logger = serviceProvider?.GetService<ILoggerFactory>()?.CreateLogger($"{nameof(TapoClient)}({endPoint})"); // TODO: logger category name

    var endPointUri = GetEndPointUri(endPoint);

    logger?.LogTrace("Device end point: {DeviceEndPointUri}", endPointUri);

    var httpClientFactory = serviceProvider?.GetService<IHttpClientFactory>() ?? TapoHttpClientFactory.Instance;

    logger?.LogTrace("IHttpClientFactory: {IHttpClientFactory}", httpClientFactory?.GetType().FullName);

    httpClient = httpClientFactory.CreateClient(nameof(TapoClient)); // TODO: name

    httpClient.BaseAddress = endPointUri;
  }

  private void Dispose(bool disposing)
  {
    if (!disposing)
      return;

    session?.Dispose();
    session = null;

    credentialProvider = null;

    httpClient = null;
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  private void ThrowIfDisposed()
  {
    if (IsDisposed)
      throw new ObjectDisposedException(GetType().FullName);
  }

  public Task AuthenticateAsync(
    CancellationToken cancellationToken = default
  )
  {
    ThrowIfDisposed();

    return AuthenticateAsyncCore(cancellationToken);
  }

  public void CloseSession()
  {
    ThrowIfDisposed();

    // dispose the established session and set to null
    session?.Dispose();
    session = null;
  }

  public Task<TResponse> SendRequestAsync<TRequest, TResponse>(
    CancellationToken cancellationToken = default
  )
    where TRequest : ITapoPassThroughRequest, new()
    where TResponse : ITapoPassThroughResponse
    => SendRequestAsync<TRequest, TResponse>(
      request: new(),
      cancellationToken: cancellationToken
    );

  public Task<TResponse> SendRequestAsync<TRequest, TResponse>(
    TRequest request,
    CancellationToken cancellationToken = default
  )
    where TRequest : ITapoPassThroughRequest
    where TResponse : ITapoPassThroughResponse
  {
    if (request is null)
      throw new ArgumentNullException(nameof(request));

    ThrowIfDisposed();

    if (Session is null)
      throw new InvalidOperationException("The session for this instance has not been established.");

    return PostSecurePassThroughRequestAsync<TRequest, TResponse>(
      request,
      cancellationToken
    );
  }
}
