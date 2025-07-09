// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices;

internal sealed class StringifyableNullDeviceEndPoint : IDeviceEndPoint {
  public string StringRepresentation { get; init; }

  public StringifyableNullDeviceEndPoint(string stringRepresentation)
  {
    StringRepresentation = stringRepresentation;
  }

  public ValueTask<EndPoint?> ResolveAsync(CancellationToken cancellationToken)
    => new((EndPoint?)null);

  public override string? ToString() => StringRepresentation;
}
