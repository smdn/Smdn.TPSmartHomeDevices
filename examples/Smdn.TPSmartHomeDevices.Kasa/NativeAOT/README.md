# Native AOT
This sample demonstrates Native AOT compilation. Executing `dotnet publish` enables the deployment of a precompiled application via [Native AOT compilation](https://learn.microsoft.com/dotnet/core/deploying/native-aot/).

## Notes on AOT Compatibility

`Smdn.TPSmartHomeDevices.Kasa` itself and the majority of its dependencies are AOT compatible. However, certain dependencies (including transitive ones) do not yet explicitly declare AOT compatibility (meaning they lack `IsAotCompatible=true` in their assembly metadata), despite their execution paths being implicitly AOT compatible. Consequently, AOT compilation may trigger warnings such as [IL3058](https://learn.microsoft.com/dotnet/core/deploying/native-aot/warnings/il3058), and potential runtime issues (such as unexpected missing metadata or execution failures) cannot be completely ruled out.
