// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

internal sealed class DynamicDeviceEndPointProvider : IDynamicDeviceEndPointProvider {
  public EndPoint EndPoint { get; set; }
  public bool HasInvalidated { get; private set; }
  public event EventHandler Invalidated;

  public DynamicDeviceEndPointProvider(EndPoint endPoint)
  {
    EndPoint = endPoint;
  }

  public ValueTask<EndPoint?> GetEndPointAsync(CancellationToken cancellationToken)
    => ValueTask.FromResult(EndPoint);

  public void InvalidateEndPoint()
  {
    try {
      Invalidated?.Invoke(this, EventArgs.Empty);
    }
    finally {
      HasInvalidated = true;
    }
  }
}
