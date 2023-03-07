// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

internal sealed class UnresolvedDeviceEndPointProvider : IDeviceEndPointProvider {
  private readonly ValueTask<EndPoint?> unresolvedEndPointValueTaskResult = new((EndPoint?)null);

  public bool IsStaticEndPoint => true;

  public ValueTask<EndPoint?> GetEndPointAsync(CancellationToken cancellationToken)
    => unresolvedEndPointValueTaskResult;
}
