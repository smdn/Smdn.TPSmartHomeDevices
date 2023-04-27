// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Kasa.Protocol;

/// <summary>
/// Provides a mechanism for determining the action of handling exceptions that occur during communication with Kasa devices.
/// </summary>
/// <seealso cref="KasaClientExceptionHandling"/>
public abstract class KasaClientExceptionHandler {
  /// <summary>
  /// A <see cref="KasaClientExceptionHandler"/> defining the default exception handling.
  /// </summary>
  protected internal static readonly KasaClientExceptionHandler Default = new KasaClientDefaultExceptionHandler();

  /// <summary>
  /// Determines the action of handling exceptions that occur when communicating with Kasa devices.
  /// </summary>
  /// <remarks>
  /// Do not throw the new exception or rethrow <paramref name="exception"/> from this method.
  /// </remarks>
  /// <param name="device">The <see cref="KasaDevice"/> that caused the thrown exception.</param>
  /// <param name="exception">The <see cref="Exception"/> thrown by <paramref name="device"/>.</param>
  /// <param name="attempt">
  /// The current number of attempts when a retry is selected to handle an exception.
  /// <c>0</c> indicates an initial attempt.
  /// </param>
  /// <param name="logger">The <see cref="ILogger"/> to report the situation.</param>
  /// <returns>
  /// The <see cref="KasaClientExceptionHandling"/> that defines the action to handle the <paramref name="exception"/>, determined by <paramref name="exception"/> and <paramref name="attempt"/>.
  /// </returns>
  /// <seealso cref="KasaClientExceptionHandling"/>
  public abstract KasaClientExceptionHandling DetermineHandling(
    KasaDevice device,
    Exception exception,
    int attempt,
    ILogger? logger
  );
}
