// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Text;
using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

[TestFixture]
partial class KlapEncryptionAlgorithmTests {
  private static string ToUpperCaseHexString(ReadOnlySpan<byte> sequence)
    =>
#if SYSTEM_CONVERT_TOHEXSTRING
      Convert.ToHexString(sequence);
#else
      Smdn.Formats.Hexadecimal.ToUpperCaseString(sequence);
#endif

  private static System.Collections.IEnumerable YieldTestCases_Ctor()
  {
    yield return new object[] {
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      "705764D7E6510B12FBE5997667232354",
      "0AB306823035661BB8DBA21C",
      -355025127,
      "B3B5C44D3DEAFA9F7FC202D2A8A8350233E989171A6FB8789DCB0038"
    };

    yield return new object[] {
      new byte[16] {
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
      },
      new byte[16] {
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
      },
      new byte[32] {
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
      },
      "74166C5AA80EC244A905DED5F9AA1631",
      "1EE245AFF80E8393E8FF192C",
      -318086272,
      "8172F457B22FCE09D5AB35BF291E68F2602FA21DD8A3FF22843B3338"
    };

    yield return new object[] {
      new byte[16] {
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
      },
      new byte[16] {
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
      },
      new byte[32] {
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
      },
      "6DB82227A2CC388C18A64876E39D5847",
      "D377A0DECED0FA06137AB3A4",
      1993271313,
      "4E3909ECCFB44BF76165DD9F3384AC622EFB68B5BEEF5E422929E6A9"
    };
  }

