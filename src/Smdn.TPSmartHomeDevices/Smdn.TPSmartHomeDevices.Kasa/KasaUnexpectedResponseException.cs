// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;

namespace Smdn.TPSmartHomeDevices.Kasa;

public class KasaUnexpectedResponseException : KasaProtocolException {
  public string RequestModule { get; }
  public string RequestMethod { get; }

  public KasaUnexpectedResponseException(
    string message,
    EndPoint deviceEndPoint,
    string requestModule,
    string requestMethod,
    Exception? innerException
  )
    : base(
      message: message,
      deviceEndPoint: deviceEndPoint,
      innerException: innerException
    )
  {
    RequestModule = requestModule;
    RequestMethod = requestMethod;
  }
}
