// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

[TestFixture]
[NonParallelizable]
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
    Assert.That(factory, Is.Not.Null, nameof(factory));
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
    Assert.That(factory, Is.Not.Null, nameof(factory));

    using var client = factory!.CreateClient(name: string.Empty);

    Assert.That(client, Is.Not.Null, nameof(client));
    Assert.That(client.Timeout, Is.EqualTo(timeout), nameof(client.Timeout));
    Assert.That(client.BaseAddress, Is.EqualTo(baseAddress), nameof(client.BaseAddress));
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
    Assert.That(factory, Is.Not.Null, nameof(factory));

    using var client = factory!.CreateClient(name: string.Empty);

    Assert.That(client, Is.Not.Null, nameof(client));
    Assert.That(client.Timeout, Is.EqualTo(timeout), nameof(client.Timeout));
    Assert.That(client.BaseAddress, Is.EqualTo(baseAddress), nameof(client.BaseAddress));
  }

  private readonly struct NullResult { }

  [Test]
  public void AddTapoHttpClient_ConfigureClient_BaseAddressMustBeOverwritten()
  {
    var services = new ServiceCollection();

    services.AddTapoCredential(
      "user",
      "pass"
    );
    services.AddTapoHttpClient(
      configureClient: client => {
        client.BaseAddress = new Uri("http://test.invalid/");
      }
    );

    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token",
      funcGeneratePassThroughResponse: static (_, _, _) => new(
        KnownErrorCodes.Success,
        new PassThroughResponse<NullResult>() {
          ErrorCode = KnownErrorCodes.Success,
          Result = new(),
        }
      )
    );

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services.BuildServiceProvider()
    );

    Assert.That(
      async () => await device.GetDeviceInfoAsync(),
      Throws.Nothing
    );
  }

  [Test]
  public void AddTapoHttpClient_ConfigureClient_ExceptionMustBeWrappedIntoInvalidOperationException()
  {
    const string ExceptionMessage = "configuring HTTP client is not implemented";
    var services = new ServiceCollection();

    services.AddTapoCredential(
      "user",
      "pass"
    );
    services.AddTapoHttpClient(
      configureClient: client => throw new NotImplementedException(message: ExceptionMessage)
    );

    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token",
      funcGeneratePassThroughResponse: static (_, _, _) => new(
        KnownErrorCodes.Success,
        new PassThroughResponse<NullResult>() {
          ErrorCode = KnownErrorCodes.Success,
          Result = new(),
        }
      )
    );

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services.BuildServiceProvider()
    );

    Assert.That(
      async () => await device.GetDeviceInfoAsync(),
      Throws
        .InvalidOperationException
        .With.InnerException.TypeOf<NotImplementedException>()
        .With.InnerException.Message.EqualTo(ExceptionMessage)
    );
  }
}
