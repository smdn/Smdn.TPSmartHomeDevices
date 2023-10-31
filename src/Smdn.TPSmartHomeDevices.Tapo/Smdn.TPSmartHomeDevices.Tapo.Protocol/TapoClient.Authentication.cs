// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

#pragma warning disable IDE0040
partial class TapoClient {
#pragma warning restore IDE0040
  // TODO: logger
  private static ValueTask<(string? SessionId, int? SessionTimeout)> ParseSessionCookieAsync(HttpResponseMessage response)
    => TapoSessionCookieUtils.TryGetCookie(response, out var sessionId, out var sessionTimeout)
      ? new(result: (sessionId, sessionTimeout))
      : default;

  private async ValueTask AuthenticateAsyncCore(
    TapoSessionProtocol? protocol,
    ITapoCredentialIdentity? identity,
    ITapoCredentialProvider credential,
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();

    var prevSession = session;

    try {
      switch (protocol) {
        case TapoSessionProtocol.Klap:
          await AuthenticateKlapAsync(
            identity,
            credential,
            cancellationToken
          ).ConfigureAwait(false);
          break;

        case TapoSessionProtocol.SecurePassThrough:
          await AuthenticateSecurePassThroughAsync(
            identity,
            credential,
            cancellationToken
          ).ConfigureAwait(false);
          break;

        case null:
          await AuthenticateProtocolUnspecifiedAsync(
            identity,
            credential,
            cancellationToken
          ).ConfigureAwait(false);
          break;

        default:
          throw new InvalidOperationException($"invalid protocol specified ({protocol})");
      }

      prevSession?.Dispose();
    }
    catch {
      session?.Dispose();
      session = null;

      throw;
    }
  }

  private async ValueTask AuthenticateProtocolUnspecifiedAsync(
    ITapoCredentialIdentity? identity,
    ITapoCredentialProvider credential,
    CancellationToken cancellationToken
  )
  {
    // try 'handshake' method first, then fallback to KLAP protocol
    // if that fails with the error code 1003
    try {
      await AuthenticateSecurePassThroughAsync(
        identity,
        credential,
        cancellationToken
      ).ConfigureAwait(false);
    }
    catch (TapoAuthenticationException ex) when (
      ex.InnerException is TapoErrorResponseException errorResponseException &&
      "handshake".Equals(errorResponseException.RequestMethod, StringComparison.Ordinal) &&
      errorResponseException.RawErrorCode == 1003
    ) {
      await AuthenticateKlapAsync(
        identity,
        credential,
        cancellationToken
      ).ConfigureAwait(false);
    }
  }
}
