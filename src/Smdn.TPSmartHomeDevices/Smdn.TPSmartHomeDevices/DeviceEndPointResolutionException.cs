// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.TPSmartHomeDevices;

public class DeviceEndPointResolutionException : Exception {
  public IDeviceEndPointProvider EndPointProvider { get; }

  public DeviceEndPointResolutionException(
    IDeviceEndPointProvider deviceEndPointProvider
  )
    : this(
      deviceEndPointProvider: deviceEndPointProvider,
      message: "Could not get or resolve the device endpoint.",
      innerException: null
    )
  {
  }

  public DeviceEndPointResolutionException(
    IDeviceEndPointProvider deviceEndPointProvider,
    string message,
    Exception? innerException
  )
    : base(
      message: message,
      innerException: innerException
    )
  {
    EndPointProvider = deviceEndPointProvider;
  }
}
