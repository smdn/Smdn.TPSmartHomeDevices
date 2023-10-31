// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

public abstract class TapoSessionProtocolSelector {
  public abstract TapoSessionProtocol? SelectProtocol(TapoDevice device);

  internal static readonly TapoSessionProtocolSelector Default = new DefaultSelector();

  private sealed class DefaultSelector : TapoSessionProtocolSelector {
    public override TapoSessionProtocol? SelectProtocol(TapoDevice device) => null;
  }

  internal sealed class FuncSelector : TapoSessionProtocolSelector {
    private readonly Func<TapoDevice, TapoSessionProtocol?> selectProtocol;

    public FuncSelector(Func<TapoDevice, TapoSessionProtocol?> selectProtocol)
    {
      this.selectProtocol = selectProtocol ?? throw new ArgumentNullException(nameof(selectProtocol));
    }

    public override TapoSessionProtocol? SelectProtocol(TapoDevice device)
      => selectProtocol(device);
  }
}
