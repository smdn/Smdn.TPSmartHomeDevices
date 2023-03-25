// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices.Tapo.Credentials;

public interface ITapoCredentialProvider {
  ITapoCredential GetCredential(ITapoCredentialIdentity? identity);
}
