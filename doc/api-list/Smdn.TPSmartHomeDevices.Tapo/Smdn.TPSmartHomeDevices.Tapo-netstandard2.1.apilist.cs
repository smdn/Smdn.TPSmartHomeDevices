// Smdn.TPSmartHomeDevices.Tapo.dll (Smdn.TPSmartHomeDevices.Tapo-2.0.0)
//   Name: Smdn.TPSmartHomeDevices.Tapo
//   AssemblyVersion: 2.0.0.0
//   InformationalVersion: 2.0.0+b94237e3ade205b0c2874616f6f9c9259586dcbd
//   TargetFramework: .NETStandard,Version=v2.1
//   Configuration: Release
//   Referenced assemblies:
//     Microsoft.Extensions.DependencyInjection.Abstractions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Microsoft.Extensions.Http, Version=6.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Microsoft.Extensions.Logging.Abstractions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Smdn.Fundamental.PrintableEncoding.Hexadecimal, Version=3.0.1.0, Culture=neutral
//     Smdn.TPSmartHomeDevices.Primitives, Version=1.1.0.0, Culture=neutral
//     System.Net.Http.Json, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     System.Text.Encodings.Web, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     System.Text.Json, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
#nullable enable annotations

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Smdn.TPSmartHomeDevices;
using Smdn.TPSmartHomeDevices.Tapo;
using Smdn.TPSmartHomeDevices.Tapo.Credentials;
using Smdn.TPSmartHomeDevices.Tapo.Json;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo {
  public class L530 :
    TapoDevice,
    IMulticolorSmartLight
  {
    public static L530 Create<TAddress>(TAddress deviceAddress, IServiceProvider serviceProvider, ITapoCredentialProvider? credential = null) where TAddress : notnull {}

    public L530(IDeviceEndPoint deviceEndPoint, ITapoCredentialProvider? credential = null, IServiceProvider? serviceProvider = null) {}
    public L530(IPAddress ipAddress, IServiceProvider serviceProvider) {}
    public L530(IPAddress ipAddress, string email, string password, IServiceProvider? serviceProvider = null) {}
    public L530(PhysicalAddress macAddress, IServiceProvider serviceProvider) {}
    public L530(PhysicalAddress macAddress, string email, string password, IServiceProvider serviceProvider) {}
    public L530(string host, IServiceProvider serviceProvider) {}
    public L530(string host, string email, string password, IServiceProvider? serviceProvider = null) {}

    public ValueTask SetBrightnessAsync(int brightness, CancellationToken cancellationToken = default) {}
    public ValueTask SetColorAsync(int hue, int saturation, int? brightness = null, CancellationToken cancellationToken = default) {}
    public ValueTask SetColorHueAsync(int hue, int? brightness = null, CancellationToken cancellationToken = default) {}
    public ValueTask SetColorSaturationAsync(int saturation, int? brightness = null, CancellationToken cancellationToken = default) {}
    public ValueTask SetColorTemperatureAsync(int colorTemperature, int? brightness = null, CancellationToken cancellationToken = default) {}
    ValueTask IMulticolorSmartLight.SetColorAsync(int hue, int saturation, int? brightness, TimeSpan transitionPeriod, CancellationToken cancellationToken) {}
    ValueTask IMulticolorSmartLight.SetColorTemperatureAsync(int colorTemperature, int? brightness, TimeSpan transitionPeriod, CancellationToken cancellationToken) {}
    ValueTask ISmartLight.SetBrightnessAsync(int brightness, TimeSpan transitionPeriod, CancellationToken cancellationToken) {}
  }

  public class L900 :
    TapoDevice,
    IMulticolorSmartLight
  {
    public static L900 Create<TAddress>(TAddress deviceAddress, IServiceProvider serviceProvider, ITapoCredentialProvider? credential = null) where TAddress : notnull {}

    public L900(IDeviceEndPoint deviceEndPoint, ITapoCredentialProvider? credential = null, IServiceProvider? serviceProvider = null) {}
    public L900(IPAddress ipAddress, IServiceProvider serviceProvider) {}
    public L900(IPAddress ipAddress, string email, string password, IServiceProvider? serviceProvider = null) {}
    public L900(PhysicalAddress macAddress, IServiceProvider serviceProvider) {}
    public L900(PhysicalAddress macAddress, string email, string password, IServiceProvider serviceProvider) {}
    public L900(string host, IServiceProvider serviceProvider) {}
    public L900(string host, string email, string password, IServiceProvider? serviceProvider = null) {}

    public ValueTask SetBrightnessAsync(int brightness, CancellationToken cancellationToken = default) {}
    public ValueTask SetColorAsync(int hue, int saturation, int? brightness = null, CancellationToken cancellationToken = default) {}
    public ValueTask SetColorHueAsync(int hue, int? brightness, CancellationToken cancellationToken = default) {}
    public ValueTask SetColorSaturationAsync(int saturation, int? brightness = null, CancellationToken cancellationToken = default) {}
    public ValueTask SetColorTemperatureAsync(int colorTemperature, int? brightness = null, CancellationToken cancellationToken = default) {}
    ValueTask IMulticolorSmartLight.SetColorAsync(int hue, int saturation, int? brightness, TimeSpan transitionPeriod, CancellationToken cancellationToken) {}
    ValueTask IMulticolorSmartLight.SetColorTemperatureAsync(int colorTemperature, int? brightness, TimeSpan transitionPeriod, CancellationToken cancellationToken) {}
    ValueTask ISmartLight.SetBrightnessAsync(int brightness, TimeSpan transitionPeriod, CancellationToken cancellationToken) {}
  }

  public class P105 : TapoDevice {
    public static P105 Create<TAddress>(TAddress deviceAddress, IServiceProvider serviceProvider, ITapoCredentialProvider? credential = null) where TAddress : notnull {}

    public P105(IDeviceEndPoint deviceEndPoint, ITapoCredentialProvider? credential = null, IServiceProvider? serviceProvider = null) {}
    public P105(IPAddress ipAddress, IServiceProvider serviceProvider) {}
    public P105(IPAddress ipAddress, string email, string password, IServiceProvider? serviceProvider = null) {}
    public P105(PhysicalAddress macAddress, IServiceProvider serviceProvider) {}
    public P105(PhysicalAddress macAddress, string email, string password, IServiceProvider serviceProvider) {}
    public P105(string host, IServiceProvider serviceProvider) {}
    public P105(string host, string email, string password, IServiceProvider? serviceProvider = null) {}
  }

  public class P110M : TapoDevice {
    public static P110M Create<TAddress>(TAddress deviceAddress, IServiceProvider serviceProvider, ITapoCredentialProvider? credential = null) where TAddress : notnull {}

    public P110M(IDeviceEndPoint deviceEndPoint, ITapoCredentialProvider? credential = null, IServiceProvider? serviceProvider = null) {}
    public P110M(IPAddress ipAddress, IServiceProvider serviceProvider) {}
    public P110M(IPAddress ipAddress, string email, string password, IServiceProvider? serviceProvider = null) {}
    public P110M(PhysicalAddress macAddress, IServiceProvider serviceProvider) {}
    public P110M(PhysicalAddress macAddress, string email, string password, IServiceProvider serviceProvider) {}
    public P110M(string host, IServiceProvider serviceProvider) {}
    public P110M(string host, string email, string password, IServiceProvider? serviceProvider = null) {}

    public virtual ValueTask<decimal?> GetCurrentPowerConsumptionAsync(CancellationToken cancellationToken = default) {}
    public virtual ValueTask<TapoPlugMonitoringData> GetMonitoringDataAsync(CancellationToken cancellationToken = default) {}
  }

  public class TapoAuthenticationException : TapoProtocolException {
    public TapoAuthenticationException(string message, Uri endPoint, Exception? innerException = null) {}
  }

  public static class TapoCredentailProviderServiceCollectionExtensions {
    public static IServiceCollection AddTapoBase64EncodedCredential(this IServiceCollection services, string base64UserNameSHA1Digest, string base64Password) {}
    public static IServiceCollection AddTapoBase64EncodedKlapCredentialFromEnvironmentVariable(this IServiceCollection services, string envVarBase64KlapLocalAuthHash) {}
    public static IServiceCollection AddTapoCredential(this IServiceCollection services, string email, string password) {}
    public static IServiceCollection AddTapoCredentialFromEnvironmentVariable(this IServiceCollection services, string envVarUsername, string envVarPassword) {}
    public static IServiceCollection AddTapoCredentialProvider(this IServiceCollection services, ITapoCredentialProvider credentialProvider) {}
  }

  public class TapoDevice :
    IDisposable,
    ISmartDevice,
    ITapoCredentialIdentity
  {
    public static TapoDevice Create(IDeviceEndPoint deviceEndPoint, ITapoCredentialProvider? credential = null, IServiceProvider? serviceProvider = null) {}
    public static TapoDevice Create(IPAddress ipAddress, IServiceProvider serviceProvider) {}
    public static TapoDevice Create(IPAddress ipAddress, string email, string password, IServiceProvider? serviceProvider = null) {}
    public static TapoDevice Create(PhysicalAddress macAddress, IServiceProvider serviceProvider) {}
    public static TapoDevice Create(PhysicalAddress macAddress, string email, string password, IServiceProvider serviceProvider) {}
    public static TapoDevice Create(string host, IServiceProvider serviceProvider) {}
    public static TapoDevice Create(string host, string email, string password, IServiceProvider? serviceProvider = null) {}
    public static TapoDevice Create<TAddress>(TAddress deviceAddress, IServiceProvider serviceProvider, ITapoCredentialProvider? credential = null) where TAddress : notnull {}

    protected TapoDevice(IDeviceEndPoint deviceEndPoint, ITapoCredentialProvider? credential, IServiceProvider? serviceProvider) {}
    protected TapoDevice(IDeviceEndPoint deviceEndPoint, ITapoCredentialProvider? credential, TapoDeviceExceptionHandler? exceptionHandler, IServiceProvider? serviceProvider) {}
    protected TapoDevice(IPAddress ipAddress, IServiceProvider serviceProvider) {}
    protected TapoDevice(IPAddress ipAddress, string email, string password, IServiceProvider? serviceProvider) {}
    protected TapoDevice(PhysicalAddress macAddress, IServiceProvider serviceProvider) {}
    protected TapoDevice(PhysicalAddress macAddress, string email, string password, IServiceProvider serviceProvider) {}
    protected TapoDevice(string host, IServiceProvider serviceProvider) {}
    protected TapoDevice(string host, string email, string password, IServiceProvider? serviceProvider) {}

    public IDeviceEndPoint EndPoint { get; }
    protected bool IsDisposed { get; }
    public TapoSession? Session { get; }
    public string TerminalUuidString { get; }
    public TimeSpan? Timeout { get; set; }

    protected virtual void Dispose(bool disposing) {}
    public void Dispose() {}
    protected ValueTask EnsureSessionEstablishedAsync(CancellationToken cancellationToken = default) {}
    public virtual ValueTask<TapoDeviceEnergyUsage?> GetCumulativeEnergyUsageAsync(CancellationToken cancellationToken = default) {}
    public ValueTask<TDeviceInfo> GetDeviceInfoAsync<TDeviceInfo>(CancellationToken cancellationToken = default) {}
    public ValueTask<TResult> GetDeviceInfoAsync<TDeviceInfo, TResult>(Func<TDeviceInfo, TResult> composeResult, CancellationToken cancellationToken = default) {}
    public ValueTask<TapoDeviceInfo> GetDeviceInfoAsync(CancellationToken cancellationToken = default) {}
    public virtual ValueTask<(TapoDeviceOperatingTime? TotalOperatingTime, TapoDeviceEnergyUsage? CumulativeEnergyUsage)> GetDeviceUsageAsync(CancellationToken cancellationToken = default) {}
    public ValueTask<bool> GetOnOffStateAsync(CancellationToken cancellationToken = default) {}
    public virtual ValueTask<TapoDeviceOperatingTime?> GetTotalOperatingTimeAsync(CancellationToken cancellationToken = default) {}
    public ValueTask<EndPoint> ResolveEndPointAsync(CancellationToken cancellationToken = default) {}
    protected ValueTask SendRequestAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) where TRequest : notnull, ITapoPassThroughRequest where TResponse : ITapoPassThroughResponse {}
    protected ValueTask<TResult> SendRequestAsync<TRequest, TResponse, TResult>(TRequest request, Func<TResponse, TResult> composeResult, CancellationToken cancellationToken = default) where TRequest : notnull, ITapoPassThroughRequest where TResponse : ITapoPassThroughResponse {}
    public ValueTask SetDeviceInfoAsync<TDeviceInfo>(TDeviceInfo deviceInfo, CancellationToken cancellationToken = default) {}
    public ValueTask SetOnOffStateAsync(bool newOnOffState, CancellationToken cancellationToken = default) {}
    async ValueTask<IDeviceInfo> ISmartDevice.GetDeviceInfoAsync(CancellationToken cancellationToken) {}
    public override string? ToString() {}
    public ValueTask TurnOffAsync(CancellationToken cancellationToken = default) {}
    public ValueTask TurnOnAsync(CancellationToken cancellationToken = default) {}
  }

  public abstract class TapoDeviceExceptionHandler {
    internal protected static readonly TapoDeviceExceptionHandler Default; // = "Smdn.TPSmartHomeDevices.Tapo.TapoDeviceDefaultExceptionHandler"

    protected TapoDeviceExceptionHandler() {}

    public abstract TapoDeviceExceptionHandling DetermineHandling(TapoDevice device, Exception exception, int attempt, ILogger? logger);
  }

  public static class TapoDeviceExceptionHandlerServiceCollectionExtensions {
    public static IServiceCollection AddTapoDeviceExceptionHandler(this IServiceCollection services, TapoDeviceExceptionHandler exceptionHandler) {}
  }

  public class TapoDeviceInfo : IDeviceInfo {
    public TapoDeviceInfo() {}

    [JsonPropertyName("avatar")]
    public string? Avatar { get; init; }
    [JsonConverter(typeof(Base16ByteArrayJsonConverter))]
    [JsonPropertyName("fw_id")]
    public byte[]? FirmwareId { get; init; }
    [JsonPropertyName("fw_ver")]
    public string? FirmwareVersion { get; init; }
    [JsonConverter(typeof(GeolocationInDecimalDegreesJsonConverter))]
    [JsonPropertyName("latitude")]
    public decimal? GeolocationLatitude { get; init; }
    [JsonConverter(typeof(GeolocationInDecimalDegreesJsonConverter))]
    [JsonPropertyName("longitude")]
    public decimal? GeolocationLongitude { get; init; }
    [JsonConverter(typeof(Base16ByteArrayJsonConverter))]
    [JsonPropertyName("hw_id")]
    public byte[]? HardwareId { get; init; }
    [JsonPropertyName("specs")]
    public string? HardwareSpecifications { get; init; }
    [JsonPropertyName("hw_ver")]
    public string? HardwareVersion { get; init; }
    [JsonPropertyName("has_set_location_info")]
    public bool HasGeolocationInfoSet { get; init; }
    [JsonConverter(typeof(TapoIPAddressJsonConverter))]
    [JsonPropertyName("ip")]
    public IPAddress? IPAddress { get; init; }
    [JsonConverter(typeof(Base16ByteArrayJsonConverter))]
    [JsonPropertyName("device_id")]
    public byte[]? Id { get; init; }
    [JsonPropertyName("device_on")]
    public bool IsOn { get; init; }
    [JsonPropertyName("overheated")]
    public bool IsOverheated { get; init; }
    [JsonPropertyName("lang")]
    public string? Language { get; init; }
    [JsonConverter(typeof(MacAddressJsonConverter))]
    [JsonPropertyName("mac")]
    public PhysicalAddress? MacAddress { get; init; }
    [JsonPropertyName("model")]
    public string? ModelName { get; init; }
    [JsonPropertyName("rssi")]
    public decimal? NetworkRssi { get; init; }
    [JsonPropertyName("signal_level")]
    public int? NetworkSignalLevel { get; init; }
    [JsonConverter(typeof(TapoBase64StringJsonConverter))]
    [JsonPropertyName("ssid")]
    public string? NetworkSsid { get; init; }
    [JsonConverter(typeof(TapoBase64StringJsonConverter))]
    [JsonPropertyName("nickname")]
    public string? NickName { get; init; }
    [JsonConverter(typeof(Base16ByteArrayJsonConverter))]
    [JsonPropertyName("oem_id")]
    public byte[]? OemId { get; init; }
    [JsonConverter(typeof(TimeSpanInSecondsJsonConverter))]
    [JsonPropertyName("on_time")]
    public TimeSpan? OnTimeDuration { get; init; }
    ReadOnlySpan<byte> IDeviceInfo.Id { get; }
    [JsonIgnore]
    public DateTimeOffset TimeStamp { get; }
    [JsonConverter(typeof(TimeSpanInMinutesJsonConverter))]
    [JsonPropertyName("time_diff")]
    public TimeSpan? TimeZoneOffset { get; init; }
    [JsonPropertyName("region")]
    public string? TimeZoneRegion { get; init; }
    [JsonPropertyName("type")]
    public string? TypeName { get; init; }
  }

  public class TapoErrorResponseException : TapoProtocolException {
    public TapoErrorResponseException(Uri requestEndPoint, string requestMethod, int rawErrorCode) {}

    public int RawErrorCode { get; }
    public string RequestMethod { get; }
  }

  public static class TapoHttpClientFactoryServiceCollectionExtensions {
    public static IServiceCollection AddTapoHttpClient(this IServiceCollection services, Action<HttpClient>? configureClient = null) {}
  }

  public class TapoPlugMonitoringData {
    public TapoPlugMonitoringData() {}

    [JsonConverter(typeof(TapoElectricEnergyInWattHourJsonConverter))]
    [JsonPropertyName("month_energy")]
    public decimal? CumulativeEnergyUsageThisMonth { get; init; }
    [JsonConverter(typeof(TapoElectricEnergyInWattHourJsonConverter))]
    [JsonPropertyName("today_energy")]
    public decimal? CumulativeEnergyUsageToday { get; init; }
    [JsonConverter(typeof(TapoElectricPowerInMilliWattJsonConverter))]
    [JsonPropertyName("current_power")]
    public decimal? CurrentPowerConsumption { get; init; }
    [JsonConverter(typeof(TapoLocalDateAndTimeJsonConverter))]
    [JsonPropertyName("local_time")]
    public DateTime? TimeStamp { get; init; }
    [JsonConverter(typeof(TimeSpanInMinutesJsonConverter))]
    [JsonPropertyName("month_runtime")]
    public TimeSpan? TotalOperatingTimeThisMonth { get; init; }
    [JsonConverter(typeof(TimeSpanInMinutesJsonConverter))]
    [JsonPropertyName("today_runtime")]
    public TimeSpan? TotalOperatingTimeToday { get; init; }
  }

  public class TapoProtocolException : InvalidOperationException {
    internal protected TapoProtocolException(string message, Uri endPoint, Exception? innerException) {}

    public Uri EndPoint { get; }
  }

  public static class TapoSessionProtocolSelectorServiceCollectionExtensions {
    public static IServiceCollection AddTapoProtocolSelector(this IServiceCollection services, Func<TapoDevice, TapoSessionProtocol?> selectProtocol) {}
    public static IServiceCollection AddTapoProtocolSelector(this IServiceCollection services, TapoSessionProtocol protocol) {}
    public static IServiceCollection AddTapoProtocolSelector(this IServiceCollection services, TapoSessionProtocolSelector selector) {}
  }

  public readonly struct TapoDeviceEnergyUsage {
    [JsonConverter(typeof(TapoElectricEnergyInWattHourJsonConverter))]
    [JsonPropertyName("past30")]
    public decimal? Past30Days { get; init; }
    [JsonConverter(typeof(TapoElectricEnergyInWattHourJsonConverter))]
    [JsonPropertyName("past7")]
    public decimal? Past7Days { get; init; }
    [JsonConverter(typeof(TapoElectricEnergyInWattHourJsonConverter))]
    [JsonPropertyName("today")]
    public decimal? Today { get; init; }
  }

  public readonly struct TapoDeviceExceptionHandling {
    public static readonly TapoDeviceExceptionHandling InvalidateEndPointAndRetry; // = "{ShouldRetry=True, RetryAfter=00:00:00, ShouldReestablishSession=False, ShouldWrapIntoTapoProtocolException=False, ShouldInvalidateEndPoint=True}"
    public static readonly TapoDeviceExceptionHandling InvalidateEndPointAndThrow; // = "{ShouldRetry=False, RetryAfter=00:00:00, ShouldReestablishSession=False, ShouldWrapIntoTapoProtocolException=False, ShouldInvalidateEndPoint=True}"
    public static readonly TapoDeviceExceptionHandling Retry; // = "{ShouldRetry=True, RetryAfter=00:00:00, ShouldReestablishSession=False, ShouldWrapIntoTapoProtocolException=False, ShouldInvalidateEndPoint=False}"
    public static readonly TapoDeviceExceptionHandling RetryAfterReestablishSession; // = "{ShouldRetry=True, RetryAfter=00:00:00, ShouldReestablishSession=True, ShouldWrapIntoTapoProtocolException=False, ShouldInvalidateEndPoint=False}"
    public static readonly TapoDeviceExceptionHandling Throw; // = "{ShouldRetry=False, RetryAfter=00:00:00, ShouldReestablishSession=False, ShouldWrapIntoTapoProtocolException=False, ShouldInvalidateEndPoint=False}"
    public static readonly TapoDeviceExceptionHandling ThrowAsTapoProtocolException; // = "{ShouldRetry=False, RetryAfter=00:00:00, ShouldReestablishSession=False, ShouldWrapIntoTapoProtocolException=True, ShouldInvalidateEndPoint=False}"

    public static TapoDeviceExceptionHandling CreateRetry(TimeSpan retryAfter, bool shouldReestablishSession = false) {}

    public TimeSpan RetryAfter { get; init; }
    public bool ShouldInvalidateEndPoint { get; init; }
    public bool ShouldReestablishSession { get; init; }
    public bool ShouldRetry { get; init; }
    public bool ShouldWrapIntoTapoProtocolException { get; init; }

    public override string ToString() {}
  }

  public readonly struct TapoDeviceOperatingTime {
    [JsonConverter(typeof(TimeSpanInMinutesJsonConverter))]
    [JsonPropertyName("past30")]
    public TimeSpan? Past30Days { get; init; }
    [JsonConverter(typeof(TimeSpanInMinutesJsonConverter))]
    [JsonPropertyName("past7")]
    public TimeSpan? Past7Days { get; init; }
    [JsonConverter(typeof(TimeSpanInMinutesJsonConverter))]
    [JsonPropertyName("today")]
    public TimeSpan? Today { get; init; }
  }
}

