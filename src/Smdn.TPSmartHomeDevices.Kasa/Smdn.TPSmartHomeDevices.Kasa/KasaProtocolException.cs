// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;

namespace Smdn.TPSmartHomeDevices.Kasa;

public abstract class KasaProtocolException : InvalidOperationException {
  public EndPoint DeviceEndPoint { get; }

  protected KasaProtocolException(
    string message,
    EndPoint deviceEndPoint,
    Exception? innerException
  )
    : base(
      message: message,
      innerException: innerException
    )
  {
    DeviceEndPoint = deviceEndPoint;
  }
}
