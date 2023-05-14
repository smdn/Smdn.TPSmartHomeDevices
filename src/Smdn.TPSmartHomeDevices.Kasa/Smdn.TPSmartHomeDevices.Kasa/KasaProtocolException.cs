// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1032

using System;
using System.Net;

namespace Smdn.TPSmartHomeDevices.Kasa;

/// <summary>
/// The exception that is thrown when the communication with the Kasa device encounters an unrecoverable condition.
/// </summary>
public abstract class KasaProtocolException : InvalidOperationException {
  /// <summary>
  /// Gets the <see cref="EndPoint"/> of the Kasa device that caused the exception.
  /// </summary>
  public EndPoint DeviceEndPoint { get; }

  protected KasaProtocolException(
    string message,
    EndPoint deviceEndPoint,
    Exception? innerException
  )
    : base(
      message: message,
      innerException: innerException
    )
  {
    DeviceEndPoint = deviceEndPoint;
  }
}
