// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

internal sealed class ThrowExceptionDeviceEndPoint : IDeviceEndPoint {
  public ValueTask<EndPoint?> ResolveAsync(CancellationToken cancellationToken)
    => throw new NotSupportedException();
}
