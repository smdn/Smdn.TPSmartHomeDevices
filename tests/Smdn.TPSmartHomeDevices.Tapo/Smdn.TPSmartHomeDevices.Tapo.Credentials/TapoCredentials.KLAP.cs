// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.TPSmartHomeDevices.Tapo.Credentials;

partial class TapoCredentialsTests {
  [Test]
  public void TryComputeKlapAuthHash_CredentialNull()
  {
    Assert.IsFalse(
      TapoCredentials.TryComputeKlapAuthHash(
        credential: null!,
        destination: default,
        out var bytesWritten
      )
    );

    Assert.AreEqual(0, bytesWritten, nameof(bytesWritten));
  }

  [TestCase(0)]
  [TestCase(1)]
  [TestCase(31)]
  public void TryComputeKlapAuthHash_DestinationTooShort(int length)
  {
    Assert.IsFalse(
      TapoCredentials.TryComputeKlapAuthHash(
        credential: null!,
        destination: 0 == length ? Span<byte>.Empty : stackalloc byte[length],
        out var bytesWritten
      )
    );

    Assert.AreEqual(0, bytesWritten, nameof(bytesWritten));
  }

  private static System.Collections.IEnumerable YieldTestCases_TryComputeKlapAuthHash()
  {
    foreach (var (email, password, expected) in new[] {
      ("user@mail.test", "password", Convert.FromHexString("0F2256EF19E6AC29DAD1F079E98FB53B7DDE48FB4E09ECDFB0B712F20C906F4F")),
      (string.Empty, string.Empty, Convert.FromHexString("B8628AB91C74F531603F6D5B45E730E54C123A7247B8978272C03F1E13560CEA")),
      ("user", string.Empty, Convert.FromHexString("808D22172BC7CDC267649C65CD69AD741EFC58BBF6B4F74E29A903100B32744E")),
      (string.Empty, "pass", Convert.FromHexString("081D58009C542FD5BFE65C5EE59EF09F47962AE2BECB4C1692C40EFB73F3DCC5")),
      ("user", "pass", Convert.FromHexString("231227829D6340F889A48EE5B6784EF8CFBE32B76C4ABB62183B600C8560870E")),
    }) {
      var services = new ServiceCollection();

      services.AddTapoCredential(
        email: email,
        password: password
      );

      yield return new object[] { services.BuildServiceProvider(), expected };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_TryComputeKlapAuthHash))]
  public void TryComputeKlapAuthHash(IServiceProvider serviceProvider, byte[] expected)
  {
    var buffer = new byte[32];

    Assert.IsTrue(
      TapoCredentials.TryComputeKlapAuthHash(
        credential: serviceProvider.GetRequiredService<ITapoCredentialProvider>().GetCredential(null),
        destination: buffer.AsSpan(),
        out var bytesWritten
      )
    );

    Assert.AreEqual(32, bytesWritten, nameof(bytesWritten));

    Assert.That(buffer, SequenceIs.EqualTo(expected), nameof(buffer));
  }
}
