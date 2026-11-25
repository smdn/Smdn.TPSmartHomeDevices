// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices;

internal static class UnconditionalSuppressMessages {
  internal const string IL3050JustificationAotCompatibleCodePathUsingJsonTypeInfo
    = "As long as a `JsonTypeInfo<T>` is passed, this method operates via the AOT-compatible code path.";
  internal const string IL2026JustificationReflectionFreeCodePathUsingJsonTypeInfo
    = "As long as a `JsonTypeInfo<T>` is passed, this method operates via the code path that does not use reflection.";
}
