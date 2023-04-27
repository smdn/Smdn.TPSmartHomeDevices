// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo;

[TestFixture]
public class TapoHttpClientFactoryServiceCollectionExtensionsTests {
  [Test]
  public void AddTapoHttpClient()
  {
    var services = new ServiceCollection();

    services.AddTapoHttpClient();

    IHttpClientFactory? factory = null;

    Assert.DoesNotThrow(
      () => factory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>()
    );
    Assert.IsNotNull(factory, nameof(factory));
  }

  [Test]
  public void AddTapoHttpClient_ConfigureClient()
  {
    var services = new ServiceCollection();
    var timeout = TimeSpan.FromSeconds(1);
    var baseAddress = new Uri("http://test.invalid/");

    services.AddTapoHttpClient(
      configureClient: client => {
        client.Timeout = timeout;
        client.BaseAddress = baseAddress;
      }
    );

    IHttpClientFactory? factory = null;

    Assert.DoesNotThrow(
      () => factory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>()
    );
    Assert.IsNotNull(factory, nameof(factory));

    using var client = factory!.CreateClient(name: string.Empty);

    Assert.IsNotNull(client, nameof(client));
    Assert.AreEqual(timeout, client.Timeout, nameof(client.Timeout));
    Assert.AreEqual(baseAddress, client.BaseAddress, nameof(client.BaseAddress));
  }

  [Test]
  public void AddTapoHttpClient_TryAddMultiple()
  {
    var services = new ServiceCollection();
    var timeout = TimeSpan.FromSeconds(1);
    var baseAddress = new Uri("http://test.invalid/");

    services.AddTapoHttpClient(
      configureClient: client => {
        client.Timeout = timeout;
        client.BaseAddress = baseAddress;
      }
    );
    services.AddTapoHttpClient(
      configureClient: static _ => throw new InvalidOperationException("this must not be invoked")
    );

    IHttpClientFactory? factory = null;

    Assert.DoesNotThrow(
      () => factory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>()
    );
    Assert.IsNotNull(factory, nameof(factory));

    using var client = factory!.CreateClient(name: string.Empty);

    Assert.IsNotNull(client, nameof(client));
    Assert.AreEqual(timeout, client.Timeout, nameof(client.Timeout));
    Assert.AreEqual(baseAddress, client.BaseAddress, nameof(client.BaseAddress));
  }
}
