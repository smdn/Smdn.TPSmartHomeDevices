// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Security.Cryptography;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

[TestFixture]
public class HashAlgorithmExtensionsTests {
  private static System.Collections.IEnumerable YieldTestCases_TryComputeHash_3Sources()
  {
    yield return new object[] {
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      SHA256.Create().ComputeHash(Array.Empty<byte>())
    };

    yield return new object[] {
      new[] { (byte)0 },
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      SHA256.Create().ComputeHash(new[] { (byte)0 } )
    };

    yield return new object[] {
      Array.Empty<byte>(),
      new[] { (byte)1 },
      Array.Empty<byte>(),
      SHA256.Create().ComputeHash(new[] { (byte)1 } )
    };

    yield return new object[] {
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      new[] { (byte)2 },
      SHA256.Create().ComputeHash(new[] { (byte)2 } )
    };

    yield return new object[] {
      new[] { (byte)0 },
      new[] { (byte)1 },
      Array.Empty<byte>(),
      SHA256.Create().ComputeHash(new[] { (byte)0, (byte)1 } )
    };

    yield return new object[] {
      Array.Empty<byte>(),
      new[] { (byte)1 },
      new[] { (byte)2 },
      SHA256.Create().ComputeHash(new[] { (byte)1, (byte)2 } )
    };

    yield return new object[] {
      new[] { (byte)0 },
      new[] { (byte)1 },
      new[] { (byte)2 },
      SHA256.Create().ComputeHash(new[] { (byte)0, (byte)1, (byte)2 } )
    };
  }

  [TestCaseSource(nameof(YieldTestCases_TryComputeHash_3Sources))]
  public void TryComputeHash_3Sources(
    byte[] source0,
    byte[] source1,
    byte[] source2,
    byte[] expected
  )
  {
    using var sha256 = SHA256.Create();
    Memory<byte> destination = new byte[32];

    Assert.IsTrue(
      sha256.TryComputeHash(
        destination.Span,
        source0,
        source1,
        source2,
        out var bytesWritten
      )
    );
    Assert.AreEqual(32, bytesWritten, nameof(bytesWritten));
    Assert.That(destination, SequenceIs.EqualTo(expected), nameof(destination));
  }

  [Test]
  public void TryComputeHash_3Sources_ArgumentNull()
    => Assert.Throws<ArgumentNullException>(
      () => HashAlgorithmExtensions.TryComputeHash(
        algorithm: null!,
        destination: default,
        source0: default,
        source1: default,
        source2: default,
        bytesWritten: out _
      )
    );

  private static System.Collections.IEnumerable YieldTestCases_TryComputeHash_4Sources()
  {
    yield return new object[] {
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      SHA256.Create().ComputeHash(Array.Empty<byte>())
    };

    yield return new object[] {
      new[] { (byte)0 },
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      SHA256.Create().ComputeHash(new[] { (byte)0 } )
    };

    yield return new object[] {
      Array.Empty<byte>(),
      new[] { (byte)1 },
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      SHA256.Create().ComputeHash(new[] { (byte)1 } )
    };

    yield return new object[] {
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      new[] { (byte)2 },
      Array.Empty<byte>(),
      SHA256.Create().ComputeHash(new[] { (byte)2 } )
    };


    yield return new object[] {
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      new[] { (byte)3 },
      SHA256.Create().ComputeHash(new[] { (byte)3 } )
    };

    yield return new object[] {
      new[] { (byte)0 },
      new[] { (byte)1 },
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      SHA256.Create().ComputeHash(new[] { (byte)0, (byte)1 } )
    };

    yield return new object[] {
      Array.Empty<byte>(),
      new[] { (byte)1 },
      new[] { (byte)2 },
      Array.Empty<byte>(),
      SHA256.Create().ComputeHash(new[] { (byte)1, (byte)2 } )
    };

    yield return new object[] {
      Array.Empty<byte>(),
      Array.Empty<byte>(),
      new[] { (byte)2 },
      new[] { (byte)3 },
      SHA256.Create().ComputeHash(new[] { (byte)2, (byte)3 } )
    };

    yield return new object[] {
      new[] { (byte)0 },
      new[] { (byte)1 },
      new[] { (byte)2 },
      new[] { (byte)3 },
      SHA256.Create().ComputeHash(new[] { (byte)0, (byte)1, (byte)2, (byte)3 } )
    };
  }

  [TestCaseSource(nameof(YieldTestCases_TryComputeHash_4Sources))]
  public void TryComputeHash_4Sources(
    byte[] source0,
    byte[] source1,
    byte[] source2,
    byte[] source3,
    byte[] expected
  )
  {
    using var sha256 = SHA256.Create();
    Memory<byte> destination = new byte[32];

    Assert.IsTrue(
      sha256.TryComputeHash(
        destination.Span,
        source0,
        source1,
        source2,
        source3,
        out var bytesWritten
      )
    );
    Assert.AreEqual(32, bytesWritten, nameof(bytesWritten));
    Assert.That(destination, SequenceIs.EqualTo(expected), nameof(destination));
  }

  [Test]
  public void TryComputeHash_4Sources_ArgumentNull()
    => Assert.Throws<ArgumentNullException>(
      () => HashAlgorithmExtensions.TryComputeHash(
        algorithm: null!,
        destination: default,
        source0: default,
        source1: default,
        source2: default,
        source3: default,
        bytesWritten: out _
      )
    );
}
