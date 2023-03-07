// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

internal class PlainTextCredentialProvider : ITapoCredentialProvider {
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
