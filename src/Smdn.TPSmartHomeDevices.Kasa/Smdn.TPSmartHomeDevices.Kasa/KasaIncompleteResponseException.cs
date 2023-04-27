// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;

namespace Smdn.TPSmartHomeDevices.Kasa;

public class KasaIncompleteResponseException : KasaUnexpectedResponseException {
  public KasaIncompleteResponseException(
    string message,
    EndPoint deviceEndPoint,
    string requestModule,
    string requestMethod,
    Exception? innerException
  )
    : base(
      message: message,
      deviceEndPoint: deviceEndPoint,
      requestModule: requestModule,
      requestMethod: requestMethod,
      innerException: innerException
    )
  {
  }
}
