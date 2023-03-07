// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices.Tapo;

public interface ITapoCredentialProvider {
  string GetBase64EncodedUserNameSHA1Digest(string host);
  string GetBase64EncodedPassword(string host);
}
