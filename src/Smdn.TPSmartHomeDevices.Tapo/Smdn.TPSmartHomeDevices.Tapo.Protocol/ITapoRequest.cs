// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

public interface ITapoRequest {
  /// <summary>Gets a string value that represents the 'method' for this request.</summary>
  /// <remarks>For types implementing this interface, the following attributes must be specified for this member: <c>[JsonPropertyName("method"), JsonPropertyOrder(int.MinValue)]</c>.</remarks>
  string Method { get; }
}
