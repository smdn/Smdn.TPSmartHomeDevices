// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.TPSmartHomeDevices.Tapo;

public class TapoAuthenticationException : TapoProtocolException {
  public TapoAuthenticationException(
    string message,
    Uri endPoint,
    Exception? innerException = null
  )
    : base(
      message: message,
      endPoint: endPoint,
      innerException: innerException
    )
  {
  }
}
