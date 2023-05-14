// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1032

namespace Smdn.TPSmartHomeDevices.Kasa.Protocol;

public class KasaMessageHeaderTooShortException : KasaMessageException {
  public KasaMessageHeaderTooShortException(string message)
    : base(message: message)
  {
  }
}
