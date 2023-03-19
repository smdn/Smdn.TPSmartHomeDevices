// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

#pragma warning disable IDE0040
partial class TapoClient {
#pragma warning restore IDE0040
  private const int KeyExchangeAlgorithmKeySizeInBytes = 128;

  private async ValueTask AuthenticateAsyncCore(
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();

    var prevSession = session;

    try {
      logger?.LogDebug("Handshake starting: {BaseAddress}", httpClient.BaseAddress);

      session = await HandshakeAsync(cancellationToken).ConfigureAwait(false);

      logger?.LogInformation(
        "Session initiated: {BaseAddress} {SessionIDPrefix}{SessionID}, expires on {ExpiresOn}",
        httpClient.BaseAddress,
        TapoSessionCookieUtils.HttpCookiePrefixForSessionId,
        session.SessionId,
        session.ExpiresOn.ToString("o", provider: null)
      );

      cancellationToken.ThrowIfCancellationRequested();

      string token;

      try {
        var loginDeviceResponse = await SendRequestAsync<LoginDeviceRequest, LoginDeviceResponse>(
          request: new(
            password: credentialProvider.GetBase64EncodedPassword(httpClient.BaseAddress.Host),
            userName: credentialProvider.GetBase64EncodedUserNameSHA1Digest(httpClient.BaseAddress.Host)
          ),
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        token = loginDeviceResponse.Result.Token;
      }
      catch (TapoErrorResponseException ex) {
        throw new TapoAuthenticationException(
          message: $"Denied to initiate authorized session with the device at '{httpClient.BaseAddress}'. (error code: {(int)ex.ErrorCode})",
          endPoint: httpClient.BaseAddress,
          innerException: ex
        );
      }
      catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException exTimeout) {
        logger?.LogCritical("Failed to initiate authorized session due to timeout. ({ExceptionMessage})", ex.Message);
        throw new TapoAuthenticationException(
          message: $"Failed to initiate authorized session with the device at '{httpClient.BaseAddress}' due to timeout. ({ex.Message})",
          endPoint: httpClient.BaseAddress,
          innerException: exTimeout
        );
      }

      if (string.IsNullOrEmpty(token)) {
        logger?.LogError("Access token has not been issued.");
        throw new TapoAuthenticationException(
          message: $"An access token was not issued from the device at '{httpClient.BaseAddress}'.",
          endPoint: httpClient.BaseAddress
        );
      }

      logger?.LogInformation("Access token has issued: {Token}", token);

      session.SetToken(token);

      logger?.LogDebug("Request path and query for the session: '{PathAndQuery}'", session.RequestPathAndQuery);

      prevSession?.Dispose();
    }
    catch {
      session?.Dispose();
      session = null;

      throw;
    }
  }

  private async ValueTask<TapoSession> HandshakeAsync(
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();

    using var keyExchangeAlgorithm = RSA.Create(keySizeInBits: KeyExchangeAlgorithmKeySizeInBytes * 8);

    var publicKeyInfoPem =
#if SYSTEM_SECURITY_CRYPTOGRAPHY_ASYMMETRICALGORITHM_EXPORTSUBJECTPUBLICKEYINFOPEM
      keyExchangeAlgorithm.ExportSubjectPublicKeyInfoPem();
#else
      AsymmetricAlgorithmShim.ExportSubjectPublicKeyInfoPem(keyExchangeAlgorithm!);
#endif

    logger?.LogTrace("[Handshake] Public key: {PublicKeyInfoPem}", publicKeyInfoPem);

    var baseTimeForExpiration = DateTime.Now;
    string base64EncryptedKey;
    string? sessionId;
    int? sessionTimeout;

    try {
      (var response, (sessionId, sessionTimeout)) = await PostPlainTextRequestAsync<
        HandshakeRequest,
        HandshakeResponse,
        (string?, int?)
      >(
        request: new(key: publicKeyInfoPem),
        processHttpResponse: ParseSessionCookie,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      if (response.Result.Key is null) {
        logger?.LogCritical("Could not exchange the key during handshaking.");
        throw new TapoAuthenticationException(
          message: $"Could not exchange the key during handshaking with the device at '{httpClient.BaseAddress}'.",
          endPoint: httpClient.BaseAddress
        );
      }

      base64EncryptedKey = response.Result.Key;
    }
    catch (TapoErrorResponseException ex) {
      logger?.LogCritical("Failed to handshake with error code {ErrorCode}.", (int)ex.ErrorCode);
      throw new TapoAuthenticationException(
        message: $"Failed to handshake with the device at '{httpClient.BaseAddress}' with error code {(int)ex.ErrorCode}.",
        endPoint: httpClient.BaseAddress,
        innerException: ex
      );
    }
    catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException exTimeout) {
      logger?.LogCritical("Failed to handshake due to timeout. ({ExceptionMessage})", ex.Message);
      throw new TapoAuthenticationException(
        message: $"Failed to handshake with the device at '{httpClient.BaseAddress}' due to timeout. ({ex.Message})",
        endPoint: httpClient.BaseAddress,
        innerException: exTimeout
      );
    }

    logger?.LogTrace("[Handshake] Exchanged key: {Key}", base64EncryptedKey);
    logger?.LogTrace("[Handshake] Session ID: {SessionId}", sessionId);
    logger?.LogTrace("[Handshake] Session timeout: {SessionTimeout}", sessionTimeout);

    var encryptedKey = Convert.FromBase64String(base64EncryptedKey);

    if (encryptedKey.Length != KeyExchangeAlgorithmKeySizeInBytes) {
      logger?.LogCritical("Exchanged unexpecting length of key");
      throw new TapoAuthenticationException(
        message: $"Exchanged unexpecting length of key from the device at '{httpClient.BaseAddress}'.",
        endPoint: httpClient.BaseAddress
      );
    }

    var keyBytes = keyExchangeAlgorithm.Decrypt(
      data: encryptedKey,
      padding: RSAEncryptionPadding.Pkcs1
    );
    var expiresOn = sessionTimeout.HasValue
      ? baseTimeForExpiration + TimeSpan.FromMinutes(sessionTimeout.Value)
      : DateTime.MaxValue;

    logger?.LogTrace(
      "[Handshake] Session expires on: {ExpiresOn}",
      expiresOn.ToString("o", provider: null)
    );
    logger?.LogTrace(
      "[Handshake] Key: {Key}",
#if SYSTEM_CONVERT_TOHEXSTRING
      Convert.ToHexString(keyBytes.AsSpan(0, 16))
#else
      Smdn.Formats.Hexadecimal.ToUpperCaseString(keyBytes.AsSpan(0, 16))
#endif
    );
    logger?.LogTrace(
      "[Handshake] IV: {IV}",
#if SYSTEM_CONVERT_TOHEXSTRING
      Convert.ToHexString(keyBytes.AsSpan(16, 16))
#else
      Smdn.Formats.Hexadecimal.ToUpperCaseString(keyBytes.AsSpan(16, 16))
#endif
    );

    return new(
      sessionId: sessionId,
      expiresOn: expiresOn,
      key: keyBytes.AsSpan(0, 16),
      iv: keyBytes.AsSpan(16, 16),
      plainTextJsonSerializerOptions: defaultJsonSerializerOptions,
      logger: logger
    );

    // TODO: logger
    static (string? SessionId, int? SessionTimeout) ParseSessionCookie(HttpResponseMessage response)
      => TapoSessionCookieUtils.TryGetCookie(response, out var sessionId, out var sessionTimeout)
        ? (sessionId, sessionTimeout)
        : default;
  }
}
