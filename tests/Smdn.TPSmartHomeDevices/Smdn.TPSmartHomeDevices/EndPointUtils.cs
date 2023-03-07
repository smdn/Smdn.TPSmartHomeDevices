// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace Smdn.TPSmartHomeDevices;

internal static class EndPointUtils {
  // IANA suggested range for dynamic or private ports
  public const int MinDynamicPort = 49215;
  public const int MaxDynamicPort = 65535;

  public static IEnumerable<int> EnumerateIANASuggestedDynamicPorts(
    int? except
  )
  {
    var dynamicPorts = Enumerable.Range(start: MinDynamicPort, count: MaxDynamicPort - MinDynamicPort);

    return except.HasValue
      ? dynamicPorts.Where(port => port != except)
      : dynamicPorts;
  }

  // ref: https://stackoverflow.com/questions/223063/how-can-i-create-an-httplistener-class-on-a-random-port-in-c
  public static bool TryFindUnusedPort(int? except, out int port)
  {
    port = default;

    var listenerPorts = IPGlobalProperties
      .GetIPGlobalProperties()
      .GetActiveTcpListeners()
      .Select(static endPoint => endPoint.Port)
      .ToHashSet();

    for (var p = MinDynamicPort; p <= MaxDynamicPort; p++) {
      if (except == p)
        continue;

      if (!listenerPorts.Contains(p)) {
        port = p;
        return true;
      }
    }

    return false;
  }
}
