// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using Smdn.Formats;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <remarks>
/// This implementation is based on and ported from the following implementation: <see href="https://github.com/fishbigger/TapoP100">fishbigger/TapoP100</see>.
/// </remarks>
public static class TapoCredentialUtils {
  public static string ToBase64EncodedSHA1DigestString(ReadOnlySpan<char> str)
  {
#if SYSTEM_SECURITY_CRYPTOGRAPHY_SHA1_HASHSIZEINBYTES
    const int SHA1HashSizeInBytes = SHA1.HashSizeInBytes;
#else
    const int SHA1HashSizeInBytes = 160/*bits*/ / 8;
#endif

    byte[]? bytes = null;

    try {
      /*
       * string -> UTF-8 byte array
       */
      var length = Encoding.UTF8.GetByteCount(str);

      bytes = ArrayPool<byte>.Shared.Rent(length);

      Encoding.UTF8.GetBytes(str, bytes);

      /*
       * UTF-8 byte array -> SHA-1 hash byte array
       */
      Span<byte> sha1hash = stackalloc byte[SHA1HashSizeInBytes];

#if SYSTEM_SECURITY_CRYPTOGRAPHY_SHA1_HASHDATA
      var bytesWritten = SHA1.HashData(bytes.AsSpan(0, length), sha1hash);
#else
#pragma warning disable CA5350
      using var sha1 = SHA1.Create();
#pragma warning restore CA5350

      if (!sha1.TryComputeHash(bytes.AsSpan(0, length), sha1hash, out var bytesWritten))
        throw new InvalidOperationException("destination too short");
#endif

      Span<byte> hashLowerCaseHex = stackalloc byte[bytesWritten * 2];

      /*
       * SHA-1 hash byte array -> hex byte array
       */
      if (!Hexadecimal.TryEncodeLowerCase(sha1hash.Slice(0, bytesWritten), hashLowerCaseHex, out var bytesEncoded))
        throw new InvalidOperationException("destination too short");

      /*
       * hex byte array -> base64 string
       */
      return Convert.ToBase64String(hashLowerCaseHex.Slice(0, bytesEncoded), Base64FormattingOptions.None);
    }
    finally {
      if (bytes is not null)
        ArrayPool<byte>.Shared.Return(bytes, clearArray: true);
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
