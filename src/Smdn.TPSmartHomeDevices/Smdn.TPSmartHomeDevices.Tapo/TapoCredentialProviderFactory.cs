// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using System.Text.Json;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

internal static class TapoCredentialProviderFactory {
  public static ITapoCredentialProvider CreateFromPlainText(string email, string password)
    => new StringCredentialProvider(
      username: email ?? throw new ArgumentNullException(nameof(email)),
      password: password ?? throw new ArgumentNullException(nameof(password)),
      isPlainText: true
    );

  public static ITapoCredentialProvider CreateFromBase64EncodedText(
    string base64UserNameSHA1Digest,
    string base64Password
  )
    => new StringCredentialProvider(
      username: base64UserNameSHA1Digest ?? throw new ArgumentNullException(nameof(base64UserNameSHA1Digest)),
      password: base64Password ?? throw new ArgumentNullException(nameof(base64Password)),
      isPlainText: false
    );

  private sealed class StringCredentialProvider : ITapoCredentialProvider {
    private readonly byte[] utf8Username;
    private readonly byte[] utf8Password;
    private readonly bool isPlainText;

    public StringCredentialProvider(
      string username,
      string password,
      bool isPlainText
    )
    {
      utf8Username = Encoding.UTF8.GetBytes(username);
      utf8Password = Encoding.UTF8.GetBytes(password);
      this.isPlainText = isPlainText;
    }

    public ITapoCredential GetCredential(string host) => new Credential(this);

    private readonly struct Credential : ITapoCredential {
      private readonly StringCredentialProvider provider;

      public Credential(StringCredentialProvider provider)
      {
        this.provider = provider;
      }

      public void Dispose() { /* nothing to do */ }

      public void WritePasswordPropertyValue(Utf8JsonWriter writer)
      {
        if (provider.isPlainText)
          writer.WriteBase64StringValue(provider.utf8Password);
        else
          writer.WriteStringValue(provider.utf8Password);
      }

      public void WriteUsernamePropertyValue(Utf8JsonWriter writer)
      {
        if (provider.isPlainText) {
          Span<byte> buffer = stackalloc byte[TapoCredentialUtils.HexSHA1HashSizeInBytes];

          try {
            if (!TapoCredentialUtils.TryConvertToHexSHA1Hash(provider.utf8Username, buffer, out _))
              throw new InvalidOperationException("failed to encode username property");

            writer.WriteBase64StringValue(buffer);
          }
          finally {
            buffer.Clear();
          }
        }
        else {
          writer.WriteStringValue(provider.utf8Username);
        }
      }
    }
  }
}
