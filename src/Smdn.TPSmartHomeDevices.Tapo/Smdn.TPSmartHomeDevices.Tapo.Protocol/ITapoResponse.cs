// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

public interface ITapoResponse {
  /// <summary>Gets a value of response error code.</summary>
  /// <remarks>For types implementing this interface, the following attributes must be specified for this member: <c>[JsonPropertyName("error_code")]</c>.</remarks>
  int ErrorCode { get; }
}
