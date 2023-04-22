// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using Smdn.Formats;

namespace Smdn.TPSmartHomeDevices.Tapo.Credentials;

/// <remarks>
/// This implementation is based on and ported from the following implementation: <see href="https://github.com/fishbigger/TapoP100">fishbigger/TapoP100</see>.
/// </remarks>
public static class TapoCredentialUtils {
#if SYSTEM_SECURITY_CRYPTOGRAPHY_SHA1_HASHSIZEINBYTES
  private const int SHA1HashSizeInBytes = SHA1.HashSizeInBytes;
#else
  private const int SHA1HashSizeInBytes = 160/*bits*/ / 8;
#endif
  public const int HexSHA1HashSizeInBytes = SHA1HashSizeInBytes * 2; // byte array -> hex byte array

  /// <summary>
  /// Hash the string passed by the <paramref name="str"/> with SHA-1 algorithm and converts to the base64 format used for authentication of Tapo devices.
  /// </summary>
  /// <param name="str">The string to convert.</param>
  /// <returns>The <see cref="string"/> containing the result of the conversion.</returns>
  public static string ToBase64EncodedSHA1DigestString(ReadOnlySpan<char> str)
  {
    byte[]? bytes = null;

    try {
      // string -> UTF-8 byte array
      var length = Encoding.UTF8.GetByteCount(str);

      bytes = ArrayPool<byte>.Shared.Rent(length);

      Encoding.UTF8.GetBytes(str, bytes);

      // UTF-8 byte array -> hex SHA-1 hash byte array
      Span<byte> hexSHA1Hash = stackalloc byte[HexSHA1HashSizeInBytes];

      if (!TryConvertToHexSHA1Hash(bytes.AsSpan(0, length), hexSHA1Hash, out _))
        throw new InvalidOperationException("failed to convert hex SHA-1 hash");

      // hex SHA-1 hash byte array -> base64 string
      return Convert.ToBase64String(hexSHA1Hash, Base64FormattingOptions.None);
    }
    finally {
      if (bytes is not null)
        ArrayPool<byte>.Shared.Return(bytes, clearArray: true);
    }
  }

  /// <summary>
  /// Attempts to convert the UTF-8 string passed by the <paramref name="input"/> to the SHA-1 hash represented in the hexadecimal format (base16).
  /// </summary>
  /// <param name="input">The UTF-8 string to convert.</param>
  /// <param name="destination">The buffer to receive the converted value.</param>
  /// <param name="bytesWritten">When this method returns, the total number of bytes written into destination.</param>
  /// <returns><see langword="false"/> if <paramref name="destination"/> is too small to hold the calculated hash, <see langword="true"/> otherwise.</returns>
  public static bool TryConvertToHexSHA1Hash(
    ReadOnlySpan<byte> input,
    Span<byte> destination,
    out int bytesWritten
  )
  {
    bytesWritten = 0;

    if (destination.Length < HexSHA1HashSizeInBytes)
      return false;

    Span<byte> sha1hash = stackalloc byte[SHA1HashSizeInBytes];

    try {
#pragma warning disable CA5350
#if SYSTEM_SECURITY_CRYPTOGRAPHY_SHA1_TRYHASHDATA
      if (!SHA1.TryHashData(input, sha1hash, out var bytesWrittenSHA1))
        return false; // destination too short
#else
      using var sha1 = SHA1.Create();

      if (!sha1.TryComputeHash(input, sha1hash, out var bytesWrittenSHA1))
        return false; // destination too short
#endif
#pragma warning restore CA5350

      if (bytesWrittenSHA1 != SHA1HashSizeInBytes)
        return false; // unexpected state

      /*
        * SHA-1 hash byte array -> hex byte array
        */
      if (!Hexadecimal.TryEncodeLowerCase(sha1hash, destination, out bytesWritten))
        return false; // destination too short

      return true;
    }
    finally {
      sha1hash.Clear();
    }
  }

  public static string ToBase64EncodedString(ReadOnlySpan<char> str)
  {
    byte[]? bytes = null;

    try {
      var length = Encoding.UTF8.GetByteCount(str);

      bytes = ArrayPool<byte>.Shared.Rent(length);

      var bytesWritten = Encoding.UTF8.GetBytes(str, bytes);

      return Convert.ToBase64String(bytes.AsSpan(0, bytesWritten), Base64FormattingOptions.None);
    }
    finally {
      if (bytes is not null)
        ArrayPool<byte>.Shared.Return(bytes, clearArray: true);
    }
  }
}
