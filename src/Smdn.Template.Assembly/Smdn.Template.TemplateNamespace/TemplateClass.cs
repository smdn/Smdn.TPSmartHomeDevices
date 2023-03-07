// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Template.TemplateNamespace;

public class TemplateClass {
  public static int TemplateMethod(int n)
  {
    if (n == 0)
      throw new ArgumentOutOfRangeException(nameof(n), n, "must be non zero value");

    return n;
  }
}
