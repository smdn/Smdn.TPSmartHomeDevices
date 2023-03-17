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
     * (string hostName, IServiceProvider? serviceProvider)
     */
    yield return new object[] {
      new Type[] { typeof(string), typeof(IServiceProvider) },
      new object?[] { null, null },
      typeof(ArgumentNullException),
      "hostName"
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
     * (PhysicalAddress macAddress, IDeviceEndPointFactory<PhysicalAddress> endPointFactory, IServiceProvider? serviceProvider)
     */
    yield return new object[] {
      new Type[] { typeof(PhysicalAddress), typeof(IDeviceEndPointFactory<PhysicalAddress>), typeof(IServiceProvider) },
      new object?[] { null, endPointFactory, null },
      typeof(ArgumentNullException),
      "macAddress"
    };
    yield return new object[] {
      new Type[] { typeof(PhysicalAddress), typeof(IDeviceEndPointFactory<PhysicalAddress>), typeof(IServiceProvider) },
      new object?[] { PhysicalAddress.None, null, null },
      typeof(ArgumentNullException),
      "endPointFactory"
    };
    yield return new object[] {
      new Type[] { typeof(PhysicalAddress), typeof(IDeviceEndPointFactory<PhysicalAddress>), typeof(IServiceProvider) },
      new object?[] { PhysicalAddress.None, endPointFactory, null },
      null,
      null
    };

    /*
     * (PhysicalAddress macAddress, IServiceProvider serviceProvider)
     */
    yield return new object[] {
      new Type[] { typeof(PhysicalAddress), typeof(IServiceProvider) },
      new object?[] { null, services.BuildServiceProvider() },
      typeof(ArgumentNullException),
      "macAddress"
    };
    yield return new object[] {
      new Type[] { typeof(PhysicalAddress), typeof(IServiceProvider) },
      new object?[] { PhysicalAddress.None, null },
      typeof(ArgumentNullException),
      "serviceProvider"
    };
    yield return new object[] {
      new Type[] { typeof(PhysicalAddress), typeof(IServiceProvider) },
      new object?[] { PhysicalAddress.None, new ServiceCollection().BuildServiceProvider() },
      typeof(InvalidOperationException),
      null
    };
  }

  internal static void TestCtor_ArgumentException<TKasaDevice>(
    Type[] ctorArgumentTypes,
    object?[] ctorParameters,
    Type? expectedExceptionType,
    string expectedParamName
  )
    where TKasaDevice : KasaDevice
  {
    var ctor = typeof(TKasaDevice).GetConstructor(
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
