// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

// The situation represented by the following error codes are not well-confirmed.
// Therefore, these error codes cannot be made public APIs.
internal static class TapoErrorCodes {
  public const int Success = 0;
  public const int InvalidCredentials = -1501;
  public const int DeviceBusy = -1301;
  public const int RequestParameterError = -1008;
  // -1012: ???
}
