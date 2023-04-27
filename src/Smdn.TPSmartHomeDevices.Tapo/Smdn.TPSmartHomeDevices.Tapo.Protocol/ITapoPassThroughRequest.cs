// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// The interface that represents that the type is an object representation of the parameter property
/// encapsulated in a JSON request for 'method: securePassthrough'.
/// </summary>
/// <seealso cref="SecurePassThroughJsonConverterFactory"/>
/// <seealso cref="SecurePassThroughRequest{TPassThroughRequest}"/>
public interface ITapoPassThroughRequest : ITapoRequest {
}
