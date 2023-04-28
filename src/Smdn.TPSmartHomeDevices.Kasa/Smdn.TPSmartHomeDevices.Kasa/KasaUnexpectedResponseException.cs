// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;

namespace Smdn.TPSmartHomeDevices.Kasa;

/// <summary>
/// The exception that is thrown when the Kasa device responds a unexpected response.
/// </summary>
public class KasaUnexpectedResponseException : KasaProtocolException {
  /// <summary>
  /// Gets the <c>module</c> of the request that caused the exception.
  /// </summary>
  public string RequestModule { get; }

  /// <summary>
  /// Gets the <c>method</c> of the request that caused the exception.
  /// </summary>
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
