// Smdn.TPSmartHomeDevices.Kasa.dll (Smdn.TPSmartHomeDevices.Kasa-2.0.0-preview2)
//   Name: Smdn.TPSmartHomeDevices.Kasa
//   AssemblyVersion: 2.0.0.0
//   InformationalVersion: 2.0.0-preview2+d001e07a3ea43a47290d9e9d2f4d582e14caa297
//   TargetFramework: .NETStandard,Version=v2.1
//   Configuration: Release
//   Referenced assemblies:
//     Microsoft.Extensions.DependencyInjection.Abstractions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Microsoft.Extensions.Logging.Abstractions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Smdn.TPSmartHomeDevices.Primitives, Version=1.1.0.0, Culture=neutral
//     System.Text.Encodings.Web, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     System.Text.Json, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
#nullable enable annotations

using System;
using System.Buffers;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Smdn.TPSmartHomeDevices;
using Smdn.TPSmartHomeDevices.Kasa;
using Smdn.TPSmartHomeDevices.Kasa.Protocol;

namespace Smdn.TPSmartHomeDevices.Kasa {
  public class HS105 :
    KasaDevice,
    ISmartDevice
  {
    public static HS105 Create<TAddress>(TAddress deviceAddress, IServiceProvider serviceProvider) where TAddress : notnull {}

    public HS105(IDeviceEndPoint deviceEndPoint, IServiceProvider? serviceProvider = null) {}
    public HS105(IPAddress ipAddress, IServiceProvider? serviceProvider = null) {}
    public HS105(PhysicalAddress macAddress, IServiceProvider serviceProvider) {}
    public HS105(string host, IServiceProvider? serviceProvider = null) {}

    public ValueTask<bool> GetOnOffStateAsync(CancellationToken cancellationToken = default) {}
    public ValueTask SetOnOffStateAsync(bool newOnOffState, CancellationToken cancellationToken = default) {}
    async ValueTask<IDeviceInfo> ISmartDevice.GetDeviceInfoAsync(CancellationToken cancellationToken) {}
    public ValueTask TurnOffAsync(CancellationToken cancellationToken = default) {}
    public ValueTask TurnOnAsync(CancellationToken cancellationToken = default) {}
  }

  public class KL130 :
    KasaDevice,
    IMulticolorSmartLight
  {
    public static KL130 Create<TAddress>(TAddress deviceAddress, IServiceProvider serviceProvider) where TAddress : notnull {}

    public KL130(IDeviceEndPoint deviceEndPoint, IServiceProvider? serviceProvider = null) {}
    public KL130(IPAddress ipAddress, IServiceProvider? serviceProvider = null) {}
    public KL130(PhysicalAddress macAddress, IServiceProvider serviceProvider) {}
    public KL130(string host, IServiceProvider? serviceProvider = null) {}

    public ValueTask<KL130LightState> GetLightStateAsync(CancellationToken cancellationToken = default) {}
    public ValueTask<bool> GetOnOffStateAsync(CancellationToken cancellationToken = default) {}
    public ValueTask SetBrightnessAsync(int brightness, TimeSpan transitionPeriod = default, CancellationToken cancellationToken = default) {}
    public ValueTask SetColorAsync(int hue, int saturation, int? brightness = null, TimeSpan transitionPeriod = default, CancellationToken cancellationToken = default) {}
    public ValueTask SetColorTemperatureAsync(int colorTemperature, int? brightness = null, TimeSpan transitionPeriod = default, CancellationToken cancellationToken = default) {}
    public ValueTask SetOnOffStateAsync(bool newOnOffState, TimeSpan transitionPeriod = default, CancellationToken cancellationToken = default) {}
    async ValueTask<IDeviceInfo> ISmartDevice.GetDeviceInfoAsync(CancellationToken cancellationToken) {}
    ValueTask ISmartDevice.SetOnOffStateAsync(bool newOnOffState, CancellationToken cancellationToken) {}
    public ValueTask TurnOffAsync(TimeSpan transitionPeriod = default, CancellationToken cancellationToken = default) {}
    public ValueTask TurnOnAsync(TimeSpan transitionPeriod = default, CancellationToken cancellationToken = default) {}
  }

