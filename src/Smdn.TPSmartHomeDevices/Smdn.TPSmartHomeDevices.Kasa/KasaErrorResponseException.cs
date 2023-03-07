// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using Smdn.TPSmartHomeDevices.Kasa.Protocol;

namespace Smdn.TPSmartHomeDevices.Kasa;

public class KasaErrorResponseException : KasaUnexpectedResponseException {
  public ErrorCode ErrorCode { get; }

  public KasaErrorResponseException(
    EndPoint deviceEndPoint,
    string requestModule,
    string requestMethod,
    ErrorCode errorCode
  )
    : base(
      message: $"Request '{requestModule}:{requestMethod}' failed with error code {(int)errorCode}. (Device end point: {deviceEndPoint})",
      deviceEndPoint: deviceEndPoint,
      requestModule: requestModule,
      requestMethod: requestMethod,
      innerException: null
    )
  {
    ErrorCode = errorCode;
  }
}
