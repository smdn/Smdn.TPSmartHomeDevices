// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.TPSmartHomeDevices.Kasa.Protocol;

public class KasaMessageException : SystemException {
  public KasaMessageException(string message)
    : base(message: message)
  {
  }
}
