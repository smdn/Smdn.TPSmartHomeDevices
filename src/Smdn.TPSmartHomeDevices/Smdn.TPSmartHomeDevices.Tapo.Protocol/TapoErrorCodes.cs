// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

// The situation represented by the following error codes are not well-confirmed.
// Therefore, these error codes cannot be made public APIs.
internal static class TapoErrorCodes {
  public const ErrorCode DeviceBusy = (ErrorCode)(-1301);
  public const ErrorCode RequestParameterError = (ErrorCode)(-1008);
  // -1012: ???
}
