// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: GPL-3.0-or-later
using System;
using System.Buffers;
using System.Security.Cryptography;

namespace Smdn.TPSmartHomeDevices.Tapo.Credentials;

#pragma warning disable IDE0040
partial class TapoCredentials {
#pragma warning restore IDE0040
  private const int SHA256HashSizeInBytes =
#if SYSTEM_SECURITY_CRYPTOGRAPHY_SHA256_HASHSIZEINBYTES
    SHA256.HashSizeInBytes;
#else
    32;
#endif

  /// <summary>
  /// A C# implementation of hash algorithm for credentials in Tapo's KLAP protocol.
  /// </summary>
  /// <remarks>
  /// This implementation is based on and ported from the following
  /// Python implementation by <see href="https://github.com/petretiandrea">petretiandrea</see>:
  /// <see href="https://github.com/petretiandrea/plugp100">petretiandrea/plugp100</see>, published under the GPL-3.0 license,
  /// forked from <see href="https://github.com/K4CZP3R/tapo-p100-python">K4CZP3R/tapo-p100-python</see>.
  /// </remarks>
#pragma warning disable CA5350
  public static bool TryComputeKlapAuthHash(
    ITapoCredential credential,
    Span<byte> destination,
    out int bytesWritten
  )
  {
    bytesWritten = 0;

    if (credential is null)
      return false;

    if (destination.Length < SHA256HashSizeInBytes)
      return false; // destination too short

    // authHashInput = SHA1(username) + SHA1(password)
    byte[]? authHashInput = null;

    try {
      var bytesWrittenAuthHashInput = 0;

      authHashInput = ArrayPool<byte>.Shared.Rent(2 * SHA1HashSizeInBytes);

      // SHA1(username)
      using (var sha1 = SHA1.Create()) {
        bytesWrittenAuthHashInput += credential.HashUsername(
          sha1,
          authHashInput.AsSpan(bytesWrittenAuthHashInput, SHA1HashSizeInBytes)
        );

        if (bytesWrittenAuthHashInput != SHA1HashSizeInBytes)
          return false;
      }

      // SHA1(password)
      using (var sha1 = SHA1.Create()) {
        bytesWrittenAuthHashInput += credential.HashPassword(
          sha1,
          authHashInput.AsSpan(bytesWrittenAuthHashInput, SHA1HashSizeInBytes)
        );

        if (bytesWrittenAuthHashInput != SHA1HashSizeInBytes * 2)
          return false;
      }

      using var sha256 = SHA256.Create();

      // SHA256(authHashInput) = SHA256(SHA1(username) + SHA1(password))
      return sha256.TryComputeHash(
        authHashInput.AsSpan(0, bytesWrittenAuthHashInput),
        destination,
        out bytesWritten
      );
    }
    finally {
      if (authHashInput is not null)
        ArrayPool<byte>.Shared.Return(authHashInput, clearArray: true);
    }
  }
#pragma warning restore CA5350
}
