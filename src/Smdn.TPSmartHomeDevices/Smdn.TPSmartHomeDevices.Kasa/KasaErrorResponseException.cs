// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using Smdn.TPSmartHomeDevices.Kasa.Protocol;

namespace Smdn.TPSmartHomeDevices.Kasa;

public class KasaErrorResponseException : KasaUnexpectedResponseException {
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
