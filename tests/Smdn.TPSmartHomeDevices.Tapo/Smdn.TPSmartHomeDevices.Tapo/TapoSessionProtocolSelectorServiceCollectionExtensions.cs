// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

[TestFixture]
public class TapoSessionProtocolSelectorServiceCollectionExtensionsTests {
  private class NullTapoSessionProtocolSelector : TapoSessionProtocolSelector {
    public override TapoSessionProtocol? SelectProtocol(TapoDevice device) => throw new NotImplementedException();
  }

  [Test]
  public void AddTapoProtocolSelector_TapoSessionProtocolSelector()
  {
    var services = new ServiceCollection();
    var selector = new NullTapoSessionProtocolSelector();

    services.AddTapoProtocolSelector(selector: selector);

    TapoSessionProtocolSelector? addedSelector = null;

    Assert.DoesNotThrow(
      () => addedSelector = services.BuildServiceProvider().GetRequiredService<TapoSessionProtocolSelector>()
    );
    Assert.That(addedSelector, Is.Not.Null);
    Assert.That(addedSelector, Is.SameAs(selector));
  }

  [Test]
  public void AddTapoProtocolSelector_TapoSessionProtocolSelector_Null()
  {
    var services = new ServiceCollection();

    Assert.Throws<ArgumentNullException>(
      () => services.AddTapoProtocolSelector(selector: (TapoSessionProtocolSelector)null!)
    );

    Assert.Throws<InvalidOperationException>(
      () => services.BuildServiceProvider().GetRequiredService<TapoSessionProtocolSelector>()
    );
  }

  [Test]
  public void AddTapoProtocolSelector_FuncProtocolSelector()
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
  public void AddTapoProtocolSelector_FuncProtocolSelector_Null()
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