  [TestCaseSource(nameof(YieldTestCases_Ctor))]
  public void Ctor(
    byte[] localSeed,
    byte[] remoteSeed,
    byte[] userHash,
    string expectedKey,
    string expectedIV,
    int expectedSequenceNumber,
    string expectedSignature
  )
  {
    var klap = new KlapEncryptionAlgorithm(
      localSeed: localSeed,
      remoteSeed: remoteSeed,
      userHash: userHash
    );

    Assert.That(
      ToUpperCaseHexString(klap.Key),
      Is.EqualTo(expectedKey),
      nameof(klap.Key)
    );
    Assert.That(
      ToUpperCaseHexString(klap.IV),
      Is.EqualTo(expectedIV),
      nameof(klap.IV)
    );
    Assert.That(
      klap.SequenceNumber,
      Is.EqualTo(expectedSequenceNumber),
      nameof(klap.SequenceNumber)
    );
    Assert.That(
      ToUpperCaseHexString(klap.Signature),
      Is.EqualTo(expectedSignature),
      nameof(klap.Signature)
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_Encrypt()
  {
    yield return new object[] {
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      "{}",
      "684337E81DBEA14B22127620B749E78CE943D2569AE7A3AF0F10C3D4EE7926D1736B699DACB2151284B9DAFA0E1C56E0",
      -355025126
    };

    yield return new object[] {
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      string.Empty,
      "77515F9A819F548B6959B160EAD2FDA80DDC65B748A2202468B6836E8180AD41532651506044A8BC4F6AE22A8DFED768",
      -355025126
    };

    yield return new object[] {
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      "{\"method\":\"xxx\"}", // 16 bytes
      "659B26E31FB68230DF25E8D8F5D31A690615CDFB8413AD3000D68B5BF36D8A3390ED06D7F695812C88A324F94720ABDBE90F020FFB51F2EFC99A2CBADB0637F7",
      -355025126
    };

    yield return new object[] {
      new byte[16] {
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
      },
      new byte[16] {
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
      },
      new byte[32] {
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
      },
      "{}",
      "203D71F48AD1E13927CD0C6D0CA4A9F4B35DA671A9DC2A2705F048F3210645775E79E4E207E6C05D88B9F9041C9D3919",
      1993271314
    };
  }

  [TestCaseSource(nameof(YieldTestCases_Encrypt))]
  public void Encrypt(
    byte[] localSeed,
    byte[] remoteSeed,
    byte[] userHash,
    string rawText,
    string expectedEncryptedBytes,
    int expectedSequenceNumber
  )
  {
    var klap = new KlapEncryptionAlgorithm(
      localSeed: localSeed,
      remoteSeed: remoteSeed,
      userHash: userHash
    );

    var dest = new ArrayBufferWriter<byte>();

    var seq = klap.Encrypt(
      Encoding.UTF8.GetBytes(rawText),
      dest
    );

    Assert.That(
      ToUpperCaseHexString(dest.WrittenSpan),
      Is.EqualTo(expectedEncryptedBytes),
      nameof(dest.WrittenSpan)
    );
    Assert.That(
      seq,
      Is.EqualTo(expectedSequenceNumber),
      nameof(seq)
    );
    Assert.That(
      klap.SequenceNumber,
      Is.EqualTo(expectedSequenceNumber),
      nameof(klap.SequenceNumber)
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_Decrypt()
  {
    static byte[] FromHexString(string hex)
#if SYSTEM_CONVERT_FROMHEXSTRING
      => Convert.FromHexString(hex);
#else
      => throw new NotImplementedException();
#endif

    yield return new object[] {
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      FromHexString("60B20DF2B71D0649F5BEB4153861F487CA19D3C8E9EF6CF4588090228CA6B2052003A0DEB763C8A0CDAD016CB9F18D01"),
      0,
      Encoding.UTF8.GetBytes("{}"),
    };

    yield return new object[] {
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      FromHexString("7257ED3C435C7654A21A1CB0E07597476823C45D1BEAFCD6279EC11898C626202F2215931DE29979F095B67D14DA0E4A"),
      -1,
      Encoding.UTF8.GetBytes("{}"),
    };

    yield return new object[] {
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      FromHexString("77515F9A819F548B6959B160EAD2FDA80DDC65B748A2202468B6836E8180AD41532651506044A8BC4F6AE22A8DFED768"),
      -355025126,
      Array.Empty<byte>(),
    };

    yield return new object[] {
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      FromHexString("659B26E31FB68230DF25E8D8F5D31A690615CDFB8413AD3000D68B5BF36D8A3390ED06D7F695812C88A324F94720ABDBE90F020FFB51F2EFC99A2CBADB0637F7"),
      -355025126,
      Encoding.UTF8.GetBytes("{\"method\":\"xxx\"}")
    };

    yield return new object[] {
      new byte[16] {
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
      },
      new byte[16] {
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
      },
      new byte[32] {
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
      },
      FromHexString("203D71F48AD1E13927CD0C6D0CA4A9F4B35DA671A9DC2A2705F048F3210645775E79E4E207E6C05D88B9F9041C9D3919"),
      1993271314,
      Encoding.UTF8.GetBytes("{}"),
    };

    yield return new object[] {
      new byte[16] {
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
      },
      new byte[16] {
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
      },
      new byte[32] {
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
      },
      FromHexString("B88349A38E19119B4F13DD8F4A2756C97B4691C3B4C3EFD125725ABE5C92FA75D8BC8EFFE63A7248D53AD52E47A71710"),
      1993271314,
      Array.Empty<byte>(),
    };
  }

  [TestCaseSource(nameof(YieldTestCases_Decrypt))]
  public void Decrypt(
    byte[] localSeed,
    byte[] remoteSeed,
    byte[] userHash,
    byte[] encryptedText,
    int sequenceNumber,
    byte[] expectedDecryptedBytes
  )
  {
    var klap = new KlapEncryptionAlgorithm(
      localSeed: localSeed,
      remoteSeed: remoteSeed,
      userHash: userHash
    );

    var dest = new ArrayBufferWriter<byte>();

    klap.Decrypt(
      encryptedText: encryptedText,
      sequenceNumber: sequenceNumber,
      destination: dest
    );

    Assert.That(
      dest.WrittenMemory,
      SequenceIs.EqualTo(expectedDecryptedBytes),
      message: nameof(dest.WrittenMemory)
    );
  }
}
