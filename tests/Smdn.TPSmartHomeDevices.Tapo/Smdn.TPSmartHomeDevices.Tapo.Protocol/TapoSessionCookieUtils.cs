// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

[TestFixture]
public class TapoSessionCookieUtilsTests {
  // valid session IDs
  [TestCase("TP_SESSIONID=XXXX", true, "XXXX", null)]
  [TestCase("TP_SESSIONID=XXXX ", true, "XXXX", null)]
  [TestCase("TP_SESSIONID=XXXX;", true, "XXXX", null)]
  [TestCase("TP_SESSIONID=XXXX ;", true, "XXXX", null)]
  // invalid session IDs
  [TestCase("TP_SESSIONID=", false, "", null)]
  [TestCase("TP_SESSIONID=;TIMEOUT=1440", false, "", null)]
  // valid TIMEOUT attributes
  [TestCase("TP_SESSIONID=X;TIMEOUT=0", true, "X", 0)]
  [TestCase("TP_SESSIONID=X;TIMEOUT=1", true, "X", 1)]
  [TestCase("TP_SESSIONID=X;TIMEOUT=1440", true, "X", 1440)]
  [TestCase("TP_SESSIONID=X;TIMEOUT=1440 ", true, "X", 1440)]
  [TestCase("TP_SESSIONID=X; TIMEOUT=1440", true, "X", 1440)]
  [TestCase("TP_SESSIONID=X;TIMEOUT=1440;", true, "X", 1440)]
  [TestCase("TP_SESSIONID=X;TIMEOUT=1440;EXTRA=extra", true, "X", 1440)]
  [TestCase("TP_SESSIONID=X;TIMEOUT=1440; EXTRA=extra", true, "X", 1440)]
  [TestCase("TP_SESSIONID=X;EXTRA=extra;TIMEOUT=1440", true, "X", 1440)]
  [TestCase("TP_SESSIONID=X; EXTRA=extra;TIMEOUT=1440", true, "X", 1440)]
  [TestCase("TP_SESSIONID=X; EXTRA=extra; TIMEOUT=1440", true, "X", 1440)]
  // invalid TIMEOUT attributes
  [TestCase("TP_SESSIONID=XXXX;TIMEOUT= 1440", true, "XXXX", null)]
  [TestCase("TP_SESSIONID=XXXX;TIMEOUT=-1", true, "XXXX", null)]
  [TestCase("TP_SESSIONID=XXXX;TIMEOUT=+1", true, "XXXX", null)]
  [TestCase("TP_SESSIONID=XXXX;TIMEOUT=1.0", true, "XXXX", null)]
  // invalid cookie value or not TP_SESSIONID
  [TestCase("TP_SESSIONID", false, null, null)]
  [TestCase("TP_UNKNOWN=XXXX", false, null, null)]
  public void TryParseCookie(
    string input,
    bool expectedResult,
    string? expectedSessionId,
    int? expectedSessionTimeout
  )
  {
    var result = TapoSessionCookieUtils.TryParseCookie(input, out var sessionId, out var sessionTimeout);

    Assert.AreEqual(expectedResult, result, nameof(result));
    Assert.AreEqual(expectedSessionId, sessionId, nameof(sessionId));
    Assert.AreEqual(expectedSessionTimeout, sessionTimeout, nameof(sessionTimeout));
  }

  private static System.Collections.IEnumerable YieldTestCases_TryGetCookie()
  {
    yield return new object?[] {
      new string[] { "TP_SESSIONID=XXXX;TIMEOUT=1440" },
      true,
      "XXXX",
      1440
    };
    yield return new object?[] {
      new string[] { "SESSIONID=123456; Domain=example.com", "TP_SESSIONID=XXXX;TIMEOUT=1440" },
      true,
      "XXXX",
      1440
    };
    yield return new object?[] {
      new string[] { "SESSIONID=123456; Domain=example.com" },
      false,
      null,
      null
    };
    yield return new object?[] {
      new string[0],
      false,
      null,
      null
    };
  }

  [TestCaseSource(nameof(YieldTestCases_TryGetCookie))]
  public void TryGetCookie(
    string[]? inputs,
    bool expectedResult,
    string? expectedSessionId,
    int? expectedSessionTimeout
  )
  {
    var result = TapoSessionCookieUtils.TryGetCookie(inputs, out var sessionId, out var sessionTimeout);

    Assert.AreEqual(expectedResult, result, nameof(result));
    Assert.AreEqual(expectedSessionId, sessionId, nameof(sessionId));
    Assert.AreEqual(expectedSessionTimeout, sessionTimeout, nameof(sessionTimeout));
  }
}
