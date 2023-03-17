// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

internal sealed class PlainTextCredentialProvider : ITapoCredentialProvider {
  public static ITapoCredentialProvider Create(string email, string password)
    => new PlainTextCredentialProvider(
      userName: email ?? throw new ArgumentNullException(nameof(email)),
      password: password ?? throw new ArgumentNullException(nameof(password))
    );

  private readonly string userName;
  private readonly string password;

  private PlainTextCredentialProvider(
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
