// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices.Tapo;

internal static class DelayUtils {
  public static void Delay(TimeSpan timeout, CancellationToken cancellationToken)
  {
    try {
      Task.Delay(timeout, cancellationToken).GetAwaiter().GetResult();
    }
    catch (OperationCanceledException) {
      // swallow
    }
  }
}

