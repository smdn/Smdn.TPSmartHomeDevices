// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;

namespace Smdn.TPSmartHomeDevices.Kasa;

public class KasaDisconnectedException : KasaProtocolException {
  public KasaDisconnectedException(
    string message,
    EndPoint deviceEndPoint,
    Exception? innerException
  )
    : base(
      message: message,
      deviceEndPoint: deviceEndPoint,
      innerException: innerException
    )
  {
  }
}
