// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
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

  private static readonly JsonSerializerOptions CommonJsonSerializerOptions = new() {
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

#if false
    // disable encoding '+' in base64 strings
    // ref: https://github.com/dotnet/runtime/issues/35281
    // Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
#endif
  };

  /*
   * instance members
   */
  private bool IsDisposed => httpClientFactory is null;

  private IHttpClientFactory? httpClientFactory; // if null, it indicates a 'disposed' state.
  private readonly Uri endPointUri;
  private readonly EndPoint endPoint;
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
      return endPointUri;
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
    IHttpClientFactory? httpClientFactory = null,
    ILogger? logger = null
  )
  {
    this.endPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
    this.logger = logger;

    endPointUri = GetEndPointUri(endPoint);

    logger?.LogTrace("Device end point: {DeviceEndPointUri}", endPointUri);

    this.httpClientFactory = httpClientFactory ?? TapoHttpClientFactory.Default;

    logger?.LogTrace("IHttpClientFactory: {IHttpClientFactory}", this.httpClientFactory.GetType().FullName);
  }

  private void Dispose(bool disposing)
  {
    if (!disposing)
      return;

    session?.Dispose();
    session = null;

    httpClientFactory = null;
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

  public ValueTask AuthenticateAsync(
    ITapoCredentialProvider credential,
    CancellationToken cancellationToken = default
  )
  {
    if (credential is null)
      throw new ArgumentNullException(nameof(credential));

    ThrowIfDisposed();

    return AuthenticateAsyncCore(
      credential,
      cancellationToken
    );
  }

  public ValueTask<TResponse> SendRequestAsync<TRequest, TResponse>(
    CancellationToken cancellationToken = default
  )
    where TRequest : ITapoPassThroughRequest, new()
    where TResponse : ITapoPassThroughResponse
    => SendRequestAsync<TRequest, TResponse>(
      request: new(),
      cancellationToken: cancellationToken
    );

  public ValueTask<TResponse> SendRequestAsync<TRequest, TResponse>(
    TRequest request,
    CancellationToken cancellationToken = default
  )
    where TRequest : ITapoPassThroughRequest
    where TResponse : ITapoPassThroughResponse
  {
    if (request is null)
      throw new ArgumentNullException(nameof(request));

    ThrowIfDisposed();

    if (session is null)
      throw new InvalidOperationException("The session for this instance has not been established.");

    return PostSecurePassThroughRequestAsync<TRequest, TResponse>(
      request: request,
      jsonSerializerOptions: session.SecurePassThroughJsonSerializerOptions,
      cancellationToken: cancellationToken
    );
  }
}
