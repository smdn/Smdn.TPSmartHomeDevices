// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo;

[TestFixture]
public class P105Tests {
  [TestCaseSource(typeof(ConcreteTapoDeviceCommonTests), nameof(ConcreteTapoDeviceCommonTests.YiledTestCases_Ctor_ArgumentException))]
  public void Ctor_ArgumentException(Type[] ctorParameterTypes, object?[] ctorParameters, Type? expectedExceptionType, string expectedParamName)
    => ConcreteTapoDeviceCommonTests.TestCtor_ArgumentException<P105>(ctorParameterTypes, ctorParameters, expectedExceptionType, expectedParamName);
}

