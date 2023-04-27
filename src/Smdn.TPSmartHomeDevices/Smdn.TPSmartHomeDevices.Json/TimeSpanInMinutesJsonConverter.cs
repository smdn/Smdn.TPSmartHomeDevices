// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.TPSmartHomeDevices.Json;

public sealed class TimeSpanInMinutesJsonConverter : TimeSpanJsonConverter {
  protected override TimeSpan ToTimeSpan(int value) => TimeSpan.FromMinutes(value);
}
