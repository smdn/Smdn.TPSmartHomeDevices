// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: GPL-3.0-or-later
#pragma warning disable CA1815 // TODO: implement equality comparison

using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// The type that reflects <c>get_device_usage</c> JSON request.
/// </summary>
/// <remarks>
/// This implementation is based on and ported from the following
/// Python implementation by <see href="https://github.com/petretiandrea">petretiandrea</see>:
/// <see href="https://github.com/petretiandrea/plugp100">petretiandrea/plugp100</see>, published under the GPL-3.0 license,
/// forked from <see href="https://github.com/K4CZP3R/tapo-p100-python">K4CZP3R/tapo-p100-python</see>.
/// </remarks>
public readonly struct GetDeviceUsageRequest : ITapoPassThroughRequest {
  [JsonPropertyName("method")]
  [JsonPropertyOrder(0)]
  public string Method => "get_device_usage";

#pragma warning disable CA1822
  [JsonPropertyName("requestTimeMils")]
  public long RequestTimeMilliseconds => 0L; // DateTimeOffset.Now.ToUnixTimeMilliseconds();
#pragma warning restore CA1822
}
