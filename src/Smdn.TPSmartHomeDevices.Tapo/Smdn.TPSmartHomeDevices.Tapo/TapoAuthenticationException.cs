// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.TPSmartHomeDevices.Tapo;

/// <summary>
/// The exception that is thrown when an unrecoverable error occurs during the authentication to a Tapo device.
/// </summary>
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
