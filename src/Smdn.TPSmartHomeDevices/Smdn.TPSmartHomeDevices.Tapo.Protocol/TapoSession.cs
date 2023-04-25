// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <remarks>
/// This implementation is based on the following
/// C# implementation by <see href="https://github.com/europowergenerators">E-Power International</see>:
/// <see href="https://github.com/europowergenerators/Tapo-plug-controller">europowergenerators/Tapo-plug-controller</see>, published under the MIT License.
/// </remarks>
public sealed class TapoSession : IDisposable {
  internal static readonly Uri RequestPath = new("/app", UriKind.Relative);

  public Uri RequestPathAndQuery { get; private set; } = RequestPath;
  public string? Token { get; private set; }
  public string? SessionId { get; }
  public DateTime ExpiresOn { get; }
  public bool HasExpired => ExpiresOn <= DateTime.Now;

  private Aes? aes;
  private SecurePassThroughJsonConverterFactory? securePassThroughJsonConverterFactory;

  internal JsonSerializerOptions SecurePassThroughJsonSerializerOptions { get; }

  internal TapoSession(
    ITapoCredentialIdentity? identity,
    string? sessionId,
    DateTime expiresOn,
    ReadOnlySpan<byte> key,
    ReadOnlySpan<byte> iv,
    JsonSerializerOptions baseJsonSerializerOptions,
    ILogger? logger
  )
  {
    SessionId = sessionId;
    ExpiresOn = expiresOn;

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

  public void Dispose()
  {
    securePassThroughJsonConverterFactory?.Dispose();
    securePassThroughJsonConverterFactory = null;

    aes?.Dispose();
    aes = null;
  }

  internal void SetToken(string token)
  {
    Token = token;

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
