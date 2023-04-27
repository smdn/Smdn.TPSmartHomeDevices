// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices.Kasa.Protocol;

public class KasaMessageBodyTooShortException : KasaMessageException {
  public int IndicatedLength { get; }
  public int ActualLength { get; }

  public KasaMessageBodyTooShortException(
    int indicatedLength,
    int actualLength
  )
    : base(
      message: $"length of body is {actualLength} bytes but falls short of {indicatedLength} bytes which is indicated in the header."
    )
  {
    IndicatedLength = indicatedLength;
    ActualLength = actualLength;
  }
}
