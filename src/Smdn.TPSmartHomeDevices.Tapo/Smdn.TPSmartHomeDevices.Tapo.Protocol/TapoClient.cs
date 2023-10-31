// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// Provides a client implementation that authenticates, sends requests, and receives responses according to the protocol used in the Tapo's communication.
/// </summary>
/// <remarks>
/// This implementation is based on and ported from the following
/// Python implementation by <see href="https://github.com/fishbigger">Toby Johnson</see>:
/// <see href="https://github.com/fishbigger/TapoP100">fishbigger/TapoP100</see>, published under the MIT License.
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
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
  [MemberNotNullWhen(false, nameof(httpClientFactory))]
#endif
  private bool IsDisposed => httpClientFactory is null;

  private IHttpClientFactory httpClientFactory; // if null, it indicates a 'disposed' state.
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

    this.httpClientFactory = httpClientFactory ?? DefaultHttpClientFactory;

    logger?.LogTrace("IHttpClientFactory: {IHttpClientFactory}", this.httpClientFactory.GetType().FullName);
  }

  private void Dispose(bool disposing)
  {
    if (!disposing)
      return;

    session?.Dispose();
    session = null;

    httpClientFactory = null!;
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

  /// <remarks>
  /// This method first attempts to authenticate with the protocol that
  /// use the 'securePassthrough' request method. And if that fails,
  /// falls back to KLAP protocol to continue.
  /// </remarks>
  public ValueTask AuthenticateAsync(
    ITapoCredentialIdentity? identity,
    ITapoCredentialProvider credential,
    CancellationToken cancellationToken = default
  )
  {
    if (credential is null)
      throw new ArgumentNullException(nameof(credential));

    ThrowIfDisposed();

    return AuthenticateAsyncCore(
      protocol: null,
      identity: identity,
      credential: credential,
      cancellationToken: cancellationToken
    );
  }

  public ValueTask AuthenticateAsync(
    TapoSessionProtocol protocol,
    ITapoCredentialIdentity? identity,
    ITapoCredentialProvider credential,
    CancellationToken cancellationToken = default
  )
  {
    if (credential is null)
      throw new ArgumentNullException(nameof(credential));

    ThrowIfDisposed();

    return AuthenticateAsyncCore(
      protocol: protocol switch {
        TapoSessionProtocol.Klap or
        TapoSessionProtocol.SecurePassThrough => protocol,
        _ => throw new ArgumentException($"invalid value of ${nameof(TapoSessionProtocol)}", paramName: nameof(protocol)),
      },
      identity: identity,
      credential: credential,
      cancellationToken: cancellationToken
    );
  }

  public ValueTask<TResponse> SendRequestAsync<TRequest, TResponse>(
    CancellationToken cancellationToken = default
  )
    where TRequest : notnull, ITapoPassThroughRequest, new()
    where TResponse : ITapoPassThroughResponse
    => SendRequestAsync<TRequest, TResponse>(
      request: new(),
      cancellationToken: cancellationToken
    );

  public ValueTask<TResponse> SendRequestAsync<TRequest, TResponse>(
    TRequest request,
    CancellationToken cancellationToken = default
  )
    where TRequest : notnull, ITapoPassThroughRequest
    where TResponse : ITapoPassThroughResponse
  {
    if (request is null)
      throw new ArgumentNullException(nameof(request));

    ThrowIfDisposed();

    if (session is null)
      throw new InvalidOperationException("The session for this instance has not been established.");

    return session switch {
      TapoSecurePassThroughSession securePassThroughSession => PostSecurePassThroughRequestAsync<TRequest, TResponse>(
        securePassThroughSession: securePassThroughSession,
        request: request,
        jsonSerializerOptions: securePassThroughSession.SecurePassThroughJsonSerializerOptions,
        cancellationToken: cancellationToken
      ),
      TapoKlapSession klapSession => PostKlapRequestAsync<TRequest, TResponse>(
        klapSession: klapSession,
        request: request,
        jsonSerializerOptions: CommonJsonSerializerOptions,
        cancellationToken: cancellationToken
      ),
      _ => throw new InvalidOperationException($"Invalid type of session: {session.GetType().FullName}"),
    };
  }
}
