// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: GPL-3.0-or-later
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Security.Cryptography;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// A C# implementation of encryption/decryption algorithm for Tapo's KLAP protocol.
/// </summary>
/// <remarks>
/// This implementation is based on and ported from the following
/// Python implementation by <see href="https://github.com/petretiandrea">petretiandrea</see>:
/// <see href="https://github.com/petretiandrea/plugp100">petretiandrea/plugp100</see>, published under the GPL-3.0 license,
/// forked from <see href="https://github.com/K4CZP3R/tapo-p100-python">K4CZP3R/tapo-p100-python</see>.
/// </remarks>
public class KlapEncryptionAlgorithm {
  private const int SHA256HashSizeInBytes =
#if SYSTEM_SECURITY_CRYPTOGRAPHY_SHA256_HASHSIZEINBYTES
    SHA256.HashSizeInBytes;
#else
    32;
#endif

  private const int KeySizeInBytes = 16;
  private const int IVSizeInBytes = 12;
  private const int SigSizeInBytes = 28;
  private const int SeqSizeInBytes = 4;

  // SymmetricAlgorithm.IV requires the type of byte[]
  private static byte[] KeyDerive(
    ReadOnlySpan<byte> localSeed,
    ReadOnlySpan<byte> remoteSeed,
    ReadOnlySpan<byte> userHash
  )
  {
    var key = new byte[SHA256HashSizeInBytes];
    using var sha256 = SHA256.Create();

    _ = sha256.TryComputeHash(
      destination: key.AsSpan(),
      source0: stackalloc byte[3] { (byte)'l', (byte)'s', (byte)'k' }, // TODO: use UTF-8 strings
      source1: localSeed,
      source2: remoteSeed,
      source3: userHash,
      bytesWritten: out _
    );

    return key.AsMemory(..KeySizeInBytes).ToArray();
  }

  private static (ReadOnlyMemory<byte> IV, int Seq) IVDerive(
    ReadOnlySpan<byte> localSeed,
    ReadOnlySpan<byte> remoteSeed,
    ReadOnlySpan<byte> userHash
  )
  {
    var iv = new byte[SHA256HashSizeInBytes];
    using var sha256 = SHA256.Create();

    _ = sha256.TryComputeHash(
      destination: iv.AsSpan(),
      source0: stackalloc byte[2] { (byte)'i', (byte)'v' }, // TODO: use UTF-8 strings
      source1: localSeed,
      source2: remoteSeed,
      source3: userHash,
      bytesWritten: out _
    );

    // iv is first 16 bytes of sha256, where the last 4 bytes forms the
    // sequence number used in requests and is incremented on each request
    return (
      IV: iv.AsMemory(..IVSizeInBytes),
      Seq: BinaryPrimitives.ReadInt32BigEndian(iv.AsSpan(^4))
    );
  }

  private static ReadOnlyMemory<byte> SigDerive(
    ReadOnlySpan<byte> localSeed,
    ReadOnlySpan<byte> remoteSeed,
    ReadOnlySpan<byte> userHash
  )
  {
    // used to create a hash with which to prefix each request
    var hash = new byte[SHA256HashSizeInBytes];
    using var sha256 = SHA256.Create();

    _ = sha256.TryComputeHash(
      destination: hash.AsSpan(),
      source0: stackalloc byte[3] { (byte)'l', (byte)'d', (byte)'k' }, // TODO: use UTF-8 strings
      source1: localSeed,
      source2: remoteSeed,
      source3: userHash,
      bytesWritten: out _
    );

    return hash.AsMemory(..SigSizeInBytes);
  }

  public ReadOnlySpan<byte> Key => key;
  public ReadOnlySpan<byte> IV => iv.Span;
  public ReadOnlySpan<byte> Signature => sig.Span;
#pragma warning disable IDE0032
  public int SequenceNumber => seq;
#pragma warning restore IDE0032

  private readonly byte[] key; // SymmetricAlgorithm.Key requires the type of byte[]
  private readonly ReadOnlyMemory<byte> iv;
  private readonly ReadOnlyMemory<byte> sig;
#pragma warning disable IDE0032
  private int seq;
#pragma warning restore IDE0032

  // SymmetricAlgorithm.IV requires the type of byte[]
  private byte[] IVSeq(int sequenceNumber)
  {
    var iv_seq = new byte[IVSizeInBytes + SeqSizeInBytes];

    iv.Span.CopyTo(iv_seq.AsSpan(0, IVSizeInBytes));

    BinaryPrimitives.WriteInt32BigEndian(iv_seq.AsSpan(^SeqSizeInBytes), sequenceNumber);

    return iv_seq;
  }

