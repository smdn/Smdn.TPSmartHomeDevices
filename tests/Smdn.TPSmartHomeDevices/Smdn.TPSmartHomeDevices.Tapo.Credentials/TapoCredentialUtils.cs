// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using NUnit.Framework;
using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.TPSmartHomeDevices.Tapo.Credentials;

[TestFixture]
public class TapoCredentialUtilsTests {
  [TestCase("user", TapoCredentialUtils.HexSHA1HashSizeInBytes, "MTJkZWE5NmZlYzIwNTkzNTY2YWI3NTY5MmM5OTQ5NTk2ODMzYWRjOQ==")]
  [TestCase("user", TapoCredentialUtils.HexSHA1HashSizeInBytes + 1, "MTJkZWE5NmZlYzIwNTkzNTY2YWI3NTY5MmM5OTQ5NTk2ODMzYWRjOQ==")]
  [TestCase("user@mail.test", TapoCredentialUtils.HexSHA1HashSizeInBytes, "YjhlY2VjNWIzNjk0ZTVlNzE0YTYxMmNhZTZlZTJiNmExMjQ5ZmZmZQ==")]
  [TestCase("", TapoCredentialUtils.HexSHA1HashSizeInBytes, "ZGEzOWEzZWU1ZTZiNGIwZDMyNTViZmVmOTU2MDE4OTBhZmQ4MDcwOQ==")]
  public void TryConvertToHexSHA1Hash(string input, int bufferSize, string expected)
  {
    var buffer = new byte[TapoCredentialUtils.HexSHA1HashSizeInBytes];

    Assert.IsTrue(TapoCredentialUtils.TryConvertToHexSHA1Hash(Encoding.UTF8.GetBytes(input), buffer, out var bytesWritten));
    Assert.AreEqual(TapoCredentialUtils.HexSHA1HashSizeInBytes, bytesWritten, nameof(bytesWritten));

    Assert.That(buffer.AsMemory(0, bytesWritten), SequenceIs.EqualTo(Convert.FromBase64String(expected)));
  }

  [TestCase(0)]
  [TestCase(1)]
  [TestCase(TapoCredentialUtils.HexSHA1HashSizeInBytes - 1)]
  public void TryConvertToHexSHA1Hash_DestinationTooShort(int bufferSize)
  {
    var buffer = new byte[bufferSize];

    Assert.IsFalse(TapoCredentialUtils.TryConvertToHexSHA1Hash(Array.Empty<byte>(), buffer, out var bytesWritten));
    Assert.AreNotEqual(TapoCredentialUtils.HexSHA1HashSizeInBytes, bytesWritten, nameof(bytesWritten));
  }

  [TestCase("user", "MTJkZWE5NmZlYzIwNTkzNTY2YWI3NTY5MmM5OTQ5NTk2ODMzYWRjOQ==")]
  [TestCase("user@mail.test", "YjhlY2VjNWIzNjk0ZTVlNzE0YTYxMmNhZTZlZTJiNmExMjQ5ZmZmZQ==")]
  [TestCase("", "ZGEzOWEzZWU1ZTZiNGIwZDMyNTViZmVmOTU2MDE4OTBhZmQ4MDcwOQ==")]
  public void ToBase64EncodedSHA1DigestString(string input, string expected)
    => Assert.AreEqual(expected, TapoCredentialUtils.ToBase64EncodedSHA1DigestString(input));

  [TestCase("pass", "cGFzcw==")]
  [TestCase("password", "cGFzc3dvcmQ=")]
  [TestCase("", "")]
  public void ToBase64EncodedString(string input, string expected)
    => Assert.AreEqual(expected, TapoCredentialUtils.ToBase64EncodedString(input));
}
