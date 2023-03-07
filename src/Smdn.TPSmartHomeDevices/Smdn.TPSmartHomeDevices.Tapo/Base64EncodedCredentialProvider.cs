// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.TPSmartHomeDevices.Tapo;

internal class Base64EncodedCredentialProvider : ITapoCredentialProvider {
  private readonly string base64UserNameSHA1Digest;
  private readonly string base64Password;

  public Base64EncodedCredentialProvider(
    string base64UserNameSHA1Digest,
    string base64Password
  )
  {
    this.base64UserNameSHA1Digest = base64UserNameSHA1Digest ?? throw new ArgumentNullException(nameof(base64UserNameSHA1Digest));
    this.base64Password = base64Password ?? throw new ArgumentNullException(nameof(base64Password));
  }

  public string GetBase64EncodedUserNameSHA1Digest(string host) => base64UserNameSHA1Digest;
  public string GetBase64EncodedPassword(string host) => base64Password;
}
