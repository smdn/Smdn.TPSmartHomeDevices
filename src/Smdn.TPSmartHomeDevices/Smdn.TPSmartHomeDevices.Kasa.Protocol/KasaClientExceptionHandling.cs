// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.TPSmartHomeDevices.Kasa.Protocol;

public readonly struct KasaClientExceptionHandling {
  public static readonly KasaClientExceptionHandling Throw = default;
  public static readonly KasaClientExceptionHandling InvalidateEndPointAndThrow = new() { ShouldInvalidateEndPoint = true };

  public static readonly KasaClientExceptionHandling Retry = new() { ShouldRetry = true };
  public static readonly KasaClientExceptionHandling RetryAfterReconnect = new() { ShouldRetry = true, ShouldReconnect = true };
  public static readonly KasaClientExceptionHandling InvalidateEndPointAndRetry = new() { ShouldRetry = true, ShouldInvalidateEndPoint = true, };

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
