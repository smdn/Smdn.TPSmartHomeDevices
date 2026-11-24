// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices;

internal static class ObsoleteMessages {
  internal const string RequiresReflectionBasedJsonSerializeInMethodOverload
    = "This method overload is deprecated, and its removal in a future major version release is under consideration. Migrate to the version that takes a `JsonTypeInfo<T>` as an argument.";
}
