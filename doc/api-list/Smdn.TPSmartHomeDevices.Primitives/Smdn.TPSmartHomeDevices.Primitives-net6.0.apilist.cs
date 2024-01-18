// Smdn.TPSmartHomeDevices.Primitives.dll (Smdn.TPSmartHomeDevices.Primitives-1.1.0-preview3)
//   Name: Smdn.TPSmartHomeDevices.Primitives
//   AssemblyVersion: 1.1.0.0
//   InformationalVersion: 1.1.0-preview3+997d1abe73c225c5c322bcb1043abfc410f19baf
//   TargetFramework: .NETCoreApp,Version=v6.0
//   Configuration: Release
//   Referenced assemblies:
//     Microsoft.Extensions.DependencyInjection.Abstractions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Smdn.Fundamental.PrintableEncoding.Hexadecimal, Version=3.0.1.0, Culture=neutral
//     System.Memory, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     System.Net.NetworkInformation, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.Net.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.Runtime, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.Text.Json, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
#nullable enable annotations

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Smdn.TPSmartHomeDevices;
using Smdn.TPSmartHomeDevices.Json;

namespace Smdn.TPSmartHomeDevices {
  public interface IDeviceEndPoint {
    ValueTask<EndPoint?> ResolveAsync(CancellationToken cancellationToken = default);
  }

  public interface IDeviceEndPointFactory<TAddress> where TAddress : notnull {
    IDeviceEndPoint Create(TAddress address);
  }

  public interface IDeviceInfo {
    string? FirmwareVersion { get; }
    string? HardwareVersion { get; }
    ReadOnlySpan<byte> Id { get; }
    PhysicalAddress? MacAddress { get; }
    string? ModelName { get; }
  }

  public interface IDynamicDeviceEndPoint : IDeviceEndPoint {
    void Invalidate();
  }

  public interface IMulticolorSmartLight : ISmartLight {
    ValueTask SetColorAsync(int hue, int saturation, int? brightness, TimeSpan transitionPeriod, CancellationToken cancellationToken);
    ValueTask SetColorTemperatureAsync(int colorTemperature, int? brightness, TimeSpan transitionPeriod, CancellationToken cancellationToken);
    virtual ValueTask SetOnOffStateAsync(bool newOnOffState, TimeSpan transitionPeriod, CancellationToken cancellationToken) {}
    virtual ValueTask TurnOffAsync(TimeSpan transitionPeriod, CancellationToken cancellationToken) {}
    virtual ValueTask TurnOnAsync(TimeSpan transitionPeriod, CancellationToken cancellationToken) {}
  }

  public interface ISmartDevice {
    IDeviceEndPoint EndPoint { get; }

    ValueTask<IDeviceInfo> GetDeviceInfoAsync(CancellationToken cancellationToken = default);
    ValueTask<bool> GetOnOffStateAsync(CancellationToken cancellationToken);
    ValueTask SetOnOffStateAsync(bool newOnOffState, CancellationToken cancellationToken);
    virtual ValueTask TurnOffAsync(CancellationToken cancellationToken = default) {}
    virtual ValueTask TurnOnAsync(CancellationToken cancellationToken = default) {}
  }

  public interface ISmartLight : ISmartDevice {
    ValueTask SetBrightnessAsync(int brightness, TimeSpan transitionPeriod, CancellationToken cancellationToken);
  }

  public static class DeviceEndPoint {
    public static IDeviceEndPoint Create(EndPoint endPoint) {}
    public static IDeviceEndPoint Create(IPAddress ipAddress) {}
    public static IDeviceEndPoint Create(string host) {}
    public static IDeviceEndPoint Create<TAddress>(TAddress address, IDeviceEndPointFactory<TAddress> endPointFactory) where TAddress : notnull {}
  }

  public static class DeviceEndPointFactoryServiceCollectionExtensions {
    public static IServiceCollection AddDeviceEndPointFactory<TAddress>(this IServiceCollection services, IDeviceEndPointFactory<TAddress> endPointFactory) where TAddress : notnull {}
  }

  public class DeviceEndPointResolutionException : Exception {
    public DeviceEndPointResolutionException(IDeviceEndPoint deviceEndPoint) {}
    public DeviceEndPointResolutionException(IDeviceEndPoint deviceEndPoint, string message, Exception? innerException) {}

    public IDeviceEndPoint DeviceEndPoint { get; }
  }

  public static class IDeviceEndPointExtensions {
    public static ValueTask<EndPoint> ResolveOrThrowAsync(this IDeviceEndPoint deviceEndPoint, int defaultPort, CancellationToken cancellationToken = default) {}
  }

  public sealed class StaticDeviceEndPoint : IDeviceEndPoint {
    public StaticDeviceEndPoint(EndPoint endPoint) {}

    public ValueTask<EndPoint?> ResolveAsync(CancellationToken cancellationToken = default) {}
    public override string? ToString() {}
  }
}

namespace Smdn.TPSmartHomeDevices.Json {
  public sealed class Base16ByteArrayJsonConverter : JsonConverter<byte[]> {
    public Base16ByteArrayJsonConverter() {}

    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {}
    public override void Write(Utf8JsonWriter writer, byte[]? @value, JsonSerializerOptions options) {}
  }

  public sealed class GeolocationInDecimalDegreesJsonConverter : JsonConverter<decimal?> {
    public GeolocationInDecimalDegreesJsonConverter() {}

    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {}
    public override void Write(Utf8JsonWriter writer, decimal? @value, JsonSerializerOptions options) {}
  }

  public sealed class MacAddressJsonConverter : JsonConverter<PhysicalAddress> {
    public MacAddressJsonConverter() {}

    public override PhysicalAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {}
    public override void Write(Utf8JsonWriter writer, PhysicalAddress @value, JsonSerializerOptions options) {}
  }

  public sealed class TimeSpanInMinutesJsonConverter : TimeSpanJsonConverter {
    public TimeSpanInMinutesJsonConverter() {}

    protected override TimeSpan ToTimeSpan(int @value) {}
  }

  public sealed class TimeSpanInSecondsJsonConverter : TimeSpanJsonConverter {
    public TimeSpanInSecondsJsonConverter() {}

    protected override TimeSpan ToTimeSpan(int @value) {}
  }

  public abstract class TimeSpanJsonConverter : JsonConverter<TimeSpan?> {
    protected TimeSpanJsonConverter() {}

    public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {}
    protected abstract TimeSpan ToTimeSpan(int @value);
    public override void Write(Utf8JsonWriter writer, TimeSpan? @value, JsonSerializerOptions options) {}
  }
}
// API list generated by Smdn.Reflection.ReverseGenerating.ListApi.MSBuild.Tasks v1.3.2.0.
// Smdn.Reflection.ReverseGenerating.ListApi.Core v1.2.0.0 (https://github.com/smdn/Smdn.Reflection.ReverseGenerating)
