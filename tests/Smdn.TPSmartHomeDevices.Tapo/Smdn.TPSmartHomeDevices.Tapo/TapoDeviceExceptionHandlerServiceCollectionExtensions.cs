// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo;

[TestFixture]
public class TapoDeviceExceptionHandlerServiceCollectionExtensionsTests {
  private class CustomExceptionHandler : TapoDeviceExceptionHandler {
    public override TapoDeviceExceptionHandling DetermineHandling(
      TapoDevice device,
      Exception exception,
      int attempt,
      ILogger? logger
    ) => throw new NotImplementedException();
  }

  [Test]
  public void AddTapoDeviceExceptionHandler()
  {
    var services = new ServiceCollection();

    services.AddTapoDeviceExceptionHandler(new CustomExceptionHandler());

    var exceptionHandler = services.BuildServiceProvider().GetRequiredService<TapoDeviceExceptionHandler>();

    Assert.That(exceptionHandler, Is.Not.Null, nameof(exceptionHandler));
  }

  [Test]
  public void AddTapoDeviceExceptionHandler_ArgumentNull()
  {
    var services = new ServiceCollection();

    Assert.Throws<ArgumentNullException>(
      () => services.AddTapoDeviceExceptionHandler(exceptionHandler: null!)
    );
  }

  [Test]
  public void AddTapoDeviceExceptionHandler_TryAddMultiple()
  {
    var services = new ServiceCollection();

    var first = new CustomExceptionHandler();
    var second = new CustomExceptionHandler();

    services.AddTapoDeviceExceptionHandler(first);
    services.AddTapoDeviceExceptionHandler(second);

    var exceptionHandler = services.BuildServiceProvider().GetRequiredService<TapoDeviceExceptionHandler>();

    Assert.That(exceptionHandler, Is.SameAs(first), "first one must be returned");
  }
}
