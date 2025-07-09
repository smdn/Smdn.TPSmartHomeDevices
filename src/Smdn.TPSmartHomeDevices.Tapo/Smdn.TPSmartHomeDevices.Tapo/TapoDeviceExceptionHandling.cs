// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1815 // TODO: implement equality comparison

using System;

namespace Smdn.TPSmartHomeDevices.Tapo;

/// <summary>
/// The type that defines the action of handling exceptions that occur during communication with Tapo devices.
/// </summary>
public readonly struct TapoDeviceExceptionHandling {
  /// <summary>
  /// Throws the exception as it is occured without any other actions.
  /// </summary>
  public static readonly TapoDeviceExceptionHandling Throw = default;

  /// <summary>
  /// Wraps the occured exception into <see cref="TapoProtocolException"/> and throw it.
  /// </summary>
  /// <seealso cref="TapoProtocolException"/>
  public static readonly TapoDeviceExceptionHandling ThrowAsTapoProtocolException = new() { ShouldWrapIntoTapoProtocolException = true };

  /// <summary>
  /// Invokes <see cref="IDynamicDeviceEndPoint.Invalidate"/> and throws the occured exception.
  /// </summary>
  /// <seealso cref="IDynamicDeviceEndPoint.Invalidate"/>
  public static readonly TapoDeviceExceptionHandling InvalidateEndPointAndThrow = new() { ShouldInvalidateEndPoint = true };

  /// <summary>
  /// Ignores the occured exception and try again immediately.
  /// </summary>
  public static readonly TapoDeviceExceptionHandling Retry = new() { ShouldRetry = true };

  /// <summary>
  /// Ignores the occured exception and try again after re-establishing session immediately.
  /// </summary>
  public static readonly TapoDeviceExceptionHandling RetryAfterReestablishSession = new() { ShouldRetry = true, ShouldReestablishSession = true };

  /// <summary>
  /// Ignores the occured exception, invokes <see cref="IDynamicDeviceEndPoint.Invalidate"/> and try again immediately.
  /// </summary>
  /// <seealso cref="IDynamicDeviceEndPoint.Invalidate"/>
  public static readonly TapoDeviceExceptionHandling InvalidateEndPointAndRetry = new() { ShouldRetry = true, ShouldInvalidateEndPoint = true, };

  /// <summary>
  /// Creates a <see cref="TapoDeviceExceptionHandling"/> that ignores the occured exception and try again after the specified period of time.
  /// </summary>
  /// <param name="retryAfter">The <see cref="TimeSpan"/> that specifies the amount of time to wait before retry.</param>
  /// <param name="shouldReestablishSession">
  /// The <see cref="bool"/> value that specifies whether the session should be re-established before retry or not.
  /// </param>
  public static TapoDeviceExceptionHandling CreateRetry(
    TimeSpan retryAfter,
    bool shouldReestablishSession = false
  )
    => new() {
      ShouldRetry = true,
      RetryAfter = retryAfter,
      ShouldReestablishSession = shouldReestablishSession,
    };

  public bool ShouldRetry { get; init; }
  public TimeSpan RetryAfter { get; init; }
  public bool ShouldReestablishSession { get; init; }
  public bool ShouldWrapIntoTapoProtocolException { get; init; }
  public bool ShouldInvalidateEndPoint { get; init; }

  public override string ToString() => $"{{{nameof(ShouldRetry)}={ShouldRetry}, {nameof(RetryAfter)}={RetryAfter}, {nameof(ShouldReestablishSession)}={ShouldReestablishSession}, {nameof(ShouldWrapIntoTapoProtocolException)}={ShouldWrapIntoTapoProtocolException}, {nameof(ShouldInvalidateEndPoint)}={ShouldInvalidateEndPoint}}}";
}
