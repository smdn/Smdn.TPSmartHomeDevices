// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

public class TapoErrorResponseException : TapoProtocolException {
  internal static void ThrowIfError(Uri requestUri, string requestMethod, int errorCode)
  {
    if (errorCode != TapoErrorCodes.Success)
      throw new TapoErrorResponseException(requestUri, requestMethod, errorCode);
  }

  public string RequestMethod { get; }
  public int RawErrorCode { get; }

  public TapoErrorResponseException(
    Uri requestEndPoint,
    string requestMethod,
    int rawErrorCode
  )
    : base(
      message: $"Request '{requestMethod}' failed with error code {rawErrorCode}. (Request URI: {requestEndPoint})",
      endPoint: requestEndPoint,
      innerException: null
    )
  {
    RequestMethod = requestMethod;
    RawErrorCode = rawErrorCode;
  }
}
