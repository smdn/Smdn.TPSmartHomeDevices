// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

public abstract class TapoSessionProtocolSelector {
  public abstract TapoSessionProtocol? SelectProtocol(TapoDevice device);

  internal static readonly TapoSessionProtocolSelector Default = new DefaultSelector();

  private sealed class DefaultSelector : TapoSessionProtocolSelector {
    public override TapoSessionProtocol? SelectProtocol(TapoDevice device) => null;
  }
}
