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

  private readonly struct NullResult { }

  [Test]
  public async Task AddTapoHttpClient_ConfigureClient_ExceptionMustBeWrappedIntoInvalidOperationException()
  {
    const string exceptionMessage = "configuring HTTP client is not implemented";
    var services = new ServiceCollection();

    services.AddTapoCredential(
      "user",
      "pass"
    );
    services.AddTapoHttpClient(
      configureClient: client => throw new NotImplementedException(message: exceptionMessage)
    );

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = static (_, _, _) => new(
        KnownErrorCodes.Success,
        new GetDeviceInfoResponse<NullResult>() {
          ErrorCode = KnownErrorCodes.Success,
          Result = new(),
        }
      ),
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services.BuildServiceProvider()
    );

    var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await device.GetDeviceInfoAsync());

    Assert.IsNotNull(ex.InnerException, nameof(ex.InnerException));
    Assert.IsInstanceOf<NotImplementedException>(ex.InnerException, nameof(ex.InnerException));
    Assert.AreEqual(exceptionMessage, ex.InnerException!.Message);
  }
}
