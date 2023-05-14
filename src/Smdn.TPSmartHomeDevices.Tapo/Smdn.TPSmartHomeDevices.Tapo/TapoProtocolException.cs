// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1032

using System;

namespace Smdn.TPSmartHomeDevices.Tapo;

/// <summary>
/// The exception that is thrown when the communication with the Tapo device encounters an unrecoverable condition.
/// </summary>
public class TapoProtocolException : InvalidOperationException {
  /// <summary>
  /// Gets the <see cref="EndPoint"/> of the Tapo device that caused the exception.
  /// </summary>
  public Uri EndPoint { get; }

  protected internal TapoProtocolException(
    string message,
    Uri endPoint,
    Exception? innerException
  )
    : base(
      message: message,
      innerException: innerException
    )
  {
    EndPoint = endPoint;
  }
}
