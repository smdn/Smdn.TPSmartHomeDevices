// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

[TestFixture]
public class TapoCredentialUtilsTests {
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
