// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

public sealed class StaticDeviceEndPoint : IDeviceEndPoint {
  private readonly EndPoint endPoint;

  public StaticDeviceEndPoint(EndPoint endPoint)
  {
    this.endPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
  }

  public ValueTask<EndPoint?> ResolveAsync(CancellationToken cancellationToken = default)
    => cancellationToken.IsCancellationRequested
      ?
#if SYSTEM_THREADING_TASKS_VALUETASK_FROMCANCELED
        ValueTask.FromCanceled<EndPoint?>(cancellationToken)
#else
        ValueTaskShim.FromCanceled<EndPoint?>(cancellationToken)
#endif
      : new(endPoint);

  public override string? ToString()
    => endPoint.ToString();
}
