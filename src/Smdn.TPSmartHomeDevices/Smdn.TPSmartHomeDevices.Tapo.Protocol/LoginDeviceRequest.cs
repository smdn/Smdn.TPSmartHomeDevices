// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Text.Json.Serialization;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

public readonly struct LoginDeviceRequest : ITapoPassThroughRequest {
  [JsonPropertyName("method")]
  [JsonPropertyOrder(0)]
  public string Method => "login_device";

  [JsonPropertyName("params")]
  public RequestParameters Parameters { get; }

#pragma warning disable CA1822
  [JsonPropertyName("requestTimeMils")]
  public long RequestTimeMilliseconds => 0L; // DateTimeOffset.Now.ToUnixTimeMilliseconds();
#pragma warning restore CA1822

  public LoginDeviceRequest(string password, string userName)
  {
    Parameters = new(password, userName);
  }

  public readonly record struct RequestParameters(
#pragma warning disable SA1313
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("username")] string UserName
#pragma warning restore SA1313
  );
}
