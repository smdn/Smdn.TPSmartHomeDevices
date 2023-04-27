// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.Sockets;

namespace Smdn.TPSmartHomeDevices;

public class CustomEndPoint : EndPoint, IEquatable<CustomEndPoint> {
  public override AddressFamily AddressFamily => System.Net.Sockets.AddressFamily.Unknown;
  public int Port { get; }

  public CustomEndPoint(int port)
  {
    Port = port;
  }

  public override bool Equals(object? obj)
    => obj switch {
      CustomEndPoint ep => Equals(ep),
      null => false,
    };

  public bool Equals(CustomEndPoint? other)
    => other is not null && this.Port == other.Port;

  public override int GetHashCode()
    => Port.GetHashCode();

  public override string ToString()
    => $"{nameof(CustomEndPoint)}:{Port}";
}