namespace Smdn.TPSmartHomeDevices.Tapo.Credentials {
  public interface ITapoCredential : IDisposable {
    [...] <unknown> WritePasswordPropertyValue(...);
    [...] <unknown> WriteUsernamePropertyValue(...);
  }

  public interface ITapoCredentialIdentity {
  }

  public interface ITapoCredentialProvider {
    ITapoCredential GetCredential(ITapoCredentialIdentity? identity);
    ITapoKlapCredential GetKlapCredential(ITapoCredentialIdentity? identity);
  }

  public interface ITapoKlapCredential : IDisposable {
    void WriteLocalAuthHash(Span<byte> destination);
  }

  public static class TapoCredentials {
    public const int HexSHA1HashSizeInBytes = 40;

    public static string ToBase64EncodedSHA1DigestString(ReadOnlySpan<char> str) {}
    public static string ToBase64EncodedString(ReadOnlySpan<char> str) {}
    public static bool TryComputeKlapLocalAuthHash(ReadOnlySpan<byte> username, ReadOnlySpan<byte> password, Span<byte> destination, out int bytesWritten) {}
    public static bool TryConvertToHexSHA1Hash(ReadOnlySpan<byte> input, Span<byte> destination, out int bytesWritten) {}
  }
}

namespace Smdn.TPSmartHomeDevices.Tapo.Json {
  public sealed class TapoBase64StringJsonConverter : JsonConverter<string> {
    public TapoBase64StringJsonConverter() {}

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {}
    [...] public override <unknown> Write(...) {}
  }

