// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// The exception that is thrown when invalid padding is detected during
/// the decryption of <see cref="SecurePassThroughResponse{TPassThroughResponse}"/>.
/// </summary>
/// <remarks>
/// Communication with the Tapo device uses the <see cref="System.Security.Cryptography.Aes"/> algorithm,
/// but in rare cases, a <see cref="System.Security.Cryptography.CryptographicException"/> is thrown with
/// the message <c>"Padding is invalid and cannot be removed."</c> during the response decryption.
/// <see cref="SecurePassThroughInvalidPaddingException"/> is thrown when such a condition occurs.
/// </remarks>
/// <see cref="SecurePassThroughJsonConverterFactory"/>
public class SecurePassThroughInvalidPaddingException : SystemException {
  public SecurePassThroughInvalidPaddingException(
    string message,
    Exception? innerException
  )
    : base(
      message: message,
      innerException: innerException
    )
  {
  }
}
