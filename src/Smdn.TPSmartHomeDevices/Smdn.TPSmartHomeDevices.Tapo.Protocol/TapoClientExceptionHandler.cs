// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

public abstract class TapoClientExceptionHandler {
  internal static readonly TapoClientExceptionHandler Default = new TapoClientDefaultExceptionHandler();

  public abstract TapoClientExceptionHandling DetermineHandling(Exception exception, int attempt, ILogger? logger);
}