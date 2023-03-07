// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.TPSmartHomeDevices.Tapo;

public abstract class TapoProtocolException : InvalidOperationException {
  public Uri EndPoint { get; }

  protected TapoProtocolException(
    string message,
    Uri endPoint,
    Exception? innerException
  )
    : base(
      message: message,
      innerException: innerException
    )
  {
    EndPoint = endPoint;
  }
}
