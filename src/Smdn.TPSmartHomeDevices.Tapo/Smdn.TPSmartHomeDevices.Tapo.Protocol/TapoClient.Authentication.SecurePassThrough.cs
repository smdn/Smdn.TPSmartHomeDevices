// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

#pragma warning disable IDE0040
partial class TapoClient {
#pragma warning restore IDE0040
  private async ValueTask AuthenticateSecurePassThroughAsync(
    ITapoCredentialIdentity? identity,
    ITapoCredentialProvider credentialProvider,
    CancellationToken cancellationToken
  )
  {
    /*
     * handshake
     */
    logger?.LogDebug("Handshake starting: {EndPointUri}", endPointUri);

    var securePassThroughSession = await HandshakeAsync(identity, cancellationToken).ConfigureAwait(false);

    // session must be set here to send a subsequent login_device method via secure pass through
    session = securePassThroughSession;

    logger?.LogDebug("Handshake completed.");

    /*
     * login_device
     */
    logger?.LogDebug("Login starting: {EndPointUri}", endPointUri);

    var token = await LoginDeviceAsync(credentialProvider, cancellationToken).ConfigureAwait(false);

    logger?.LogDebug("Login completed, access token has issued: {Token}", token);

    /*
     * session established
     */
    securePassThroughSession.SetToken(token);

    logger?.LogDebug("Request path and query for the session: '{PathAndQuery}'", securePassThroughSession.RequestPathAndQuery);

    logger?.LogInformation(
      "Secure pass through session established: {SessionRequestPathAndQuery}; {SessionIDPrefix}{SessionID}; expires on {ExpiresOn}",
      securePassThroughSession.RequestPathAndQuery,
      TapoSessionCookieUtils.HttpCookiePrefixForSessionId,
      securePassThroughSession.SessionId,
      securePassThroughSession.ExpiresOn.ToString("o", provider: null)
    );
  }

  private async ValueTask<TapoSecurePassThroughSession> HandshakeAsync(
    ITapoCredentialIdentity? identity,
    CancellationToken cancellationToken
  )
  {
    const int KeyExchangeAlgorithmKeySizeInBytes = 128;

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
      (_, var response, (sessionId, sessionTimeout)) = await PostPlainTextRequestAsync<
        HandshakeRequest,
        HandshakeResponse,
        (string?, int?)
      >(
        requestPathAndQuery: TapoSecurePassThroughSession.RequestPath,
        request: new(key: publicKeyInfoPem),
        jsonSerializerOptions: CommonJsonSerializerOptions,
        processHttpResponseAsync: ParseSessionCookieAsync,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      if (response.Result.Key is null) {
        logger?.LogCritical("Could not exchange the key during handshaking.");
        throw new TapoAuthenticationException(
          message: $"Could not exchange the key during handshaking with the device at '{endPointUri}'.",
          endPoint: endPointUri
        );
      }

      base64EncryptedKey = response.Result.Key;
    }
    catch (TapoErrorResponseException ex) {
      logger?.LogCritical("Failed to handshake with error code {ErrorCode}.", ex.RawErrorCode);
      throw new TapoAuthenticationException(
        message: $"Failed to handshake with the device at '{endPointUri}' with error code {ex.RawErrorCode}.",
        endPoint: endPointUri,
        innerException: ex
      );
    }
    catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException exTimeout) {
      logger?.LogCritical("Failed to handshake due to timeout. ({ExceptionMessage})", ex.Message);
      throw new TapoAuthenticationException(
        message: $"Failed to handshake with the device at '{endPointUri}' due to timeout. ({ex.Message})",
        endPoint: endPointUri,
        innerException: exTimeout
      );
    }

    logger?.LogTrace("[Handshake] Exchanged key: {Key}", base64EncryptedKey);
    logger?.LogTrace("[Handshake] Session ID: {SessionId}", sessionId);
    logger?.LogTrace("[Handshake] Session timeout: {SessionTimeout}", sessionTimeout);

    var encryptedKey = Convert.FromBase64String(base64EncryptedKey);

    if (encryptedKey.Length != KeyExchangeAlgorithmKeySizeInBytes) {
      logger?.LogCritical("Exchanged an unexpected length of key");
      throw new TapoAuthenticationException(
        message: $"Exchanged an unexpected length of key from the device at '{endPointUri}'.",
        endPoint: endPointUri
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
      identity: identity,
      sessionId: sessionId,
      expiresOn: expiresOn,
      key: keyBytes.AsSpan(0, 16),
      iv: keyBytes.AsSpan(16, 16),
      baseJsonSerializerOptions: CommonJsonSerializerOptions,
      logger: logger
    );
  }

  private async ValueTask<string> LoginDeviceAsync(
    ITapoCredentialProvider credential,
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();

    string token;

    try {
      var loginDeviceResponse = await SendRequestAsync<LoginDeviceRequest, LoginDeviceResponse>(
        request: new(credential: credential),
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      token = loginDeviceResponse.Result.Token;
    }
    catch (TapoErrorResponseException ex) {
      throw new TapoAuthenticationException(
        message: ex.RawErrorCode switch {
          TapoErrorCodes.InvalidCredentials => $"Failed to initiate authorized session with the device at '{endPointUri}'. Credentials may be invalid, check your username and password.",
          _ => $"Denied to initiate authorized session with the device at '{endPointUri}'. (error code: {ex.RawErrorCode})",
        },
        endPoint: endPointUri,
        innerException: ex
      );
    }
    catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException exTimeout) {
      logger?.LogCritical("Failed to initiate authorized session due to timeout. ({ExceptionMessage})", ex.Message);
      throw new TapoAuthenticationException(
        message: $"Failed to initiate authorized session with the device at '{endPointUri}' due to timeout. ({ex.Message})",
        endPoint: endPointUri,
        innerException: exTimeout
      );
    }

    if (string.IsNullOrEmpty(token)) {
      logger?.LogError("Access token has not been issued.");
      throw new TapoAuthenticationException(
        message: $"An access token was not issued from the device at '{endPointUri}'.",
        endPoint: endPointUri
      );
    }

    return token;
  }
}
