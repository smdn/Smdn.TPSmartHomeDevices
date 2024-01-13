// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo;

[TestFixture]
public class P110MTests {
  private ServiceCollection? services;

  [OneTimeSetUp]
  public void SetUp()
  {
    services = new ServiceCollection();

    services.AddTapoBase64EncodedCredential(
      base64UserNameSHA1Digest: Convert.ToBase64String(Encoding.UTF8.GetBytes("user")),
      base64Password: Convert.ToBase64String(Encoding.UTF8.GetBytes("pass"))
    );
  }

  [Test]
  public new void ToString()
    => ConcreteTapoDeviceCommonTests.TestToString<P110M>();

  [TestCaseSource(typeof(ConcreteTapoDeviceCommonTests), nameof(ConcreteTapoDeviceCommonTests.YiledTestCases_Ctor_ArgumentException))]
  public void Ctor_ArgumentException(Type[] ctorParameterTypes, object?[] ctorParameters, Type? expectedExceptionType, string expectedParamName)
    => ConcreteTapoDeviceCommonTests.TestCtor_ArgumentException<P110M>(ctorParameterTypes, ctorParameters, expectedExceptionType, expectedParamName);
}
