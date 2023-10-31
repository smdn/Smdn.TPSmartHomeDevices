// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
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
    ITapoCredentialIdentity? identity,
    ITapoCredentialProvider credential,
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();

    var prevSession = session;

    try {
      await AuthenticateSecurePassThroughAsync(
        identity,
        credential,
        cancellationToken
      ).ConfigureAwait(false);

      prevSession?.Dispose();
    }
    catch {
      session?.Dispose();
      session = null;

      throw;
    }
  }
}
