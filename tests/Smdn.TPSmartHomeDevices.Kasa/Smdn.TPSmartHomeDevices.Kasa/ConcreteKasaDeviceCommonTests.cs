// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Kasa;

internal class ConcreteKasaDeviceCommonTests {
  internal static System.Collections.IEnumerable YiledTestCases_Ctor_ArgumentException()
  {
    /*
     * (string host, IServiceProvider? serviceProvider)
     */
    yield return new object[] {
      new Type[] { typeof(string), typeof(IServiceProvider) },
      new object?[] { null, null },
      typeof(ArgumentNullException),
      "host"
    };

    /*
     * (IPAddress ipAddress, IServiceProvider? serviceProvider)
     */
    yield return new object[] {
      new Type[] { typeof(IPAddress), typeof(IServiceProvider) },
      new object?[] { null, null },
      typeof(ArgumentNullException),
      "ipAddress"
    };

    var services = new ServiceCollection();
    var endPointFactory = new NullMacAddressDeviceEndPointFactory();

    services.AddDeviceEndPointFactory(endPointFactory);

    /*
     * (PhysicalAddress macAddress, IServiceProvider serviceProvider)
     */
    yield return new object?[] {
      new Type[] { typeof(PhysicalAddress), typeof(IServiceProvider) },
      new object?[] { null, services.BuildServiceProvider() },
      typeof(ArgumentNullException),
      "macAddress"
    };
    yield return new object?[] {
      new Type[] { typeof(PhysicalAddress), typeof(IServiceProvider) },
      new object?[] { PhysicalAddress.None, null },
      typeof(ArgumentNullException),
      "serviceProvider"
    };
    yield return new object?[] {
      new Type[] { typeof(PhysicalAddress), typeof(IServiceProvider) },
      new object?[] { PhysicalAddress.None, new ServiceCollection().BuildServiceProvider() },
      typeof(InvalidOperationException),
      null
    };
  }

  internal static void TestCtor_ArgumentException<TKasaDevice>(
    Type[] ctorParameterTypes,
    object?[] ctorParameters,
    Type? expectedExceptionType,
    string expectedParamName
  )
    where TKasaDevice : KasaDevice
  {
    var ctor = typeof(TKasaDevice).GetConstructor(
      bindingAttr: BindingFlags.Public | BindingFlags.Instance,
      types: ctorParameterTypes
    );

    if (ctor is null) {
      Assert.Fail("Constructor for test case was not found.");
      return;
    }

    TestCreate_ArgumentException(
      ctor,
      ctorParameters,
      expectedExceptionType,
      expectedParamName
    );
  }

  internal static void TestCreate_ArgumentException(
    Type type,
    string methodName,
    Type[] methodParameterTypes,
    object?[] methodParameters,
    Type? expectedExceptionType,
    string expectedParamName
  )
  {
    var method = type.GetMethod(
      name: methodName,
      bindingAttr: BindingFlags.Public | BindingFlags.Static,
      types: methodParameterTypes
    );

    if (method is null) {
      Assert.Fail("Method for test case was not found.");
      return;
    }

    TestCreate_ArgumentException(
      method,
      methodParameters,
      expectedExceptionType,
      expectedParamName
    );
  }

  private static void TestCreate_ArgumentException(
    MethodBase method,
    object?[] methodParameters,
    Type? expectedExceptionType,
    string expectedParamName
  )
  {
    if (expectedExceptionType is null) {
      Assert.DoesNotThrow(
        () => {
          if (method is ConstructorInfo ctor)
            ctor.Invoke(methodParameters);
          else
            method.Invoke(null, methodParameters);
        }
      );
    }
    else {
      var ex = Assert.Throws<TargetInvocationException>(
        () => {
          if (method is ConstructorInfo ctor)
            ctor.Invoke(methodParameters);
          else
            method.Invoke(null, methodParameters);
        }
      );

      var actualException = ex!.InnerException!;

      Assert.IsInstanceOf(expectedExceptionType, actualException);

      if (actualException is ArgumentException argumentException)
        Assert.AreEqual(expectedParamName, argumentException.ParamName, nameof(argumentException.ParamName));
    }
  }
}
