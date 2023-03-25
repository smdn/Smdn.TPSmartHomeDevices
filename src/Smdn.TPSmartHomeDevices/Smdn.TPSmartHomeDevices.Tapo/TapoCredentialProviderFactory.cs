// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using System.Text.Json;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

internal static class TapoCredentialProviderFactory {
  public static ITapoCredentialProvider CreateFromPlainText(string email, string password)
    => new SingleIdentityStringCredentialProvider(
      username: email ?? throw new ArgumentNullException(nameof(email)),
      password: password ?? throw new ArgumentNullException(nameof(password)),
      isPlainText: true
    );

  public static ITapoCredentialProvider CreateFromBase64EncodedText(
    string base64UserNameSHA1Digest,
    string base64Password
  )
    => new SingleIdentityStringCredentialProvider(
      username: base64UserNameSHA1Digest ?? throw new ArgumentNullException(nameof(base64UserNameSHA1Digest)),
      password: base64Password ?? throw new ArgumentNullException(nameof(base64Password)),
      isPlainText: false
    );

  private sealed class SingleIdentityStringCredentialProvider : ITapoCredentialProvider, ITapoCredential {
    private readonly byte[] utf8Username;
    private readonly byte[] utf8Password;
    private readonly bool isPlainText;

    public SingleIdentityStringCredentialProvider(
      string username,
      string password,
      bool isPlainText
    )
    {
      utf8Username = Encoding.UTF8.GetBytes(username);
      utf8Password = Encoding.UTF8.GetBytes(password);
      this.isPlainText = isPlainText;
    }

    ITapoCredential ITapoCredentialProvider.GetCredential(ITapoCredentialIdentity? identity) => this;

    void IDisposable.Dispose() { /* nothing to do */ }

    void ITapoCredential.WritePasswordPropertyValue(Utf8JsonWriter writer)
    {
      if (isPlainText)
        writer.WriteBase64StringValue(utf8Password);
      else
        writer.WriteStringValue(utf8Password);
    }

    void ITapoCredential.WriteUsernamePropertyValue(Utf8JsonWriter writer)
    {
      if (isPlainText) {
        Span<byte> buffer = stackalloc byte[TapoCredentialUtils.HexSHA1HashSizeInBytes];

        try {
          if (!TapoCredentialUtils.TryConvertToHexSHA1Hash(utf8Username, buffer, out _))
            throw new InvalidOperationException("failed to encode username property");

          writer.WriteBase64StringValue(buffer);
        }
        finally {
          buffer.Clear();
        }
      }
      else {
        writer.WriteStringValue(utf8Username);
      }
    }
  }
}