  public class KasaDevice : IDisposable {
    protected readonly struct NullParameter {
    }

    protected static readonly JsonEncodedText MethodTextGetSysInfo;
    protected static readonly JsonEncodedText ModuleTextSystem;

    public static KasaDevice Create(IDeviceEndPoint deviceEndPoint, IServiceProvider? serviceProvider = null) {}
    public static KasaDevice Create(IPAddress ipAddress, IServiceProvider? serviceProvider = null) {}
    public static KasaDevice Create(PhysicalAddress macAddress, IServiceProvider serviceProvider) {}
    public static KasaDevice Create(string host, IServiceProvider? serviceProvider = null) {}
    public static KasaDevice Create<TAddress>(TAddress deviceAddress, IServiceProvider serviceProvider) where TAddress : notnull {}

    protected KasaDevice(IDeviceEndPoint deviceEndPoint, IServiceProvider? serviceProvider) {}
    protected KasaDevice(IPAddress ipAddress, IServiceProvider? serviceProvider) {}
    protected KasaDevice(PhysicalAddress macAddress, IServiceProvider serviceProvider) {}
    protected KasaDevice(string host, IServiceProvider? serviceProvider) {}

    public bool IsConnected { get; }
    protected bool IsDisposed { get; }

    protected virtual void Dispose(bool disposing) {}
    public void Dispose() {}
    public ValueTask<KasaDeviceInfo?> GetDeviceInfoAsync(CancellationToken cancellationToken = default) {}
    protected ValueTask<TSysInfo> GetSysInfoAsync<TSysInfo>(Func<JsonElement, TSysInfo> composeResult, CancellationToken cancellationToken = default) {}
    public ValueTask<EndPoint> ResolveEndPointAsync(CancellationToken cancellationToken = default) {}
    protected ValueTask SendRequestAsync<TMethodParameter>(JsonEncodedText module, JsonEncodedText method, TMethodParameter parameters, CancellationToken cancellationToken) {}
    protected ValueTask<TMethodResult> SendRequestAsync<TMethodParameter, TMethodResult>(JsonEncodedText module, JsonEncodedText method, TMethodParameter parameters, Func<JsonElement, TMethodResult> composeResult, CancellationToken cancellationToken) {}
    protected ValueTask<TMethodResult> SendRequestAsync<TMethodResult>(JsonEncodedText module, JsonEncodedText method, Func<JsonElement, TMethodResult> composeResult, CancellationToken cancellationToken) {}
    public override string? ToString() {}
  }

  public abstract class KasaDeviceExceptionHandler {
    internal protected static readonly KasaDeviceExceptionHandler Default; // = "Smdn.TPSmartHomeDevices.Kasa.KasaDeviceDefaultExceptionHandler"

    protected KasaDeviceExceptionHandler() {}

    public abstract KasaDeviceExceptionHandling DetermineHandling(KasaDevice device, Exception exception, int attempt, ILogger? logger);
  }

  public static class KasaDeviceExceptionHandlerServiceCollectionExtensions {
    public static IServiceCollection AddKasaDeviceExceptionHandler(this IServiceCollection services, KasaDeviceExceptionHandler exceptionHandler) {}
  }

  public class KasaDeviceInfo : IDeviceInfo {
    public KasaDeviceInfo() {}

