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
    public IServiceProvider? ServiceProvider { get; }

    public NullDeviceEndPointFactory()
    {
    }

    public NullDeviceEndPointFactory(IServiceProvider serviceProvider)
    {
      ServiceProvider = serviceProvider;
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

    Assert.That((actualEndPointFactory as NullDeviceEndPointFactory)?.ServiceProvider, Is.Null);
  }

  [Test]
  public void AddDeviceEndPointFactory_WithImplementationFactory()
  {
    var services = new ServiceCollection();
    IDeviceEndPointFactory<string>? expectedEndPointFactory = null;

    services.AddDeviceEndPointFactory(
      serviceProvider => {
        expectedEndPointFactory = new NullDeviceEndPointFactory(serviceProvider);

        return expectedEndPointFactory;
      }
    );

    var serviceProvider = services.BuildServiceProvider();
    var actualEndPointFactory = serviceProvider.GetRequiredService<IDeviceEndPointFactory<string>>();

    Assert.That(
      actualEndPointFactory,
      Is.SameAs(expectedEndPointFactory)
    );

    Assert.That((actualEndPointFactory as NullDeviceEndPointFactory)?.ServiceProvider, Is.Not.Null);
  }
}