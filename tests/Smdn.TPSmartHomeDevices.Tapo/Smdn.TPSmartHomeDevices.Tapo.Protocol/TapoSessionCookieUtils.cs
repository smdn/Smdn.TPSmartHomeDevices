// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
// cSpell:ignore SESSIONID
using System.Net.Http;

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

    Assert.That(result, Is.EqualTo(expectedResult), nameof(result));
    Assert.That(sessionId, Is.EqualTo(expectedSessionId), nameof(sessionId));
    Assert.That(sessionTimeout, Is.EqualTo(expectedSessionTimeout), nameof(sessionTimeout));
  }

  private static System.Collections.IEnumerable YieldTestCases_TryGetCookie_OfStringArray()
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

  [TestCaseSource(nameof(YieldTestCases_TryGetCookie_OfStringArray))]
  public void TryGetCookie_OfStringArray(
    string[]? inputs,
    bool expectedResult,
    string? expectedSessionId,
    int? expectedSessionTimeout
  )
  {
    var result = TapoSessionCookieUtils.TryGetCookie(inputs, out var sessionId, out var sessionTimeout);

    Assert.That(result, Is.EqualTo(expectedResult), nameof(result));
    Assert.That(sessionId, Is.EqualTo(expectedSessionId), nameof(sessionId));
    Assert.That(sessionTimeout, Is.EqualTo(expectedSessionTimeout), nameof(sessionTimeout));
  }

  private static System.Collections.IEnumerable YieldTestCases_TryGetCookie_OfHttpResponseMessage()
  {
    var response1 = new HttpResponseMessage();

    response1.Headers.Add("Set-Cookie", "TP_SESSIONID=XXXX;TIMEOUT=1440");

    yield return new object?[] {
      response1,
      true,
      "XXXX",
      1440
    };

    var response2 = new HttpResponseMessage();

    response2.Headers.Add("Set-Cookie", "SESSIONID=123456; Domain=example.com");

    yield return new object?[] {
      response2,
      false,
      null,
      null
    };

    var response3 = new HttpResponseMessage();

    yield return new object?[] {
      response3,
      false,
      null,
      null
    };

    HttpResponseMessage? response4 = null;

    yield return new object?[] {
      response4,
      false,
      null,
      null
    };
  }

  [TestCaseSource(nameof(YieldTestCases_TryGetCookie_OfHttpResponseMessage))]
  public void TryGetCookie_OfHttpResponseMessage(
    HttpResponseMessage? response,
    bool expectedResult,
    string? expectedSessionId,
    int? expectedSessionTimeout
  )
  {
    var result = TapoSessionCookieUtils.TryGetCookie(response!, out var sessionId, out var sessionTimeout);

    Assert.That(result, Is.EqualTo(expectedResult), nameof(result));
    Assert.That(sessionId, Is.EqualTo(expectedSessionId), nameof(sessionId));
    Assert.That(sessionTimeout, Is.EqualTo(expectedSessionTimeout), nameof(sessionTimeout));
  }
}
