// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.TPSmartHomeDevices;

/// <summary>
/// The exception that is thrown when the device endpoint resolution by <see cref="IDeviceEndPoint.ResolveAsync"/> fails.
/// </summary>
/// <seealso cref="IDeviceEndPointExtensions.ResolveOrThrowAsync(IDeviceEndPoint, int, System.Threading.CancellationToken)"/>
public class DeviceEndPointResolutionException : Exception {
  /// <summary>
  /// Gets the <see cref="IDeviceEndPoint"/> that caused the exception.
  /// </summary>
  public IDeviceEndPoint DeviceEndPoint { get; }

  public DeviceEndPointResolutionException(
    IDeviceEndPoint deviceEndPoint
  )
    : this(
      deviceEndPoint: deviceEndPoint,
      message: "Could not get or resolve the device endpoint.",
      innerException: null
    )
  {
  }

  public DeviceEndPointResolutionException(
    IDeviceEndPoint deviceEndPoint,
    string message,
    Exception? innerException
  )
    : base(
      message: message,
      innerException: innerException
    )
  {
    DeviceEndPoint = deviceEndPoint;
  }
}
