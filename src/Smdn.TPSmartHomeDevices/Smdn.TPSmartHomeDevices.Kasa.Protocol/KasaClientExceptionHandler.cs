// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Kasa.Protocol;

public abstract class KasaClientExceptionHandler {
  protected internal static readonly KasaClientExceptionHandler Default = new KasaClientDefaultExceptionHandler();

  public abstract KasaClientExceptionHandling DetermineHandling(
    KasaDevice device,
    Exception exception,
    int attempt,
    ILogger? logger
  );
}
