// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

public class SecurePassThroughInvalidPaddingException : SystemException {
  public SecurePassThroughInvalidPaddingException(
    string message,
    Exception? innerException
  )
    : base(
      message: message,
      innerException: innerException
    )
  {
  }
}
