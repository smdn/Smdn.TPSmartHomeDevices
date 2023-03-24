// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

public readonly struct TapoClientExceptionHandling {
  public static readonly TapoClientExceptionHandling Throw = default;
  public static readonly TapoClientExceptionHandling ThrowAsTapoProtocolException = new() { ShouldWrapIntoTapoProtocolException = true };
  public static readonly TapoClientExceptionHandling InvalidateEndPointAndThrow = new() { ShouldInvalidateEndPoint = true };

  public static readonly TapoClientExceptionHandling Retry = new() { ShouldRetry = true };
  public static readonly TapoClientExceptionHandling RetryAfterReconnect = new() { ShouldRetry = true, ShouldReconnect = true };
  public static readonly TapoClientExceptionHandling InvalidateEndPointAndRetry = new() { ShouldRetry = true, ShouldInvalidateEndPoint = true, };

  public static TapoClientExceptionHandling CreateRetry(
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
  public bool ShouldWrapIntoTapoProtocolException { get; init; }
  public bool ShouldInvalidateEndPoint { get; init; }

  public override string ToString() => $"{{{nameof(ShouldRetry)}={ShouldRetry}, {nameof(RetryAfter)}={RetryAfter}, {nameof(ShouldReconnect)}={ShouldReconnect}, {nameof(ShouldWrapIntoTapoProtocolException)}={ShouldWrapIntoTapoProtocolException}, {nameof(ShouldInvalidateEndPoint)}={ShouldInvalidateEndPoint}}}";
}
