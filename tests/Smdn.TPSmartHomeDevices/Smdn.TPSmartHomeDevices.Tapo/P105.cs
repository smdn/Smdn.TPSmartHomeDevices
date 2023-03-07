// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo;

[TestFixture]
public class P105Tests {
  private static System.Collections.IEnumerable YiledTestCases_Ctor_ArgumentNull()
  {
    yield return new object[] {
      new TestDelegate(() => new P105(hostName: null!)),
      "hostName"
    };

    yield return new object[] {
      new TestDelegate(() => new P105(ipAddress: null!, email: "user@mail.test", password: "pass")),
      "ipAddress"
    };

    yield return new object[] {
      new TestDelegate(() => new P105(deviceEndPointProvider: null!)),
      "deviceEndPointProvider"
    };

    yield return new object[] {
      new TestDelegate(() => new P105(hostName: null!, email: "user@mail.test", password: "pass")),
      "hostName"
    };
    yield return new object[] {
      new TestDelegate(() => new P105(hostName: "localhost", email: null!, password: "pass")),
      "email"
    };
    yield return new object[] {
      new TestDelegate(() => new P105(hostName: "localhost", email: "user@mail.test", password: null!)),
      "password"
    };
  }

  [TestCaseSource(nameof(YiledTestCases_Ctor_ArgumentNull))]
  public void Ctor_ArgumentNull(TestDelegate testAction, string expectedParamName)
  {
    var ex = Assert.Throws<ArgumentNullException>(testAction)!;

    Assert.AreEqual(expectedParamName, ex.ParamName, nameof(ex.ParamName));
  }
}

