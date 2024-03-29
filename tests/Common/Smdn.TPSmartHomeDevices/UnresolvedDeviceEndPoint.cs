// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

internal sealed class UnresolvedDeviceEndPoint : IDeviceEndPoint {
  private readonly ValueTask<EndPoint?> unresolvedEndPointValueTaskResult = new((EndPoint?)null);

  public ValueTask<EndPoint?> ResolveAsync(CancellationToken cancellationToken)
    => unresolvedEndPointValueTaskResult;
}