  private Aes CreateAes(int sequenceNumber)
  {
    var aes = Aes.Create();

    aes.Mode = CipherMode.CBC;
    aes.Padding = PaddingMode.PKCS7;
    aes.Key = key;
    aes.IV = IVSeq(iv.Span, sequenceNumber);

    return aes;

    static byte[] IVSeq(ReadOnlySpan<byte> iv, int seq)
    {
      var iv_seq = new byte[IVSizeInBytes + SeqSizeInBytes];

      iv.CopyTo(iv_seq.AsSpan(0, IVSizeInBytes));

      BinaryPrimitives.WriteInt32BigEndian(iv_seq.AsSpan(^SeqSizeInBytes), seq);

      return iv_seq;
    }
  }

  public KlapEncryptionAlgorithm(
    ReadOnlySpan<byte> localSeed,
    ReadOnlySpan<byte> remoteSeed,
    ReadOnlySpan<byte> userHash
  )
  {
    key = KeyDerive(localSeed, remoteSeed, userHash);
    (iv, seq) = IVDerive(localSeed, remoteSeed, userHash);
    sig = SigDerive(localSeed, remoteSeed, userHash);
  }

  public int Encrypt(
    ReadOnlySpan<byte> rawText,
    IBufferWriter<byte> destination
  )
  {
    if (destination is null)
      throw new ArgumentNullException(nameof(destination));

    seq = unchecked(seq + 1); // increment the sequence number

    Encrypt(
      rawText: rawText,
      sequenceNumber: seq,
      destination: destination
    );

    return seq;
  }

  public void Encrypt(
    ReadOnlySpan<byte> rawText,
    int sequenceNumber,
    IBufferWriter<byte> destination
  )
  {
    if (destination is null)
      throw new ArgumentNullException(nameof(destination));

    // cipher_text = AES(raw_text)
    // signature = SHA256(sig + seq + cipher_text)
    // encryption = signature + cipher_text
    using var aes = CreateAes(sequenceNumber);

    // allocates 'signature' first, and writes the actual content after writing 'cipher_text'.
    var signatureSpan = destination.GetSpan(SHA256HashSizeInBytes).Slice(0, SHA256HashSizeInBytes);

    destination.Advance(SHA256HashSizeInBytes);

    // ref: https://stackoverflow.com/questions/3283787/size-of-data-after-aes-cbc-and-aes-ecb-encryption
    var cipherTextLength = rawText.Length + 16 - (rawText.Length % 16);
    var cipherTextSpan = destination.GetSpan(cipherTextLength).Slice(0, cipherTextLength);

    byte[]? cipherTextBuffer = null;

    try {
      cipherTextBuffer = ArrayPool<byte>.Shared.Rent(cipherTextLength);

      using var cipherTextStream = new MemoryStream(buffer: cipherTextBuffer, writable: true);

      using (var encryptingStream = new CryptoStream(
        stream: cipherTextStream,
        transform: aes.CreateEncryptor(),
        mode: CryptoStreamMode.Write,
        leaveOpen: true
      )) {
        encryptingStream.Write(rawText);

        encryptingStream.Flush();
      }

      cipherTextBuffer.AsSpan(0, cipherTextLength).CopyTo(cipherTextSpan);

      destination.Advance(cipherTextLength);
    }
    finally {
      if (cipherTextBuffer is not null)
        ArrayPool<byte>.Shared.Return(cipherTextBuffer, clearArray: true);
    }

    Span<byte> seqBigEndian = stackalloc byte[4];

    BinaryPrimitives.WriteInt32BigEndian(seqBigEndian, sequenceNumber);

    // SHA256(sig + seq + cipher_text)
    using var sha256 = SHA256.Create();

    _ = sha256.TryComputeHash(
      destination: signatureSpan,
      sig.Span,
      seqBigEndian,
      cipherTextSpan,
      out _
    );
  }

  public unsafe void Decrypt(
    ReadOnlySpan<byte> encryptedText,
    int sequenceNumber,
    IBufferWriter<byte> destination
  )
  {
    if (destination is null)
      throw new ArgumentNullException(nameof(destination));

    encryptedText = encryptedText.Slice(SHA256HashSizeInBytes); // skip SHA256 signature?

    using var aes = CreateAes(sequenceNumber);

    // TODO: reduce unmanaged code
    fixed (byte* ptrEncryptedText = encryptedText) {
      using var encryptedTextStream = new UnmanagedMemoryStream(ptrEncryptedText, encryptedText.Length);
      using var decryptingStream = new CryptoStream(
        stream: encryptedTextStream,
        transform: aes.CreateDecryptor(),
        mode: CryptoStreamMode.Read,
        leaveOpen: true
      );

      for (; ; ) {
        var buffer = destination.GetSpan(sizeHint: 256); // TODO: best size hint

        var length = decryptingStream.Read(buffer);

        if (length == 0)
          break;

        destination.Advance(length);
      }
    }
  }
}
