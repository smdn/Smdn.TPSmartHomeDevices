// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

public interface IDeviceEndPoint {
  ValueTask<EndPoint?> ResolveAsync(CancellationToken cancellationToken = default);
}