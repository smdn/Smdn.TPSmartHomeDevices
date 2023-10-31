// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Security.Cryptography;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// Maintains authenticated Tapo session information, including access token, exchanged key and session ID.
/// </summary>
/// <remarks>
/// This implementation is based on the following
/// C# implementation by <see href="https://github.com/europowergenerators">E-Power International</see>:
/// <see href="https://github.com/europowergenerators/Tapo-plug-controller">europowergenerators/Tapo-plug-controller</see>, published under the MIT License.
/// </remarks>
internal sealed class TapoSecurePassThroughSession : TapoSession {
  internal static readonly Uri RequestPath = new("/app", UriKind.Relative);

  public Uri RequestPathAndQuery { get; private set; } = RequestPath;
  public override string? Token => token;

  private string? token;
  private Aes? aes;
  private SecurePassThroughJsonConverterFactory? securePassThroughJsonConverterFactory;

  internal JsonSerializerOptions SecurePassThroughJsonSerializerOptions { get; }

  internal TapoSecurePassThroughSession(
    ITapoCredentialIdentity? identity,
    string? sessionId,
    DateTime expiresOn,
    ReadOnlySpan<byte> key,
    ReadOnlySpan<byte> iv,
    JsonSerializerOptions baseJsonSerializerOptions,
    ILogger? logger
  )
    : base(
      sessionId,
      expiresOn
    )
  {
    aes = Aes.Create();
    aes.Mode = CipherMode.CBC;
    aes.Padding = PaddingMode.PKCS7;
    aes.Key = key.ToArray();
    aes.IV = iv.ToArray();

    securePassThroughJsonConverterFactory = new(
      identity: identity,
      encryptorForPassThroughRequest: aes.CreateEncryptor(),
      decryptorForPassThroughResponse: aes.CreateDecryptor(),
      baseJsonSerializerOptionsForPassThroughMessage: baseJsonSerializerOptions,
      logger: logger
    );

    SecurePassThroughJsonSerializerOptions = new(baseJsonSerializerOptions);
    SecurePassThroughJsonSerializerOptions.Converters.Add(securePassThroughJsonConverterFactory);
  }

  protected override void Dispose(bool disposing)
  {
    if (!disposing) {
      securePassThroughJsonConverterFactory?.Dispose();
      securePassThroughJsonConverterFactory = null;

      aes?.Dispose();
      aes = null;
    }

    base.Dispose(disposing);
  }

  internal void SetToken(string token)
  {
    this.token = token;

    // append issued token to the request path query
    RequestPathAndQuery = new Uri(
      string.Concat(
        RequestPath.ToString(), // only path
        "?token=",
        token
      ),
      UriKind.Relative
    );
  }
}
