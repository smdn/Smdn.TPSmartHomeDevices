// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text.Json.Serialization;
using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// The type that reflects <c>login_device</c> JSON request.
/// </summary>
public readonly partial struct LoginDeviceRequest : ITapoPassThroughRequest {
  [JsonPropertyName("method")]
  [JsonPropertyOrder(0)]
  public string Method => "login_device";

  [JsonPropertyName("params")]
  public ITapoCredentialProvider Parameters { get; }

#pragma warning disable CA1822
  [JsonPropertyName("requestTimeMils")]
  public long RequestTimeMilliseconds => 0L; // DateTimeOffset.Now.ToUnixTimeMilliseconds();
#pragma warning restore CA1822

  public LoginDeviceRequest(ITapoCredentialProvider credential)
  {
    Parameters = credential ?? throw new ArgumentNullException(nameof(credential));
  }
}
