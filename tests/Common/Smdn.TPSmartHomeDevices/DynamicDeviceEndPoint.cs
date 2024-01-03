// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

internal sealed class DynamicDeviceEndPoint : IDynamicDeviceEndPoint {
  public EndPoint? EndPoint { get; set; }
  public bool HasInvalidated { get; private set; }
  public event EventHandler? Invalidated;

  public DynamicDeviceEndPoint(EndPoint? endPoint)
  {
    EndPoint = endPoint;
  }

  public ValueTask<EndPoint?> ResolveAsync(CancellationToken cancellationToken)
    => ValueTask.FromResult(EndPoint);

  public void Invalidate()
  {
    try {
      Invalidated?.Invoke(this, EventArgs.Empty);
    }
    finally {
      HasInvalidated = true;
    }
  }
}
