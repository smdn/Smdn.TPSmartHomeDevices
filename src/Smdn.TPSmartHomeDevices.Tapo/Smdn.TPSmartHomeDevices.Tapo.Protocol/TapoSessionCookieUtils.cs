// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
// cSpell:ignore SESSIONID
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

public static class TapoSessionCookieUtils {
  private const string HeaderNameSetCookie = "Set-Cookie";

  // Format and example of Set-Cookie header sent back by the Tapo HTTP server.
  //   Set-Cookie: TP_SESSIONID=XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX;TIMEOUT=1440
  internal const string HttpCookiePrefixForSessionId = "TP_SESSIONID="; // TP_SESSIONID=XXXXXX
  internal const string HttpCookieAttributePrefixForTimeout = "TIMEOUT="; // TIMEOUT=xxxx

  public static bool TryGetCookie(
    HttpResponseMessage response,
    out string? sessionId,
    out int? sessionTimeout
  )
  {
    sessionId = default;
    sessionTimeout = default;

    if (response is null)
      return false;

    return
      response.Headers.TryGetValues(HeaderNameSetCookie, out var setCookieValues) &&
      TryGetCookie(setCookieValues, out sessionId, out sessionTimeout);
  }

  public static bool TryGetCookie(
    IEnumerable<string>? cookieValues,
    out string? sessionId,
    out int? sessionTimeout
  )
  {
    sessionId = default;
    sessionTimeout = default;

    var cookieValueOfTPSessionId = cookieValues?.FirstOrDefault(
      static val => val.StartsWith(HttpCookiePrefixForSessionId, StringComparison.Ordinal)
    );

    return
      cookieValueOfTPSessionId is not null &&
      TryParseCookie(
        cookieValueOfTPSessionId,
        id: out sessionId,
        timeout: out sessionTimeout
      );
  }

  // We cannot use System.Net.Cookie and its relevant collection classes to handle cookies
  // since the Tapo HTTP server uses non-standard cookie attribute 'TIMEOUT'.
  public static bool TryParseCookie(
    ReadOnlySpan<char> cookie,
    out string? id,
    out int? timeout
  )
  {
    id = default;
    timeout = default;

    var indexOfSessionIdPrefix = cookie.IndexOf(HttpCookiePrefixForSessionId, StringComparison.Ordinal);

    if (indexOfSessionIdPrefix < 0)
      return false;

    var cookieName = cookie.Slice(indexOfSessionIdPrefix + HttpCookiePrefixForSessionId.Length);
    var indexOfCookieNameTerminator = cookieName.IndexOf(';');

    ReadOnlySpan<char> cookieAttributes;

    if (0 <= indexOfCookieNameTerminator) {
      id = cookieName.Slice(0, indexOfCookieNameTerminator).TrimEnd().ToString(); // for the case of "TP_SESSIONID=XXXXXXXXXX ;"

      if (id.Length == 0)
        return false;

      cookieAttributes = cookie.Slice(indexOfSessionIdPrefix + HttpCookiePrefixForSessionId.Length + indexOfCookieNameTerminator);
    }
    else {
      id = cookieName.TrimEnd().ToString(); // for the case of "TP_SESSIONID=XXXXXXXXXX"
      return id.Length != 0; // no attributes
    }

    var indexOfTimeoutPrefix = cookieAttributes.IndexOf(HttpCookieAttributePrefixForTimeout, StringComparison.Ordinal);

    if (0 <= indexOfTimeoutPrefix) {
      var timeoutAttributeValue = cookieAttributes.Slice(indexOfTimeoutPrefix + HttpCookieAttributePrefixForTimeout.Length);
      var indexOfTimeoutTerminator = timeoutAttributeValue.IndexOf(';');

      if (0 <= indexOfTimeoutTerminator)
        // for the case of "TIMEOUT=XXXXXXXXXX ;"
        // no need to trim whitespaces here since NumberStyles.Integer trims leading and trailing white spaces
        timeoutAttributeValue = timeoutAttributeValue.Slice(0, indexOfTimeoutTerminator);
      else
        timeoutAttributeValue = timeoutAttributeValue.TrimEnd(); // for the case of "TIMEOUT=XXXXXXXXXX"

      const NumberStyles TimeoutNumberStyles = NumberStyles.AllowTrailingWhite;

      if (int.TryParse(timeoutAttributeValue, TimeoutNumberStyles, provider: null, out var to))
        timeout = to;
    }

    return true;
  }
}