    public string? Description { get; }
    [JsonConverter(typeof(Base16ByteArrayJsonConverter))]
    [JsonPropertyName("fwId")]
    public byte[]? FirmwareId { get; init; }
    [JsonPropertyName("sw_ver")]
    public string? FirmwareVersion { get; init; }
    [JsonConverter(typeof(Base16ByteArrayJsonConverter))]
    [JsonPropertyName("hwId")]
    public byte[]? HardwareId { get; init; }
    [JsonPropertyName("hw_ver")]
    public string? HardwareVersion { get; init; }
    [JsonConverter(typeof(Base16ByteArrayJsonConverter))]
    [JsonPropertyName("deviceId")]
    public byte[]? Id { get; init; }
    [Obsolete("This property is intended to be used only for JSON deserialization. Refer 'Description' to get the value.")]
    [JsonPropertyName("description")]
    public string? JsonPropertyDescription { get; set; }
    [Obsolete("This property is intended to be used only for JSON deserialization. Refer 'Description' to get the value.")]
    [JsonPropertyName("dev_name")]
    public string? JsonPropertyDevName { get; set; }
    [Obsolete("This property is intended to be used only for JSON deserialization. Refer 'MacAddress' to get the value.")]
    [JsonConverter(typeof(MacAddressJsonConverter))]
    [JsonPropertyName("mac")]
    public PhysicalAddress? JsonPropertyMac { get; init; }
    [Obsolete("This property is intended to be used only for JSON deserialization. Refer 'MacAddress' to get the value.")]
    [JsonConverter(typeof(MacAddressJsonConverter))]
    [JsonPropertyName("mic_mac")]
    public PhysicalAddress? JsonPropertyMicMac { get; init; }
    [Obsolete("This property is intended to be used only for JSON deserialization. Refer 'TypeName' to get the value.")]
    [JsonPropertyName("mic_type")]
    public string? JsonPropertyMicType { get; set; }
    [Obsolete("This property is intended to be used only for JSON deserialization. Refer 'TypeName' to get the value.")]
    [JsonPropertyName("type")]
    public string? JsonPropertyType { get; set; }
    public PhysicalAddress? MacAddress { get; }
    [JsonPropertyName("model")]
    public string? ModelName { get; init; }
    [JsonPropertyName("rssi")]
    public decimal? NetworkRssi { get; init; }
    [JsonConverter(typeof(Base16ByteArrayJsonConverter))]
    [JsonPropertyName("oemId")]
    public byte[]? OemId { get; init; }
    ReadOnlySpan<byte> IDeviceInfo.Id { get; }
    [JsonIgnore]
    public DateTimeOffset TimeStamp { get; }
    public string? TypeName { get; }
  }

  public class KasaDisconnectedException : KasaProtocolException {
    public KasaDisconnectedException(string message, EndPoint deviceEndPoint, Exception? innerException) {}
  }

  public class KasaErrorResponseException : KasaUnexpectedResponseException {
    public KasaErrorResponseException(EndPoint deviceEndPoint, string requestModule, string requestMethod, int rawErrorCode) {}

    public int RawErrorCode { get; }
  }

  public class KasaIncompleteResponseException : KasaUnexpectedResponseException {
    public KasaIncompleteResponseException(string message, EndPoint deviceEndPoint, string requestModule, string requestMethod, Exception? innerException) {}
  }

  public abstract class KasaProtocolException : InvalidOperationException {
    protected KasaProtocolException(string message, EndPoint deviceEndPoint, Exception? innerException) {}

    public EndPoint DeviceEndPoint { get; }
  }

  public class KasaUnexpectedResponseException : KasaProtocolException {
    public KasaUnexpectedResponseException(string message, EndPoint deviceEndPoint, string requestModule, string requestMethod, Exception? innerException) {}

    public string RequestMethod { get; }
    public string RequestModule { get; }
  }

  public readonly struct KL130LightState {
    [JsonPropertyName("brightness")]
    public int? Brightness { get; init; }
    [JsonPropertyName("color_temp")]
    public int? ColorTemperature { get; init; }
    [JsonPropertyName("hue")]
    public int? Hue { get; init; }
    [JsonConverter(typeof(KasaNumericalBooleanJsonConverter))]
    [JsonPropertyName("on_off")]
    public bool IsOn { get; init; }
    [JsonPropertyName("mode")]
    public string? Mode { get; init; }
    [JsonPropertyName("saturation")]
    public int? Saturation { get; init; }
  }

