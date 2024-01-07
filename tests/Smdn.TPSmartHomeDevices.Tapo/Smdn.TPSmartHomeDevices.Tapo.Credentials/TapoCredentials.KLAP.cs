// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.TPSmartHomeDevices.Tapo.Credentials;

partial class TapoCredentialsTests {
  [TestCase(0)]
  [TestCase(1)]
  [TestCase(31)]
  public void TryComputeKlapLocalAuthHash_DestinationTooShort(int length)
  {
    Assert.That(
      TapoCredentials.TryComputeKlapLocalAuthHash(
        username: default,
        password: default,
        destination: 0 == length ? Span<byte>.Empty : stackalloc byte[length],
        out var bytesWritten
      ),
      Is.False
    );

    Assert.That(bytesWritten, Is.EqualTo(0), nameof(bytesWritten));
  }

  private static System.Collections.IEnumerable YieldTestCases_TryComputeKlapLocalAuthHash()
  {
    yield return new object?[] { "user@mail.test", "password", Convert.FromHexString("0F2256EF19E6AC29DAD1F079E98FB53B7DDE48FB4E09ECDFB0B712F20C906F4F") };
    yield return new object?[] { string.Empty, string.Empty, Convert.FromHexString("B8628AB91C74F531603F6D5B45E730E54C123A7247B8978272C03F1E13560CEA") };
    yield return new object?[] { "user", string.Empty, Convert.FromHexString("808D22172BC7CDC267649C65CD69AD741EFC58BBF6B4F74E29A903100B32744E") };
    yield return new object?[] { string.Empty, "pass", Convert.FromHexString("081D58009C542FD5BFE65C5EE59EF09F47962AE2BECB4C1692C40EFB73F3DCC5") };
    yield return new object?[] { "user", "pass", Convert.FromHexString("231227829D6340F889A48EE5B6784EF8CFBE32B76C4ABB62183B600C8560870E") };
  }

  [TestCaseSource(nameof(YieldTestCases_TryComputeKlapLocalAuthHash))]
  public void TryComputeKlapLocalAuthHash(string username, string password, byte[] expected)
  {
    var buffer = new byte[32];

    Assert.That(
      TapoCredentials.TryComputeKlapLocalAuthHash(
        username: Encoding.ASCII.GetBytes(username),
        password: Encoding.ASCII.GetBytes(password),
        destination: buffer.AsSpan(),
        out var bytesWritten
      ),
      Is.True
    );

    Assert.That(bytesWritten, Is.EqualTo(32), nameof(bytesWritten));

    Assert.That(buffer, SequenceIs.EqualTo(expected), nameof(buffer));
  }
}
