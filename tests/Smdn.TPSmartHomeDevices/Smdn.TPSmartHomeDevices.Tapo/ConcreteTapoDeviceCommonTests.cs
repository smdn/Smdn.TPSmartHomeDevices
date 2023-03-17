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

    /*
     * (string hostName, string email, string password, IServiceProvider? serviceProvider)
     */
    yield return new object[] {
      new Type[] { typeof(string), typeof(string), typeof(string), typeof(IServiceProvider) },
      new object?[] { null, email, password, null },
      typeof(ArgumentNullException),
      "hostName"
    };
    yield return new object[] {
      new Type[] { typeof(string), typeof(string), typeof(string), typeof(IServiceProvider) },
      new object?[] { hostName, null, password, null },
      typeof(ArgumentNullException),
      "email"
    };
    yield return new object[] {
      new Type[] { typeof(string), typeof(string), typeof(string), typeof(IServiceProvider) },
      new object?[] { hostName, email, null, null },
      typeof(ArgumentNullException),
      "password"
    };

    /*
     * (IPAddress ipAddress, string email, string password, IServiceProvider? serviceProvider)
     */
    yield return new object[] {
      new Type[] { typeof(IPAddress), typeof(string), typeof(string), typeof(IServiceProvider) },
      new object?[] { null, email, password, null },
      typeof(ArgumentNullException),
      "ipAddress"
    };
    yield return new object[] {
      new Type[] { typeof(IPAddress), typeof(string), typeof(string), typeof(IServiceProvider) },
      new object?[] { IPAddress.Loopback, null, password, null },
      typeof(ArgumentNullException),
      "email"
    };
    yield return new object[] {
      new Type[] { typeof(IPAddress), typeof(string), typeof(string), typeof(IServiceProvider) },
      new object?[] { IPAddress.Loopback, email, null, null },
      typeof(ArgumentNullException),
      "password"
    };

    var services = new ServiceCollection();
    var endPointFactory = new NullMacAddressDeviceEndPointFactory();

    services.AddDeviceEndPointFactory(endPointFactory);

    /*
     * (PhysicalAddress macAddress, string email, string password, IDeviceEndPointFactory<PhysicalAddress> endPointFactory, IServiceProvider? serviceProvider)
     */
    yield return new object[] {
      new Type[] { typeof(PhysicalAddress), typeof(string), typeof(string), typeof(IDeviceEndPointFactory<PhysicalAddress>), typeof(IServiceProvider) },
      new object?[] { null, email, password, endPointFactory, null },
      typeof(ArgumentNullException),
      "macAddress"
    };
    yield return new object[] {
      new Type[] { typeof(PhysicalAddress), typeof(string), typeof(string), typeof(IDeviceEndPointFactory<PhysicalAddress>), typeof(IServiceProvider) },
      new object?[] { PhysicalAddress.None, null, password, endPointFactory, null },
      typeof(ArgumentNullException),
      "email"
    };
    yield return new object[] {
      new Type[] { typeof(PhysicalAddress), typeof(string), typeof(string), typeof(IDeviceEndPointFactory<PhysicalAddress>), typeof(IServiceProvider) },
      new object?[] { PhysicalAddress.None, email, null, endPointFactory, null },
      typeof(ArgumentNullException),
      "password"
    };
    yield return new object[] {
      new Type[] { typeof(PhysicalAddress), typeof(string), typeof(string), typeof(IDeviceEndPointFactory<PhysicalAddress>), typeof(IServiceProvider) },
      new object?[] { PhysicalAddress.None, email, password, null, null },
      typeof(ArgumentNullException),
      "endPointFactory"
    };
    yield return new object[] {
      new Type[] { typeof(PhysicalAddress), typeof(string), typeof(string), typeof(IDeviceEndPointFactory<PhysicalAddress>), typeof(IServiceProvider) },
      new object?[] { PhysicalAddress.None, email, password, endPointFactory, null },
      null,
      null
    };

    /*
     * (PhysicalAddress macAddress, string email, string password, IServiceProvider serviceProvider)
     */
    yield return new object[] {
      new Type[] { typeof(PhysicalAddress), typeof(string), typeof(string), typeof(IServiceProvider) },
      new object?[] { null, email, password, services.BuildServiceProvider() },
      typeof(ArgumentNullException),
      "macAddress"
    };
    yield return new object[] {
      new Type[] { typeof(PhysicalAddress), typeof(string), typeof(string), typeof(IServiceProvider) },
      new object?[] { PhysicalAddress.None, null, password, services.BuildServiceProvider() },
      typeof(ArgumentNullException),
      "email"
    };
    yield return new object[] {
      new Type[] { typeof(PhysicalAddress), typeof(string), typeof(string), typeof(IServiceProvider) },
      new object?[] { PhysicalAddress.None, email, null, services.BuildServiceProvider() },
      typeof(ArgumentNullException),
      "password"
    };
    yield return new object[] {
      new Type[] { typeof(PhysicalAddress), typeof(string), typeof(string), typeof(IServiceProvider) },
      new object?[] { PhysicalAddress.None, email, password, null },
      typeof(ArgumentNullException),
      "serviceProvider"
    };
    yield return new object[] {
      new Type[] { typeof(PhysicalAddress), typeof(string), typeof(string), typeof(IServiceProvider) },
      new object?[] { PhysicalAddress.None, email, password, new ServiceCollection().BuildServiceProvider() },
      typeof(InvalidOperationException),
      null
    };
  }

  internal static void TestCtor_ArgumentException<TTapoDevice>(
    Type[] ctorArgumentTypes,
    object?[] ctorParameters,
    Type? expectedExceptionType,
    string expectedParamName
  )
    where TTapoDevice : TapoDevice
  {
    var ctor = typeof(TTapoDevice).GetConstructor(
      bindingAttr: BindingFlags.Public | BindingFlags.Instance,
      types: ctorArgumentTypes
    );

    if (ctor is null)
      Assert.Fail("Constructor for test case was not found.");

    if (expectedExceptionType is null) {
      Assert.DoesNotThrow(() => ctor.Invoke(ctorParameters));
    }
    else {
      var ex = Assert.Throws<TargetInvocationException>(() => ctor.Invoke(ctorParameters));
      var actualException = ex.InnerException;

      Assert.IsInstanceOf(expectedExceptionType, actualException);

      if (actualException is ArgumentException argumentException)
        Assert.AreEqual(expectedParamName, argumentException.ParamName, nameof(argumentException.ParamName));
    }
  }
}
