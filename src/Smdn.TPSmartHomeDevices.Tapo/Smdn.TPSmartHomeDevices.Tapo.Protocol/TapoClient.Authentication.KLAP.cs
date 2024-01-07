// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: GPL-3.0-or-later
using System;
using System.Buffers;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

#pragma warning disable IDE0040
partial class TapoClient {
#pragma warning restore IDE0040
  private const int SHA256HashSizeInBytes =
#if SYSTEM_SECURITY_CRYPTOGRAPHY_SHA256_HASHSIZEINBYTES
    SHA256.HashSizeInBytes;
#else
    32;
#endif

  /// <summary>
  /// A C# implementation of authentication process of Tapo's KLAP protocol.
  /// </summary>
  /// <remarks>
  /// This implementation is based on and ported from the following
  /// Python implementation by <see href="https://github.com/petretiandrea">petretiandrea</see>:
  /// <see href="https://github.com/petretiandrea/plugp100">petretiandrea/plugp100</see>, published under the GPL-3.0 license,
  /// forked from <see href="https://github.com/K4CZP3R/tapo-p100-python">K4CZP3R/tapo-p100-python</see>.
  /// </remarks>
  private async ValueTask AuthenticateKlapAsync(
    ITapoCredentialIdentity? identity,
    ITapoCredentialProvider credentialProvider,
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();

    // local_seed = rand() * 16
    var localSeed = new byte[16];

    RandomNumberGenerator.Fill(localSeed);

    // local_auth_hash = SHA256(SHA1(username) + SHA1(password))
    var localAuthHash = new byte[SHA256HashSizeInBytes];

    using (
      var credential =
        credentialProvider.GetCredential(identity)
        ?? throw TapoCredentials.CreateExceptionNoCredentialForIdentity(identity)
    ) {
      _ = TapoCredentials.TryComputeKlapAuthHash(
        credential,
        localAuthHash.AsSpan(0, SHA256HashSizeInBytes),
#if DEBUG
        out var bytesWritten
#else
        out _
#endif
      );

#if DEBUG
      if (bytesWritten != SHA256HashSizeInBytes)
        throw new InvalidOperationException("invalid legnth of local_auth_hash");
#endif
    }

    var baseTimeForExpiration = DateTime.Now;

    var (remoteSeed, sessionId, sessionTimeout) = await KlapProtocolHandshake1Async(
      localSeed: localSeed,
      localAuthHash: localAuthHash,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    await KlapProtocolHandshake2Async(
      localSeed: localSeed,
      localAuthHash: localAuthHash,
      remoteSeed: remoteSeed,
      sessionId: sessionId,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    var expiresOn = sessionTimeout.HasValue
      ? baseTimeForExpiration + TimeSpan.FromMinutes(sessionTimeout.Value)
      : DateTime.MaxValue;

    logger?.LogTrace(
      "[Handshake] Session expires on: {ExpiresOn}",
      expiresOn.ToString("o", provider: null)
    );

    session = new TapoKlapSession(
      sessionId: sessionId,
      expiresOn: expiresOn,
      localSeed: localSeed,
      remoteSeed: remoteSeed.Span,
      userHash: localAuthHash,
      logger: logger
    );

    /*
      * session established
      */
    logger?.LogInformation(
      "KLAP session established: {SessionIDPrefix}{SessionID}; expires on {ExpiresOn}",
      TapoSessionCookieUtils.HttpCookiePrefixForSessionId,
      session.SessionId,
      session.ExpiresOn.ToString("o", provider: null)
    );
  }

  private static readonly Uri RequestUriKlapHandshake1 = new("/app/handshake1", UriKind.Relative);
  private static readonly Uri RequestUriKlapHandshake2 = new("/app/handshake2", UriKind.Relative);

  private async
  ValueTask<(
    ReadOnlyMemory<byte> RemoteSeed,
    string? SessionId,
    int? SessionTimeout
  )>
  KlapProtocolHandshake1Async(
    ReadOnlyMemory<byte> localSeed,
    ReadOnlyMemory<byte> localAuthHash,
    CancellationToken cancellationToken
  )
  {
    using var content = new ReadOnlyMemoryContent(localSeed);

    var (_, (responseHandshake1, sessionId, sessionTimeout)) = await PostAsync(
      requestUri: RequestUriKlapHandshake1,
      requestContent: content,
      processHttpResponseAsync: async response => {
        var responseHandshake1 = await response
          .Content
#if NET5_0_OR_GREATER
          .ReadAsByteArrayAsync(cancellationToken)
#else
          .ReadAsByteArrayAsync()
#endif
          .ConfigureAwait(false);

        var (sessionId, sessionTimeout) = await ParseSessionCookieAsync(response).ConfigureAwait(false);

        return (responseHandshake1, sessionId, sessionTimeout);
      },
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    var remoteSeed = responseHandshake1.AsMemory(0, 16);
    var serverHash = responseHandshake1.AsMemory(16);

    logger?.LogTrace("[Handshake1] Session ID: {SessionId}", sessionId);
    logger?.LogTrace("[Handshake1] Session timeout: {SessionTimeout}", sessionTimeout);

    logger?.LogTrace(
      "[Handshake1] Remote seed: {RemoteSeed}",
#if SYSTEM_CONVERT_TOHEXSTRING
      Convert.ToHexString(remoteSeed.Span)
#else
      Smdn.Formats.Hexadecimal.ToUpperCaseString(remoteSeed.Span)
#endif
    );
    logger?.LogTrace(
      "[Handshake1] Server hash: {ServerHash}",
#if SYSTEM_CONVERT_TOHEXSTRING
      Convert.ToHexString(serverHash.Span)
#else
      Smdn.Formats.Hexadecimal.ToUpperCaseString(serverHash.Span)
#endif
    );

    // local_seed_auth_hash = SHA256(local_seed + remote_seed + local_auth_hash)
    byte[]? localSeedAuthHash = null;

    try {
      localSeedAuthHash = ArrayPool<byte>.Shared.Rent(SHA256HashSizeInBytes);

      using (var sha256 = SHA256.Create()) {
        _ = sha256.TryComputeHash(
          localSeedAuthHash.AsSpan(0, SHA256HashSizeInBytes),
          localSeed.Span,
          remoteSeed.Span,
          localAuthHash.Span,
          out _
        );
      }

      logger?.LogTrace(
        "[Handshake1] Client hash: {ClientHash}",
#if SYSTEM_CONVERT_TOHEXSTRING
        Convert.ToHexString(localSeedAuthHash.AsSpan(0, SHA256HashSizeInBytes))
#else
        Smdn.Formats.Hexadecimal.ToUpperCaseString(localSeedAuthHash.AsSpan(0, SHA256HashSizeInBytes))
#endif
      );

      if (!localSeedAuthHash.AsSpan(0, SHA256HashSizeInBytes).SequenceEqual(serverHash.Span)) {
        throw new TapoAuthenticationException(
          message: $"The hash of the client credential does not match the hash responded from the device at '{endPointUri}'.",
          endPoint: endPointUri
        );
      }
    }
    finally {
      if (localSeedAuthHash is not null)
        ArrayPool<byte>.Shared.Return(localSeedAuthHash, clearArray: true);
    }

    return (remoteSeed, sessionId, sessionTimeout);
  }

  private async ValueTask KlapProtocolHandshake2Async(
    ReadOnlyMemory<byte> localSeed,
    ReadOnlyMemory<byte> localAuthHash,
    ReadOnlyMemory<byte> remoteSeed,
    string? sessionId,
    CancellationToken cancellationToken
  )
  {
    // handshake2_payload = SHA256(remote_seed + local_seed + local_auth_hash)
    byte[]? handshake2Payload = null;

    try {
      handshake2Payload = ArrayPool<byte>.Shared.Rent(SHA256HashSizeInBytes);

      using (var sha256 = SHA256.Create()) {
        _ = sha256.TryComputeHash(
          handshake2Payload.AsSpan(0, SHA256HashSizeInBytes),
          remoteSeed.Span,
          localSeed.Span,
          localAuthHash.Span,
          out _
        );
      }

      using var content = new ReadOnlyMemoryContent(handshake2Payload);

      if (sessionId is not null) {
        content.Headers.Add(
          "Cookie",
          string.Concat(TapoSessionCookieUtils.HttpCookiePrefixForSessionId, sessionId)
        );
      }

      try {
        _ = await PostAsync<HttpStatusCode?>(
          requestUri: RequestUriKlapHandshake2,
          requestContent: content,
          processHttpResponseAsync: static httpResponse => new(result: httpResponse.StatusCode),
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
      }
      catch (HttpRequestException ex) {
        // TODO: exception message
#pragma warning disable SA1114
        throw new TapoAuthenticationException(
#if NET6_0_OR_GREATER
          message: $"Handshake with the device at '{endPointUri}' failed by status code {(int?)ex.StatusCode:D3}.",
#else
          message: $"Handshake with the device at '{endPointUri}' failed.",
#endif
          endPoint: endPointUri,
          innerException: ex
        );
#pragma warning restore SA1114
      }
    }
    finally {
      if (handshake2Payload is not null)
        ArrayPool<byte>.Shared.Return(handshake2Payload, clearArray: true);
    }
  }
}
