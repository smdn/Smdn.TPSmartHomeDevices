// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

internal sealed class RequestCancellationAfterReturnDeviceEndPoint : IDeviceEndPoint {
  private readonly CancellationTokenSource cancellationTokenSource;
  private readonly ValueTask<EndPoint?> staticEndPointValueTaskResult;

  public RequestCancellationAfterReturnDeviceEndPoint(
    CancellationTokenSource cancellationTokenSource,
    EndPoint endPoint
  )
  {
    this.cancellationTokenSource = cancellationTokenSource;
    staticEndPointValueTaskResult = new ValueTask<EndPoint?>(endPoint);
  }

  public ValueTask<EndPoint?> GetEndPointAsync(CancellationToken cancellationToken)
  {
    try {
      return staticEndPointValueTaskResult;
    }
    finally {
      cancellationTokenSource.Cancel();
    }
  }
}
