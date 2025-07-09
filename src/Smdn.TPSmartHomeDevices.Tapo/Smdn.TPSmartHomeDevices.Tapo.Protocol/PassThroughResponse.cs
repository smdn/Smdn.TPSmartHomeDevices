// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1815 // TODO: implement equality comparison

using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// The type that reflects JSON response common to the result property encapsulated
/// in a response for 'method: securePassthrough'.
/// </summary>
/// <typeparam name="TResult">A type that will be deserialized from the value of the <c>result</c> JSON property.</typeparam>
public readonly struct PassThroughResponse<TResult> : ITapoPassThroughResponse {
  [JsonPropertyName("error_code")]
  public int ErrorCode { get; init; }

  [JsonPropertyName("result")]
  public TResult Result { get; init; }
}
