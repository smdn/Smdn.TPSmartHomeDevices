// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

[TestFixture]
public class TapoSessionProtocolSelectorServiceCollectionExtensionsTests {
  [Test]
  public void AddTapoProtocolSelector()
  {
    var services = new ServiceCollection();

    services.AddTapoProtocolSelector(
      static _ => TapoSessionProtocol.Klap
    );

    TapoSessionProtocolSelector? selector = null;

    Assert.DoesNotThrow(
      () => selector = services.BuildServiceProvider().GetRequiredService<TapoSessionProtocolSelector>()
    );
    Assert.That(selector, Is.Not.Null, nameof(selector));
  }

  [Test]
  public void AddTapoProtocolSelector_Null()
  {
    var services = new ServiceCollection();

    Assert.Throws<ArgumentNullException>(
      () => services.AddTapoProtocolSelector(selectProtocol: null!)
    );

    Assert.Throws<InvalidOperationException>(
      () => services.BuildServiceProvider().GetRequiredService<TapoSessionProtocolSelector>()
    );
  }
}
