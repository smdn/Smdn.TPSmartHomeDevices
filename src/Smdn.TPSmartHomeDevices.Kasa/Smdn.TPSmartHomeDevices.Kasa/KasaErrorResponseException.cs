// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1032

using System.Net;

namespace Smdn.TPSmartHomeDevices.Kasa;

/// <summary>
/// The exception that is thrown when the Kasa device responds a response with an error code.
/// </summary>
public class KasaErrorResponseException : KasaUnexpectedResponseException {
  /// <summary>
  /// Gets the <c>error_code</c> of the response that caused the exception.
  /// </summary>
  public int RawErrorCode { get; }

  public KasaErrorResponseException(
    EndPoint deviceEndPoint,
    string requestModule,
    string requestMethod,
    int rawErrorCode
  )
    : base(
      message: $"Request '{requestModule}:{requestMethod}' failed with error code {rawErrorCode}. (Device end point: {deviceEndPoint})",
      deviceEndPoint: deviceEndPoint,
      requestModule: requestModule,
      requestMethod: requestMethod,
      innerException: null
    )
  {
    RawErrorCode = rawErrorCode;
  }
}
