// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices.Tapo;

public interface ITapoCredentialProvider {
  ITapoCredential GetCredential(string host);
}
