// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

public interface IDeviceEndPointProvider {
  ValueTask<EndPoint?> GetEndPointAsync(CancellationToken cancellationToken = default);
}
