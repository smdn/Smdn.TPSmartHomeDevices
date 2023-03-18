// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo;

internal class ConcreteTapoDeviceCommonTests {
  internal static System.Collections.IEnumerable YiledTestCases_Ctor_ArgumentException()
  {
    const string hostName = "localhost";
    const string email = "user@mail.test";
    const string password = "password";

    var services = new ServiceCollection();

    /*
     * (string hostName, string email, string password, IServiceProvider? serviceProvider)
     */
    var parameterTypes = new[] { typeof(string), typeof(string), typeof(string), typeof(IServiceProvider) };

    yield return new object[] {
      parameterTypes,
      new object?[] { null, email, password, null },
      typeof(ArgumentNullException),
      "hostName"
    };
    yield return new object[] {
      parameterTypes,
      new object?[] { hostName, null, password, null },
      typeof(ArgumentNullException),
      "email"
    };
    yield return new object[] {
      parameterTypes,
      new object?[] { hostName, email, null, null },
      typeof(ArgumentNullException),
      "password"
    };

    /*
     * (string hostName, IServiceProvider serviceProvider)
     */
    parameterTypes = new[] { typeof(string), typeof(IServiceProvider) };

    yield return new object[] {
      parameterTypes,
      new object?[] { null, services.BuildServiceProvider() },
      typeof(ArgumentNullException),
      "hostName"
    };
    yield return new object[] {
      parameterTypes,
      new object?[] { hostName, null },
      typeof(ArgumentNullException),
      "serviceProvider"
    };
    yield return new object[] {
      parameterTypes,
      new object?[] { hostName, new ServiceCollection().BuildServiceProvider() },
      typeof(InvalidOperationException),
      null
    };

    /*
     * (IPAddress ipAddress, string email, string password, IServiceProvider? serviceProvider)
     */
    parameterTypes = new[] { typeof(IPAddress), typeof(string), typeof(string), typeof(IServiceProvider) };

    yield return new object[] {
      parameterTypes,
      new object?[] { null, email, password, null },
      typeof(ArgumentNullException),
      "ipAddress"
    };
    yield return new object[] {
      parameterTypes,
      new object?[] { IPAddress.Loopback, null, password, null },
      typeof(ArgumentNullException),
      "email"
    };
    yield return new object[] {
      parameterTypes,
      new object?[] { IPAddress.Loopback, email, null, null },
      typeof(ArgumentNullException),
      "password"
    };

    /*
     * (IPAddress ipAddress, IServiceProvider serviceProvider)
     */
    parameterTypes = new[] { typeof(IPAddress), typeof(IServiceProvider) };

    yield return new object[] {
      parameterTypes,
      new object?[] { null, null },
      typeof(ArgumentNullException),
      "ipAddress"
    };
    yield return new object[] {
      parameterTypes,
      new object?[] { IPAddress.Loopback, null },
      typeof(ArgumentNullException),
      "serviceProvider"
    };
    yield return new object[] {
      parameterTypes,
      new object?[] { IPAddress.Loopback, new ServiceCollection().BuildServiceProvider() },
      typeof(InvalidOperationException),
      null
    };

    /*
     * (PhysicalAddress macAddress, string email, string password, IServiceProvider serviceProvider)
     */
    parameterTypes = new[] { typeof(PhysicalAddress), typeof(string), typeof(string), typeof(IServiceProvider) };

    var endPointFactory = new NullMacAddressDeviceEndPointFactory();

    services = new ServiceCollection();
    services.AddDeviceEndPointFactory(endPointFactory);

    yield return new object[] {
      parameterTypes,
      new object?[] { null, email, password, services.BuildServiceProvider() },
      typeof(ArgumentNullException),
      "macAddress"
    };
    yield return new object[] {
      parameterTypes,
      new object?[] { PhysicalAddress.None, null, password, services.BuildServiceProvider() },
      typeof(ArgumentNullException),
      "email"
    };
    yield return new object[] {
      parameterTypes,
      new object?[] { PhysicalAddress.None, email, null, services.BuildServiceProvider() },
      typeof(ArgumentNullException),
      "password"
    };
    yield return new object[] {
      parameterTypes,
      new object?[] { PhysicalAddress.None, email, password, null },
      typeof(ArgumentNullException),
      "serviceProvider"
    };
    yield return new object[] {
      parameterTypes,
      new object?[] { PhysicalAddress.None, email, password, new ServiceCollection().BuildServiceProvider() },
      typeof(InvalidOperationException),
      null
    };

    /*
     * (PhysicalAddress macAddress, IServiceProvider serviceProvider)
     */
    parameterTypes = new[] { typeof(PhysicalAddress), typeof(IServiceProvider) };

    yield return new object[] {
      parameterTypes,
      new object?[] { null, services.BuildServiceProvider() },
      typeof(ArgumentNullException),
      "macAddress"
    };
    yield return new object[] {
      parameterTypes,
      new object?[] { PhysicalAddress.None, null },
      typeof(ArgumentNullException),
      "serviceProvider"
    };
    yield return new object[] {
      parameterTypes,
      new object?[] { PhysicalAddress.None, new ServiceCollection().BuildServiceProvider() },
      typeof(InvalidOperationException),
      null
    };

    /*
     * IDeviceEndPointProvider deviceEndPointProvider, ITapoCredentialProvider? credentialProvider = null, IServiceProvider? serviceProvider = null
     */
    parameterTypes = new[] { typeof(IDeviceEndPointProvider), typeof(ITapoCredentialProvider), typeof(IServiceProvider) };

    services = new ServiceCollection();
    services.AddTapoCredential(email, password);
    services.AddDeviceEndPointFactory(endPointFactory);

    var credential = services.BuildServiceProvider().GetRequiredService<ITapoCredentialProvider>();

    yield return new object[] {
      parameterTypes,
      new object?[] { null, credential, services.BuildServiceProvider() },
      typeof(ArgumentNullException),
      "deviceEndPointProvider"
    };
    yield return new object[] {
      parameterTypes,
      new object?[] { new ThrowExceptionDeviceEndPointProvider(), null, new ServiceCollection().BuildServiceProvider() },
      typeof(InvalidOperationException),
      null
    };
    yield return new object[] {
      parameterTypes,
      new object?[] { new ThrowExceptionDeviceEndPointProvider(), null, null },
      typeof(ArgumentNullException),
      "serviceProvider"
    };
  }

  internal static void TestCtor_ArgumentException<TTapoDevice>(
    Type[] ctorParameterTypes,
    object?[] ctorParameters,
    Type? expectedExceptionType,
    string expectedParamName
  )
    where TTapoDevice : TapoDevice
  {
    var ctor = typeof(TTapoDevice).GetConstructor(
      bindingAttr: BindingFlags.Public | BindingFlags.Instance,
      types: ctorParameterTypes
    );

    if (ctor is null)
      Assert.Fail("Constructor for test case was not found.");

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

    if (method is null)
      Assert.Fail("Method for test case was not found.");

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
      var actualException = ex.InnerException;

      Assert.IsInstanceOf(expectedExceptionType, actualException);

      if (actualException is ArgumentException argumentException)
        Assert.AreEqual(expectedParamName, argumentException.ParamName, nameof(argumentException.ParamName));
    }
  }
}
