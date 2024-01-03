// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

internal sealed class NullLoggerScope : IDisposable {
  public static readonly NullLoggerScope Instance = new();
  public void Dispose() { }
}
