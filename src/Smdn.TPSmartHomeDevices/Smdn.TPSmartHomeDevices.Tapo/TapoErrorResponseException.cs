// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

public class TapoErrorResponseException : TapoProtocolException {
  internal static void ThrowIfError(Uri requestUri, string requestMethod, ErrorCode errorCode)
  {
    if (errorCode != ErrorCode.Success)
      throw new TapoErrorResponseException(requestUri, requestMethod, errorCode);
  }

  public string RequestMethod { get; }
  public ErrorCode ErrorCode { get; }

  public TapoErrorResponseException(
    Uri requestEndPoint,
    string requestMethod,
    ErrorCode errorCode
  )
    : base(
      message: $"Request '{requestMethod}' failed with error code {(int)errorCode}. (Request URI: {requestEndPoint})",
      endPoint: requestEndPoint,
      innerException: null
    )
  {
    RequestMethod = requestMethod;
    ErrorCode = errorCode;
  }
}
