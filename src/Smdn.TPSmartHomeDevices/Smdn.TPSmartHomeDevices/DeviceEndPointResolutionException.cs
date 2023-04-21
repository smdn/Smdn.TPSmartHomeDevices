// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.TPSmartHomeDevices;

public class DeviceEndPointResolutionException : Exception {
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