  public sealed class TapoElectricEnergyInWattHourJsonConverter : JsonConverter<decimal?> {
    public TapoElectricEnergyInWattHourJsonConverter() {}

    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {}
    [...] public override <unknown> Write(...) {}
  }

  public sealed class TapoElectricPowerInMilliWattJsonConverter : TapoElectricPowerJsonConverter {
    public TapoElectricPowerInMilliWattJsonConverter() {}
  }

  public sealed class TapoElectricPowerInWattJsonConverter : TapoElectricPowerJsonConverter {
    public TapoElectricPowerInWattJsonConverter() {}
  }

  public abstract class TapoElectricPowerJsonConverter : JsonConverter<decimal?> {
    protected TapoElectricPowerJsonConverter() {}

    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {}
    [...] public override <unknown> Write(...) {}
  }

  public sealed class TapoIPAddressJsonConverter : JsonConverter<IPAddress> {
    public TapoIPAddressJsonConverter() {}

    public override IPAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {}
    [...] public override <unknown> Write(...) {}
  }

  public sealed class TapoLocalDateAndTimeJsonConverter : JsonConverter<DateTime?> {
    public TapoLocalDateAndTimeJsonConverter() {}

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {}
    [...] public override <unknown> Write(...) {}
  }
}

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol {
  public interface ITapoPassThroughRequest : ITapoRequest {
  }

  public interface ITapoPassThroughResponse : ITapoResponse {
  }

  public interface ITapoRequest {
    string Method { get; }
  }

  public interface ITapoResponse {
    int ErrorCode { get; }
  }

  public enum TapoSessionProtocol : int {
    Klap = 1,
    SecurePassThrough = 0,
  }

  public static class HashAlgorithmExtensions {
    public static bool TryComputeHash(this HashAlgorithm algorithm, Span<byte> destination, ReadOnlySpan<byte> source0, ReadOnlySpan<byte> source1, ReadOnlySpan<byte> source2, ReadOnlySpan<byte> source3, out int bytesWritten) {}
    public static bool TryComputeHash(this HashAlgorithm algorithm, Span<byte> destination, ReadOnlySpan<byte> source0, ReadOnlySpan<byte> source1, ReadOnlySpan<byte> source2, out int bytesWritten) {}
  }

  public class KlapEncryptionAlgorithm {
    public KlapEncryptionAlgorithm(ReadOnlySpan<byte> localSeed, ReadOnlySpan<byte> remoteSeed, ReadOnlySpan<byte> userHash) {}

    public ReadOnlySpan<byte> IV { get; }
    public ReadOnlySpan<byte> Key { get; }
    public int SequenceNumber { get; }
    public ReadOnlySpan<byte> Signature { get; }

    public void Decrypt(ReadOnlySpan<byte> encryptedText, int sequenceNumber, IBufferWriter<byte> destination) {}
    public int Encrypt(ReadOnlySpan<byte> rawText, IBufferWriter<byte> destination) {}
    public void Encrypt(ReadOnlySpan<byte> rawText, int sequenceNumber, IBufferWriter<byte> destination) {}
  }

  public class SecurePassThroughInvalidPaddingException : SystemException {
    public SecurePassThroughInvalidPaddingException(string message, Exception? innerException) {}
  }

  public sealed class SecurePassThroughJsonConverterFactory :
    JsonConverterFactory,
    IDisposable
  {
    public SecurePassThroughJsonConverterFactory(ITapoCredentialIdentity? identity, ICryptoTransform? encryptorForPassThroughRequest, ICryptoTransform? decryptorForPassThroughResponse, JsonSerializerOptions? baseJsonSerializerOptionsForPassThroughMessage, ILogger? logger = null) {}

    public override bool CanConvert(Type typeToConvert) {}
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {}
    public void Dispose() {}
  }

  public sealed class TapoClient : IDisposable {
    public const int DefaultPort = 80;

    public static IHttpClientFactory DefaultHttpClientFactory { get; }

    public TapoClient(EndPoint endPoint, IHttpClientFactory? httpClientFactory = null, ILogger? logger = null) {}

    public Uri EndPointUri { get; }
    public TapoSession? Session { get; }
    public TimeSpan? Timeout { get; set; }

    public ValueTask AuthenticateAsync(ITapoCredentialIdentity? identity, ITapoCredentialProvider credential, CancellationToken cancellationToken = default) {}
    public ValueTask AuthenticateAsync(TapoSessionProtocol protocol, ITapoCredentialIdentity? identity, ITapoCredentialProvider credential, CancellationToken cancellationToken = default) {}
    public void Dispose() {}
    public ValueTask<TResponse> SendRequestAsync<TRequest, TResponse>(CancellationToken cancellationToken = default) where TRequest : ITapoPassThroughRequest, new() where TResponse : ITapoPassThroughResponse {}
    public ValueTask<TResponse> SendRequestAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) where TRequest : notnull, ITapoPassThroughRequest where TResponse : ITapoPassThroughResponse {}
  }

  public abstract class TapoSession : IDisposable {
    public DateTime ExpiresOn { get; }
    public bool HasExpired { get; }
    public abstract TapoSessionProtocol Protocol { get; }
    public string? SessionId { get; }
    public abstract string? Token { get; }

    protected virtual void Dispose(bool disposing) {}
    public void Dispose() {}
  }

  public static class TapoSessionCookieUtils {
    public static bool TryGetCookie(HttpResponseMessage response, out string? sessionId, out int? sessionTimeout) {}
    public static bool TryGetCookie(IEnumerable<string>? cookieValues, out string? sessionId, out int? sessionTimeout) {}
    public static bool TryParseCookie(ReadOnlySpan<char> cookie, out string? id, out int? timeout) {}
  }

  public abstract class TapoSessionProtocolSelector {
    protected TapoSessionProtocolSelector() {}

    public abstract TapoSessionProtocol? SelectProtocol(TapoDevice device);
  }

  public readonly struct GetCurrentPowerRequest : ITapoPassThroughRequest {
    [JsonPropertyName("method")]
    [JsonPropertyOrder(0)]
    public string Method { get; }
    [JsonPropertyName("requestTimeMils")]
    public long RequestTimeMilliseconds { get; }
  }

  public readonly struct GetDeviceInfoRequest : ITapoPassThroughRequest {
    [JsonPropertyName("method")]
    [JsonPropertyOrder(0)]
    public string Method { get; }
    [JsonPropertyName("requestTimeMils")]
    public long RequestTimeMilliseconds { get; }
  }

  public readonly struct GetDeviceUsageRequest : ITapoPassThroughRequest {
    [JsonPropertyName("method")]
    [JsonPropertyOrder(0)]
    public string Method { get; }
    [JsonPropertyName("requestTimeMils")]
    public long RequestTimeMilliseconds { get; }
  }

  public readonly struct GetEnergyUsageRequest : ITapoPassThroughRequest {
    [JsonPropertyName("method")]
    [JsonPropertyOrder(0)]
    public string Method { get; }
    [JsonPropertyName("requestTimeMils")]
    public long RequestTimeMilliseconds { get; }
  }

  public readonly struct HandshakeRequest : ITapoRequest {
    public readonly struct RequestParameters {
      [JsonPropertyName("key")]
      public string Key { get; init; }
      [JsonPropertyName("requestTimeMils")]
      public long RequestTimeMilliseconds { get; }
    }

    public HandshakeRequest(string key) {}

    [JsonPropertyName("method")]
    [JsonPropertyOrder(0)]
    public string Method { get; }
    [JsonPropertyName("params")]
    public HandshakeRequest.RequestParameters Parameters { get; }
  }

  public readonly struct HandshakeResponse : ITapoResponse {
    public readonly struct ResponseResult {
      [JsonPropertyName("key")]
      public string? Key { get; init; }
    }

    [JsonPropertyName("error_code")]
    public int ErrorCode { get; init; }
    [JsonPropertyName("result")]
    public HandshakeResponse.ResponseResult Result { get; init; }
  }

  public readonly struct LoginDeviceRequest : ITapoPassThroughRequest {
    public LoginDeviceRequest(ITapoCredentialProvider credential) {}

    [JsonPropertyName("method")]
    [JsonPropertyOrder(0)]
    public string Method { get; }
    [JsonPropertyName("params")]
    public ITapoCredentialProvider Parameters { get; }
    [JsonPropertyName("requestTimeMils")]
    public long RequestTimeMilliseconds { get; }
  }

  public readonly struct LoginDeviceResponse : ITapoPassThroughResponse {
    public readonly struct ResponseResult {
      [JsonPropertyName("token")]
      public string Token { get; init; }
    }

    [JsonPropertyName("error_code")]
    public int ErrorCode { get; init; }
    [JsonPropertyName("result")]
    public LoginDeviceResponse.ResponseResult Result { get; init; }
  }

  public readonly struct PassThroughResponse<TResult> : ITapoPassThroughResponse {
    [JsonPropertyName("error_code")]
    public int ErrorCode { get; init; }
    [JsonPropertyName("result")]
    public TResult Result { get; init; }
  }

  public readonly struct SecurePassThroughRequest<TPassThroughRequest> : ITapoRequest where TPassThroughRequest : notnull, ITapoPassThroughRequest {
    public readonly struct RequestParams where TPassThroughRequest : notnull, ITapoPassThroughRequest {
      [JsonPropertyName("request")]
      public TPassThroughRequest PassThroughRequest { get; init; }
    }

    public SecurePassThroughRequest(TPassThroughRequest passThroughRequest) {}

    [JsonPropertyName("method")]
    [JsonPropertyOrder(0)]
    public string Method { get; }
    [JsonPropertyName("params")]
    public SecurePassThroughRequest<TPassThroughRequest>.RequestParams Params { get; }
  }

  public readonly struct SecurePassThroughResponse<TPassThroughResponse> : ITapoResponse where TPassThroughResponse : ITapoPassThroughResponse {
    public readonly struct ResponseResult where TPassThroughResponse : notnull, ITapoPassThroughResponse {
      [JsonPropertyName("response")]
      public TPassThroughResponse PassThroughResponse { get; init; }
    }

    public SecurePassThroughResponse(int errorCode, TPassThroughResponse passThroughResponse) {}

    [JsonPropertyName("error_code")]
    public int ErrorCode { get; init; }
    [JsonPropertyName("result")]
    public SecurePassThroughResponse<TPassThroughResponse>.ResponseResult Result { get; init; }
  }

  public readonly struct SetDeviceInfoRequest<TParameters> : ITapoPassThroughRequest {
    public SetDeviceInfoRequest(string terminalUuid, TParameters parameters) {}

    [JsonPropertyName("method")]
    [JsonPropertyOrder(0)]
    public string Method { get; }
    [JsonPropertyName("params")]
    public TParameters Parameters { get; }
    [JsonPropertyName("requestTimeMils")]
    public long RequestTimeMilliseconds { get; }
    [JsonPropertyName("terminalUUID")]
    public string TerminalUuid { get; }
  }
}
// API list generated by Smdn.Reflection.ReverseGenerating.ListApi.MSBuild.Tasks v1.3.2.0.
// Smdn.Reflection.ReverseGenerating.ListApi.Core v1.2.0.0 (https://github.com/smdn/Smdn.Reflection.ReverseGenerating)
