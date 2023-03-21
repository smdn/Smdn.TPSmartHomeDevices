// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

public enum TapoClientExceptionHandling {
  Throw = default,
  ThrowAndInvalidateEndPoint = 1,
  ThrowWrapTapoProtocolException = 2,

  Retry = 100,
  RetryAfterReconnect = 101,
  RetryAfterReestablishSession = 102,
  RetryAfterResolveEndPoint = 103,
}
