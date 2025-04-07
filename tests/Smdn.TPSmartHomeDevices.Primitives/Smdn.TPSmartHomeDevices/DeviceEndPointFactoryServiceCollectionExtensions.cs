// SPDX-FileCopyrightText: 2025 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net.NetworkInformation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices;

[TestFixture]
public class DeviceEndPointFactoryServiceCollectionExtensionsTests {
  private class NullDeviceEndPointFactory : IDeviceEndPointFactory<string> {
    public NullDeviceEndPointFactory()
    {
    }

    public IDeviceEndPoint Create(string address)
      => throw new NotImplementedException();
  }

  [Test]
  public void AddDeviceEndPointFactory()
  {
    var services = new ServiceCollection();
    var expectedEndPointFactory = new NullDeviceEndPointFactory();

    services.AddDeviceEndPointFactory(expectedEndPointFactory);

    var serviceProvider = services.BuildServiceProvider();
    var actualEndPointFactory = serviceProvider.GetRequiredService<IDeviceEndPointFactory<string>>();

    Assert.That(
      actualEndPointFactory,
      Is.SameAs(expectedEndPointFactory)
    );
  }
}