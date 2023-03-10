// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

internal sealed class ThrowExceptionDeviceEndPointProvider : IDeviceEndPointProvider {
  public bool IsStaticEndPoint => true;

  public ValueTask<EndPoint?> GetEndPointAsync(CancellationToken cancellationToken)
    => throw new NotSupportedException();
}
