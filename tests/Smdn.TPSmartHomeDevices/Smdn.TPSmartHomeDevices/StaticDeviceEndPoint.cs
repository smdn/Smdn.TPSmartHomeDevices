// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

internal sealed class StaticDeviceEndPoint : IDeviceEndPoint {
  private readonly ValueTask<EndPoint?> staticEndPointValueTaskResult;

  public StaticDeviceEndPoint(EndPoint endPoint)
  {
    staticEndPointValueTaskResult = new ValueTask<EndPoint?>(endPoint);
  }

  public ValueTask<EndPoint?> GetEndPointAsync(CancellationToken cancellationToken)
    => cancellationToken.IsCancellationRequested
      ?
#if SYSTEM_THREADING_TASKS_VALUETASK_FROMCANCELED
        ValueTask.FromCanceled<EndPoint?>(cancellationToken)
#else
        new(Task.FromCanceled<EndPoint?>(cancellationToken))
#endif
      : staticEndPointValueTaskResult;
}
