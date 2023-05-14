// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1032

using System;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

/// <summary>
/// The exception that is thrown when the Tapo device responds a response with an error code.
/// </summary>
public class TapoErrorResponseException : TapoProtocolException {
  internal static void ThrowIfError(Uri requestUri, string requestMethod, int errorCode)
  {
    if (errorCode == TapoErrorCodes.Success)
      return;

    throw new TapoErrorResponseException(
      requestUri,
      requestMethod,
      errorCode
    );
  }

  private static string GetMessage(
    Uri requestEndPoint,
    string requestMethod,
    int rawErrorCode
  )
  {
    var informationalErrorMessage = rawErrorCode switch {
      // make sure to include a space at the beginning of the informatonal message
      TapoErrorCodes.DeviceBusy => " Device may be busy. Retry after a few moments.",
      TapoErrorCodes.InvalidCredentials => " Credentials may be invalid. Check your username and password.",
      TapoErrorCodes.RequestParameterError => " It may be an error in the request parameters. It is possible that the value may be out of range, etc.",
      _ => null,
    };

    return $"Request '{requestMethod}' failed with error code {rawErrorCode}.{informationalErrorMessage} (Request URI: {requestEndPoint})";
  }

  /// <summary>
  /// Gets the <c>method</c> of the request that caused the exception.
  /// </summary>
  public string RequestMethod { get; }

  /// <summary>
  /// Gets the <c>error_code</c> of the response that caused the exception.
  /// </summary>
  public int RawErrorCode { get; }

  public TapoErrorResponseException(
    Uri requestEndPoint,
    string requestMethod,
    int rawErrorCode
  )
    : base(
      message: GetMessage(requestEndPoint, requestMethod, rawErrorCode),
      endPoint: requestEndPoint,
      innerException: null
    )
  {
    RequestMethod = requestMethod;
    RawErrorCode = rawErrorCode;
  }
}
