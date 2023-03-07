// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using NUnit.Framework;

namespace Smdn.Template.TemplateNamespace;

[TestFixture]
public class ClassTests {
  [Test]
  public void Method_Zero() => Assert.Throws<ArgumentOutOfRangeException>(() => TemplateClass.TemplateMethod(0));

  [TestCase(1)]
  [TestCase(-1)]
  public void Method_NonZero(int n) => Assert.AreEqual(n, TemplateClass.TemplateMethod(n));
}
