// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices;

public interface IDeviceEndPointFactory<TAddress> {
  IDeviceEndPoint Create(TAddress address, int port = 0);
}
