// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;

namespace Smdn.TPSmartHomeDevices.Kasa;

/// <summary>
/// The exception that is thrown when the Kasa device responds an incomplete response.
/// </summary>
/// <remarks>
/// The Kasa devices indicates the its length at the beginning of response, but may actually send a response less than that length and stop responding.
/// <see cref="KasaIncompleteResponseException"/> is thrown when such a response is received.
/// </remarks>
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
