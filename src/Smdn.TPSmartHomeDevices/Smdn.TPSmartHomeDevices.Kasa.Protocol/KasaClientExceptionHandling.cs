// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.TPSmartHomeDevices.Kasa.Protocol;

/// <summary>
/// The type that defines the action of handling exceptions that occur during communication with Kasa devices.
/// </summary>
public readonly struct KasaClientExceptionHandling {
  /// <summary>
  /// Throws the exception as it is occured without any other actions.
  /// </summary>
  public static readonly KasaClientExceptionHandling Throw = default;

  /// <summary>
  /// Invokes <see cref="IDynamicDeviceEndPoint.Invalidate"/> and throws the occured exception.
  /// </summary>
  /// <seealso cref="IDynamicDeviceEndPoint.Invalidate"/>
  public static readonly KasaClientExceptionHandling InvalidateEndPointAndThrow = new() { ShouldInvalidateEndPoint = true };

  /// <summary>
  /// Ignores the occured exception and try again immediately.
  /// </summary>
  public static readonly KasaClientExceptionHandling Retry = new() { ShouldRetry = true };

  /// <summary>
  /// Ignores the occured exception and try again after reconnect immediately.
  /// </summary>
  public static readonly KasaClientExceptionHandling RetryAfterReconnect = new() { ShouldRetry = true, ShouldReconnect = true };

  /// <summary>
  /// Ignores the occured exception, invokes <see cref="IDynamicDeviceEndPoint.Invalidate"/> and try again immediately.
  /// </summary>
  /// <seealso cref="IDynamicDeviceEndPoint.Invalidate"/>
  public static readonly KasaClientExceptionHandling InvalidateEndPointAndRetry = new() { ShouldRetry = true, ShouldInvalidateEndPoint = true, };

  /// <summary>
  /// Creates a <see cref="KasaClientExceptionHandling"/> that ignores the occured exception and try again after the specified period of time.
  /// </summary>
  /// <param name="retryAfter">The <see cref="TimeSpan"/> that specifies the amount of time to wait before retry.</param>
  /// <param name="shouldReconnect">
  /// The <see cref="bool"/> value that specifies whether the
  /// <see cref="IDynamicDeviceEndPoint.Invalidate"/> should be invoked before retry or not.
  /// </param>
  public static KasaClientExceptionHandling CreateRetry(
    TimeSpan retryAfter,
    bool shouldReconnect = false
  )
    => new() {
      ShouldRetry = true,
      RetryAfter = retryAfter,
      ShouldReconnect = shouldReconnect,
    };

  public bool ShouldRetry { get; init; }
  public TimeSpan RetryAfter { get; init; }
  public bool ShouldReconnect { get; init; }
  public bool ShouldInvalidateEndPoint { get; init; }

  public override string ToString() => $"{{{nameof(ShouldRetry)}={ShouldRetry}, {nameof(RetryAfter)}={RetryAfter}, {nameof(ShouldReconnect)}={ShouldReconnect}, {nameof(ShouldInvalidateEndPoint)}={ShouldInvalidateEndPoint}}}";
}
