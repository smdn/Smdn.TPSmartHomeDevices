// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

internal sealed class StringifiableNullDeviceEndPoint : IDeviceEndPoint {
  public string StringRepresentation { get; init;}

  public StringifiableNullDeviceEndPoint(string stringRepresentation)
  {
    StringRepresentation = stringRepresentation;
  }

  public ValueTask<EndPoint?> ResolveAsync(CancellationToken cancellationToken)
    => new((EndPoint?)null);

  public override string? ToString() => StringRepresentation;
}
