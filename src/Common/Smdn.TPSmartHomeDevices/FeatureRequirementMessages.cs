// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.TPSmartHomeDevices;

internal static class FeatureRequirementMessages {
  internal const string RequiresDynamicCodeJsonSerializeInMethodOverload
    = "This method overload is not AOT-compatible. Consider using the version that takes a `JsonTypeInfo<T>` as an argument, which does not require dynamic code.";
  internal const string RequiresUnreferencedCodeJsonSerializeInMethodOverload
    = "This method overload requires the use of reflection. Consider using the version that takes a `JsonTypeInfo<T>` as an argument, which does not require reflection.";

  internal const string RequiresDynamicCodeConditionalJsonSerializeInMethodOverload
    = "This method overload performs on an AOT-incompatible code path unless JsonTypeInfo<T> is specified. Consider specifying `JsonTypeInfo<T>`.";
  internal const string RequiresUnreferencedCodeConditionalJsonSerializeInMethodOverload
    = "This method overload performs on a code path that uses reflection unless JsonTypeInfo<T> is specified. Consider specifying `JsonTypeInfo<T>`.";

  internal const string RequiresDynamicCodeJsonSerializeInMethod
    = "This method is not AOT-compatible. Consider using the variant that takes a `JsonTypeInfo<T>` as an argument, which does not require dynamic code.";
  internal const string RequiresUnreferencedCodeJsonSerializeInMethod
    = "This method requires the use of reflection. Consider using the variant that takes a `JsonTypeInfo<T>` as an argument, which does not require reflection.";

  internal const string RequiresDynamicCodeJsonConverterFactoryInType
    = "This type is not AOT-compatible. Consider using an implementation that relies on `JsonTypeInfo<T>` which does not require dynamic code.";
  internal const string RequiresUnreferencedCodeJsonConverterFactoryInType
    = "This type requires the use of reflection. Consider using an implementation that relies on `JsonTypeInfo<T>` which does not require reflection.";

  internal const string RequiresDynamicCodeJsonConverterFactoryInMethod
    = "This method is not AOT-compatible. Consider using an implementation that relies on `JsonTypeInfo<T>` which does not require dynamic code.";
  internal const string RequiresUnreferencedCodeJsonConverterFactoryInMethod
    = "This method requires the use of reflection. Consider using an implementation that relies on `JsonTypeInfo<T>` which does not require reflection.";
}
