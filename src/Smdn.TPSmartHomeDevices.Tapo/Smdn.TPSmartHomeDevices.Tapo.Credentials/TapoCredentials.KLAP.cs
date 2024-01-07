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
  public static bool TryComputeKlapLocalAuthHash(
    ReadOnlySpan<byte> username,
    ReadOnlySpan<byte> password,
    Span<byte> destination,
    out int bytesWritten
  )
  {
    bytesWritten = 0;

    if (destination.Length < SHA256HashSizeInBytes)
      return false; // destination too short

    // authHashInput = SHA1(username) + SHA1(password)
    byte[]? authHashInput = null;

    try {
      var bytesWrittenAuthHashInput = 0;

      authHashInput = ArrayPool<byte>.Shared.Rent(2 * SHA1HashSizeInBytes);

      // SHA1(username)
#pragma warning disable SA1114
#if SYSTEM_SECURITY_CRYPTOGRAPHY_SHA1_TRYHASHDATA
      {
        var retUsernameHash = SHA1.TryHashData(
#else
      using (var sha1ForUsername = SHA1.Create()) {
        var retUsernameHash = sha1ForUsername.TryComputeHash(
#endif
          username,
          authHashInput.AsSpan(bytesWrittenAuthHashInput, SHA1HashSizeInBytes),
          out var bytesWrittenByUsernameHash
        );
#pragma warning restore SA1114

        if (!retUsernameHash)
          return false;
        if (bytesWrittenByUsernameHash != SHA1HashSizeInBytes)
          return false;

        bytesWrittenAuthHashInput += bytesWrittenByUsernameHash;
      }

      // SHA1(password)
#pragma warning disable SA1114
#if SYSTEM_SECURITY_CRYPTOGRAPHY_SHA1_TRYHASHDATA
      {
        var retPasswordHash = SHA1.TryHashData(
#else
      using (var sha1ForPassword = SHA1.Create()) {
        var retPasswordHash = sha1ForPassword.TryComputeHash(
#endif
          password,
          authHashInput.AsSpan(bytesWrittenAuthHashInput, SHA1HashSizeInBytes),
          out var bytesWrittenByPasswordHash
        );
#pragma warning restore SA1114

        if (!retPasswordHash)
          return false;
        if (bytesWrittenByPasswordHash != SHA1HashSizeInBytes)
          return false;

        bytesWrittenAuthHashInput += bytesWrittenByPasswordHash;
      }

      // SHA256(authHashInput) = SHA256(SHA1(username) + SHA1(password))
#pragma warning disable SA1114
#if SYSTEM_SECURITY_CRYPTOGRAPHY_SHA256_TRYHASHDATA
      return SHA256.TryHashData(
#else
      using var sha256 = SHA256.Create();

      return sha256.TryComputeHash(
#endif
        authHashInput.AsSpan(0, bytesWrittenAuthHashInput),
        destination,
        out bytesWritten
      );
#pragma warning restore SA1114
    }
    finally {
      if (authHashInput is not null)
        ArrayPool<byte>.Shared.Return(authHashInput, clearArray: true);
    }
  }
#pragma warning restore CA5350
}
