// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.TPSmartHomeDevices.Tapo.Credentials;

[TestFixture]
public partial class TapoCredentialsTests {
  [TestCase("user", TapoCredentials.HexSHA1HashSizeInBytes, "MTJkZWE5NmZlYzIwNTkzNTY2YWI3NTY5MmM5OTQ5NTk2ODMzYWRjOQ==")]
  [TestCase("user", TapoCredentials.HexSHA1HashSizeInBytes + 1, "MTJkZWE5NmZlYzIwNTkzNTY2YWI3NTY5MmM5OTQ5NTk2ODMzYWRjOQ==")]
  [TestCase("user@mail.test", TapoCredentials.HexSHA1HashSizeInBytes, "YjhlY2VjNWIzNjk0ZTVlNzE0YTYxMmNhZTZlZTJiNmExMjQ5ZmZmZQ==")]
  [TestCase("", TapoCredentials.HexSHA1HashSizeInBytes, "ZGEzOWEzZWU1ZTZiNGIwZDMyNTViZmVmOTU2MDE4OTBhZmQ4MDcwOQ==")]
  public void TryConvertToHexSHA1Hash(string input, int bufferSize, string expected)
  {
    var buffer = new byte[bufferSize];

    Assert.That(TapoCredentials.TryConvertToHexSHA1Hash(Encoding.UTF8.GetBytes(input), buffer, out var bytesWritten), Is.True);
    Assert.That(bytesWritten, Is.EqualTo(TapoCredentials.HexSHA1HashSizeInBytes), nameof(bytesWritten));

    Assert.That(buffer.AsMemory(0, bytesWritten), SequenceIs.EqualTo(Convert.FromBase64String(expected)));
  }

  [TestCase(0)]
  [TestCase(1)]
  [TestCase(TapoCredentials.HexSHA1HashSizeInBytes - 1)]
  public void TryConvertToHexSHA1Hash_DestinationTooShort(int bufferSize)
  {
    var buffer = new byte[bufferSize];

    Assert.That(TapoCredentials.TryConvertToHexSHA1Hash(Array.Empty<byte>(), buffer, out var bytesWritten), Is.False);
    Assert.That(bytesWritten, Is.Not.EqualTo(TapoCredentials.HexSHA1HashSizeInBytes), nameof(bytesWritten));
  }

  [TestCase("user", "MTJkZWE5NmZlYzIwNTkzNTY2YWI3NTY5MmM5OTQ5NTk2ODMzYWRjOQ==")]
  [TestCase("user@mail.test", "YjhlY2VjNWIzNjk0ZTVlNzE0YTYxMmNhZTZlZTJiNmExMjQ5ZmZmZQ==")]
  [TestCase("", "ZGEzOWEzZWU1ZTZiNGIwZDMyNTViZmVmOTU2MDE4OTBhZmQ4MDcwOQ==")]
  public void ToBase64EncodedSHA1DigestString(string input, string expected)
    => Assert.That(TapoCredentials.ToBase64EncodedSHA1DigestString(input), Is.EqualTo(expected));

  [TestCase("pass", "cGFzcw==")]
  [TestCase("password", "cGFzc3dvcmQ=")]
  [TestCase("", "")]
  public void ToBase64EncodedString(string input, string expected)
    => Assert.That(TapoCredentials.ToBase64EncodedString(input), Is.EqualTo(expected));
}
