// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

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
    string? sessionId,
    DateTime expiresOn,
    ReadOnlySpan<byte> key,
    ReadOnlySpan<byte> iv,
    JsonSerializerOptions plainTextJsonSerializerOptions,
    IServiceProvider? serviceProvider
  )
  {
    var logger = serviceProvider?.GetService<ILoggerFactory>()?.CreateLogger($"{nameof(TapoSession)}({sessionId})");

    SessionId = sessionId;
    ExpiresOn = expiresOn;

    aes = Aes.Create();
    aes.Padding = PaddingMode.PKCS7;
    aes.Key = key.ToArray();
    aes.IV = iv.ToArray();

    securePassThroughJsonConverterFactory = new(
      encryptorForPassThroughRequest: aes.CreateEncryptor(),
      decryptorForPassThroughResponse: aes.CreateDecryptor(),
      plainTextJsonSerializerOptions: plainTextJsonSerializerOptions,
      logger: logger
    );

    SecurePassThroughJsonSerializerOptions = new(plainTextJsonSerializerOptions);
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
