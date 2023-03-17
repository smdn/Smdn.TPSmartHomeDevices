// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

internal static class TapoCredentialProviderFactory {
  public static ITapoCredentialProvider CreateFromPlainText(string email, string password)
    => new PlainTextCredentialProvider(
      userName: email ?? throw new ArgumentNullException(nameof(email)),
      password: password ?? throw new ArgumentNullException(nameof(password))
    );

  public static ITapoCredentialProvider CreateFromBase64EncodedText(
    string base64UserNameSHA1Digest,
    string base64Password
  )
    => new Base64EncodedCredentialProvider(
      base64UserNameSHA1Digest: base64UserNameSHA1Digest ?? throw new ArgumentNullException(nameof(base64UserNameSHA1Digest)),
      base64Password: base64Password ?? throw new ArgumentNullException(nameof(base64Password))
    );

  private sealed class PlainTextCredentialProvider : ITapoCredentialProvider{
    private readonly string userName;
    private readonly string password;

    public PlainTextCredentialProvider(
      string userName,
      string password
    )
    {
      this.userName = userName;
      this.password = password;
    }

    public string GetBase64EncodedUserNameSHA1Digest(string host)
      => TapoCredentialUtils.ToBase64EncodedSHA1DigestString(userName);

    public string GetBase64EncodedPassword(string host)
      => TapoCredentialUtils.ToBase64EncodedString(password);
  }

  private sealed class Base64EncodedCredentialProvider : ITapoCredentialProvider {
    private readonly string base64UserNameSHA1Digest;
    private readonly string base64Password;

    public Base64EncodedCredentialProvider(
      string base64UserNameSHA1Digest,
      string base64Password
    )
    {
      this.base64UserNameSHA1Digest = base64UserNameSHA1Digest;
      this.base64Password = base64Password;
    }

    public string GetBase64EncodedUserNameSHA1Digest(string host) => base64UserNameSHA1Digest;
    public string GetBase64EncodedPassword(string host) => base64Password;
  }
}
