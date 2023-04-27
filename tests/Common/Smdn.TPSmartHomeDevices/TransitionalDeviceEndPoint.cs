// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

internal sealed class TransitionalDeviceEndPoint : IDeviceEndPoint {
  private int current;
  private readonly EndPoint[] endPoints;

  public TransitionalDeviceEndPoint(IEnumerable<EndPoint> endPoints)
  {
    this.endPoints = endPoints.ToArray();
  }

  public ValueTask<EndPoint?> ResolveAsync(CancellationToken cancellationToken)
  {
    if (endPoints.Length <= current)
      throw new InvalidOperationException("transitioned to the maximum value");

    return new(endPoints[current++]);
  }
}
