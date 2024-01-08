// Smdn.TPSmartHomeDevices.Primitives.dll (Smdn.TPSmartHomeDevices.Primitives-1.1.0-preview1)
//   Name: Smdn.TPSmartHomeDevices.Primitives
//   AssemblyVersion: 1.1.0.0
//   InformationalVersion: 1.1.0-preview1+d9122eb664899e2e3470e87efca152f3456eb904
//   TargetFramework: .NETStandard,Version=v2.0
//   Configuration: Release
//   Referenced assemblies:
//     Microsoft.Extensions.DependencyInjection.Abstractions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     System.Text.Json, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     System.Threading.Tasks.Extensions, Version=4.2.0.1, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
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

  public interface IDynamicDeviceEndPoint : IDeviceEndPoint {
    void Invalidate();
  }

  public interface IMulticolorSmartLight : ISmartDevice {
    ValueTask SetBrightnessAsync(int brightness, TimeSpan transitionPeriod, CancellationToken cancellationToken);
    ValueTask SetColorAsync(int hue, int saturation, int? brightness, TimeSpan transitionPeriod, CancellationToken cancellationToken);
    ValueTask SetColorTemperatureAsync(int colorTemperature, int? brightness, TimeSpan transitionPeriod, CancellationToken cancellationToken);
  }

  public interface ISmartDevice {
    ValueTask<bool> GetOnOffStateAsync(CancellationToken cancellationToken);
    ValueTask SetOnOffStateAsync(bool newOnOffState, CancellationToken cancellationToken);
  }

  public interface ISmartPlug : ISmartDevice {
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
