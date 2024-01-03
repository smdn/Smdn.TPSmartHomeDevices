// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Kasa;

[TestFixture]
public class KasaDeviceExceptionHandlerServiceCollectionExtensionsTests {
  private class CustomExceptionHandler : KasaDeviceExceptionHandler {
    public override KasaDeviceExceptionHandling DetermineHandling(
      KasaDevice device,
      Exception exception,
      int attempt,
      ILogger? logger
    ) => throw new NotImplementedException();
  }

  [Test]
  public void AddKasaDeviceExceptionHandler()
  {
    var services = new ServiceCollection();

    services.AddKasaDeviceExceptionHandler(new CustomExceptionHandler());

    var exceptionHandler = services.BuildServiceProvider().GetRequiredService<KasaDeviceExceptionHandler>();

    Assert.That(exceptionHandler, Is.Not.Null, nameof(exceptionHandler));
  }

  [Test]
  public void AddKasaDeviceExceptionHandler_ArgumentNull()
  {
    var services = new ServiceCollection();

    Assert.Throws<ArgumentNullException>(
      () => services.AddKasaDeviceExceptionHandler(exceptionHandler: null!)
    );
  }

  [Test]
  public void AddKasaDeviceExceptionHandler_TryAddMultiple()
  {
    var services = new ServiceCollection();

    var first = new CustomExceptionHandler();
    var second = new CustomExceptionHandler();

    services.AddKasaDeviceExceptionHandler(first);
    services.AddKasaDeviceExceptionHandler(second);

    var exceptionHandler = services.BuildServiceProvider().GetRequiredService<KasaDeviceExceptionHandler>();

    Assert.That(exceptionHandler, Is.SameAs(first), "first one must be returned");
  }
}
