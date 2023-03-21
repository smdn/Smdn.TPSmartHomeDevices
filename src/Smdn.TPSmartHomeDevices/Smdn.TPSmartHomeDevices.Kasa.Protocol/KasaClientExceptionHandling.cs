// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices.Kasa.Protocol;

public enum KasaClientExceptionHandling {
  Throw = default,
  ThrowAndInvalidateEndPoint = 1,

  Retry = 100,
  RetryAfterReconnect = 101,
  RetryAfterResolveEndPoint = 102,
}
