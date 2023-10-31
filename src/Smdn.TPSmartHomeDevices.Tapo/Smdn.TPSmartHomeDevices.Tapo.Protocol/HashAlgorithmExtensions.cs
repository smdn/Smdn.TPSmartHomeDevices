// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Security.Cryptography;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

public static class HashAlgorithmExtensions {
  public static bool TryComputeHash(
    this HashAlgorithm algorithm,
    Span<byte> destination,
    ReadOnlySpan<byte> source0,
    ReadOnlySpan<byte> source1,
    ReadOnlySpan<byte> source2,
    out int bytesWritten
  )
    => TryComputeHash(
      algorithm: algorithm ?? throw new ArgumentNullException(nameof(algorithm)),
      destination: destination,
      source0: source0,
      source1: source1,
      source2: source2,
      source3: default,
      bytesWritten: out bytesWritten
    );

  public static bool TryComputeHash(
    this HashAlgorithm algorithm,
    Span<byte> destination,
    ReadOnlySpan<byte> source0,
    ReadOnlySpan<byte> source1,
    ReadOnlySpan<byte> source2,
    ReadOnlySpan<byte> source3,
    out int bytesWritten
  )
  {
    if (algorithm is null)
      throw new ArgumentNullException(nameof(algorithm));

    byte[]? source = null;

    try {
      var sourceLength = source0.Length + source1.Length + source2.Length + source3.Length;

      source = ArrayPool<byte>.Shared.Rent(sourceLength);

      var sourceSpan = source.AsSpan(0, sourceLength);

      source0.CopyTo(sourceSpan);

      sourceSpan = sourceSpan.Slice(source0.Length);

      source1.CopyTo(sourceSpan);

      sourceSpan = sourceSpan.Slice(source1.Length);

      source2.CopyTo(sourceSpan);

      sourceSpan = sourceSpan.Slice(source2.Length);

      source3.CopyTo(sourceSpan);

      return algorithm.TryComputeHash(source.AsSpan(0, sourceLength), destination, out bytesWritten);
    }
    finally {
      if (source is not null)
        ArrayPool<byte>.Shared.Return(source, clearArray: true);
    }
  }
}
