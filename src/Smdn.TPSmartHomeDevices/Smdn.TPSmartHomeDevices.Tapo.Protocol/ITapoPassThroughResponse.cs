// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// The interface that represents that the type is an object representation of the result property
/// encapsulated in a JSON response for 'method: securePassthrough'.
/// </summary>
/// <seealso cref="SecurePassThroughJsonConverterFactory"/>
/// <seealso cref="SecurePassThroughResponse{TPassThroughResponse}"/>
public interface ITapoPassThroughResponse : ITapoResponse {
}
