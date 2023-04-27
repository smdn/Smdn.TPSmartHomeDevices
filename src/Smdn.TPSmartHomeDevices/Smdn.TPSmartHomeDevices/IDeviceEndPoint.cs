// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

/// <summary>
/// Provides a mechanism for abstracting device endpoints and resolving to specific <seealso cref="EndPoint" />.
/// </summary>
/// <seealso cref="DeviceEndPoint" />
/// <seealso cref="StaticDeviceEndPoint" />
/// <seealso cref="IDynamicDeviceEndPoint" />
public interface IDeviceEndPoint {
  /// <summary>
  /// Resolves this endpoint to its corresponding <see cref="EndPoint"/>.
  /// </summary>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
  /// <returns>
  /// A <see cref="ValueTask{EndPoint}"/> representing the result of endpoint resolution.
  /// If the endpoint is successfully resolved, <see cref="EndPoint"/> representing the resolved endpoint is set. If not, <see langword="null" /> is set.
  /// </returns>
  /// <seealso cref="IDynamicDeviceEndPoint.Invalidate" />
  ValueTask<EndPoint?> ResolveAsync(CancellationToken cancellationToken = default);
}