  public readonly struct KasaDeviceExceptionHandling {
    public static readonly KasaDeviceExceptionHandling InvalidateEndPointAndRetry; // = "{ShouldRetry=True, RetryAfter=00:00:00, ShouldReconnect=False, ShouldInvalidateEndPoint=True}"
    public static readonly KasaDeviceExceptionHandling InvalidateEndPointAndThrow; // = "{ShouldRetry=False, RetryAfter=00:00:00, ShouldReconnect=False, ShouldInvalidateEndPoint=True}"
    public static readonly KasaDeviceExceptionHandling Retry; // = "{ShouldRetry=True, RetryAfter=00:00:00, ShouldReconnect=False, ShouldInvalidateEndPoint=False}"
    public static readonly KasaDeviceExceptionHandling RetryAfterReconnect; // = "{ShouldRetry=True, RetryAfter=00:00:00, ShouldReconnect=True, ShouldInvalidateEndPoint=False}"
    public static readonly KasaDeviceExceptionHandling Throw; // = "{ShouldRetry=False, RetryAfter=00:00:00, ShouldReconnect=False, ShouldInvalidateEndPoint=False}"

    public static KasaDeviceExceptionHandling CreateRetry(TimeSpan retryAfter, bool shouldReconnect = false) {}

    public TimeSpan RetryAfter { get; init; }
    public bool ShouldInvalidateEndPoint { get; init; }
    public bool ShouldReconnect { get; init; }
    public bool ShouldRetry { get; init; }

    public override string ToString() {}
  }
}

namespace Smdn.TPSmartHomeDevices.Kasa.Json {
  public sealed class KasaNumericalBooleanJsonConverter : JsonConverter<bool> {
    public KasaNumericalBooleanJsonConverter() {}

    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {}
    [...] public override <unknown> Write(...) {}
  }
}

namespace Smdn.TPSmartHomeDevices.Kasa.Protocol {
  public sealed class KasaClient : IDisposable {
    public const int DefaultPort = 9999;

    public KasaClient(EndPoint endPoint, ILogger? logger = null) {}

    public EndPoint EndPoint { get; }
    public bool IsConnected { get; }

    public void Dispose() {}
    public ValueTask<TMethodResult> SendAsync<TMethodParameter, TMethodResult>(JsonEncodedText module, JsonEncodedText method, TMethodParameter parameter, Func<JsonElement, TMethodResult> composeResult, CancellationToken cancellationToken = default) {}
  }

  public static class KasaJsonSerializer {
    public const byte InitialKey = 171;

    public static void DecryptInPlace(Span<byte> body) {}
    public static JsonElement Deserialize(ArrayBufferWriter<byte> buffer, JsonEncodedText module, JsonEncodedText method, ILogger? logger = null) {}
    public static void EncryptInPlace(Span<byte> body) {}
    public static void Serialize<TMethodParameter>(ArrayBufferWriter<byte> buffer, JsonEncodedText module, JsonEncodedText method, TMethodParameter parameter, ILogger? logger = null) {}
  }

  public class KasaMessageBodyTooShortException : KasaMessageException {
    public KasaMessageBodyTooShortException(int indicatedLength, int actualLength) {}

    public int ActualLength { get; }
    public int IndicatedLength { get; }
  }

  public class KasaMessageException : SystemException {
    public KasaMessageException(string message) {}
  }

  public class KasaMessageHeaderTooShortException : KasaMessageException {
    public KasaMessageHeaderTooShortException(string message) {}
  }
}
// API list generated by Smdn.Reflection.ReverseGenerating.ListApi.MSBuild.Tasks v1.3.2.0.
// Smdn.Reflection.ReverseGenerating.ListApi.Core v1.2.0.0 (https://github.com/smdn/Smdn.Reflection.ReverseGenerating)
