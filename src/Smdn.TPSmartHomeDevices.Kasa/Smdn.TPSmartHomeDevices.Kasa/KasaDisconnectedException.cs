// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;

namespace Smdn.TPSmartHomeDevices.Kasa;

/// <summary>
/// The exception that is thrown when the Kasa device disconnects the connection.
/// </summary>
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
