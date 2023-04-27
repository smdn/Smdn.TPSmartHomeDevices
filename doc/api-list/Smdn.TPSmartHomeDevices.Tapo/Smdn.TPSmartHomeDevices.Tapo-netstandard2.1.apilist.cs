// Smdn.TPSmartHomeDevices.Tapo.dll (Smdn.TPSmartHomeDevices.Tapo-1.0.0-rc1)
//   Name: Smdn.TPSmartHomeDevices.Tapo
//   AssemblyVersion: 1.0.0.0
//   InformationalVersion: 1.0.0-rc1+00727d1f82dcb2b9dd9c6e586f6c54110349bf48
//   TargetFramework: .NETStandard,Version=v2.1
//   Configuration: Release
//   Referenced assemblies:
//     Microsoft.Extensions.DependencyInjection.Abstractions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Microsoft.Extensions.Http, Version=6.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Microsoft.Extensions.Logging.Abstractions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Smdn.Fundamental.PrintableEncoding.Hexadecimal, Version=3.0.1.0, Culture=neutral
//     Smdn.TPSmartHomeDevices.Primitives, Version=1.0.0.0, Culture=neutral
//     System.Net.Http.Json, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     System.Text.Encodings.Web, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     System.Text.Json, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
#nullable enable annotations

using System;
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
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo {
  public class L530 : TapoDevice {
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
  }

  public class L900 : TapoDevice {
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

  public class TapoAuthenticationException : TapoProtocolException {
    public TapoAuthenticationException(string message, Uri endPoint, Exception? innerException = null) {}
  }

  public static class TapoCredentailProviderServiceCollectionExtensions {
    public static IServiceCollection AddTapoBase64EncodedCredential(this IServiceCollection services, string base64UserNameSHA1Digest, string base64Password) {}
    public static IServiceCollection AddTapoCredential(this IServiceCollection services, string email, string password) {}
    public static IServiceCollection AddTapoCredentialProvider(this IServiceCollection services, ITapoCredentialProvider credentialProvider) {}
  }

  public class TapoDevice :
    IDisposable,
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

    protected TapoDevice(IDeviceEndPoint deviceEndPoint, ITapoCredentialProvider? credential = null, TapoDeviceExceptionHandler? exceptionHandler = null, IServiceProvider? serviceProvider = null) {}
    protected TapoDevice(IPAddress ipAddress, IServiceProvider serviceProvider) {}
    protected TapoDevice(IPAddress ipAddress, string email, string password, IServiceProvider? serviceProvider = null) {}
    protected TapoDevice(PhysicalAddress macAddress, IServiceProvider serviceProvider) {}
    protected TapoDevice(PhysicalAddress macAddress, string email, string password, IServiceProvider serviceProvider) {}
    protected TapoDevice(string host, IServiceProvider serviceProvider) {}
    protected TapoDevice(string host, string email, string password, IServiceProvider? serviceProvider = null) {}

    protected bool IsDisposed { get; }
    public TapoSession? Session { get; }
    string ITapoCredentialIdentity.Name { get; }
    public string TerminalUuidString { get; }
    public TimeSpan? Timeout { get; set; }

    protected virtual void Dispose(bool disposing) {}
    public void Dispose() {}
    protected ValueTask EnsureSessionEstablishedAsync(CancellationToken cancellationToken = default) {}
    public ValueTask<TDeviceInfo> GetDeviceInfoAsync<TDeviceInfo>(CancellationToken cancellationToken = default) {}
    public ValueTask<TResult> GetDeviceInfoAsync<TDeviceInfo, TResult>(Func<TDeviceInfo, TResult> composeResult, CancellationToken cancellationToken = default) {}
    public ValueTask<TapoDeviceInfo> GetDeviceInfoAsync(CancellationToken cancellationToken = default) {}
    public ValueTask<bool> GetOnOffStateAsync(CancellationToken cancellationToken = default) {}
    public ValueTask<EndPoint> ResolveEndPointAsync(CancellationToken cancellationToken = default) {}
    protected ValueTask SendRequestAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) where TRequest : notnull, ITapoPassThroughRequest where TResponse : ITapoPassThroughResponse {}
    protected ValueTask<TResult> SendRequestAsync<TRequest, TResponse, TResult>(TRequest request, Func<TResponse, TResult> composeResult, CancellationToken cancellationToken = default) where TRequest : notnull, ITapoPassThroughRequest where TResponse : ITapoPassThroughResponse {}
    public ValueTask SetDeviceInfoAsync<TDeviceInfo>(TDeviceInfo deviceInfo, CancellationToken cancellationToken = default) {}
    public ValueTask SetOnOffStateAsync(bool newOnOffState, CancellationToken cancellationToken = default) {}
    public ValueTask TurnOffAsync(CancellationToken cancellationToken = default) {}
    public ValueTask TurnOnAsync(CancellationToken cancellationToken = default) {}
  }

  public abstract class TapoDeviceExceptionHandler {
    internal protected static readonly TapoDeviceExceptionHandler Default; // = "Smdn.TPSmartHomeDevices.Tapo.TapoDeviceDefaultExceptionHandler"

    protected TapoDeviceExceptionHandler() {}

    public abstract TapoDeviceExceptionHandling DetermineHandling(TapoDevice device, Exception exception, int attempt, ILogger? logger);
  }

  public class TapoDeviceInfo {
    public TapoDeviceInfo() {}

    [JsonPropertyName("avatar")]
    public string? Avatar { get; init; }
    [JsonConverter(typeof(TapoBase16ByteArrayJsonConverter))]
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
    [JsonConverter(typeof(TapoBase16ByteArrayJsonConverter))]
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
    [JsonConverter(typeof(TapoBase16ByteArrayJsonConverter))]
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
    [JsonConverter(typeof(TapoBase16ByteArrayJsonConverter))]
    [JsonPropertyName("oem_id")]
    public byte[]? OemId { get; init; }
    [JsonConverter(typeof(TimeSpanInSecondsJsonConverter))]
    [JsonPropertyName("on_time")]
    public TimeSpan? OnTimeDuration { get; init; }
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

  public class TapoProtocolException : InvalidOperationException {
    internal protected TapoProtocolException(string message, Uri endPoint, Exception? innerException) {}

    public Uri EndPoint { get; }
  }

  public readonly struct TapoDeviceExceptionHandling {
    public static readonly TapoDeviceExceptionHandling InvalidateEndPointAndRetry; // = "{ShouldRetry=True, RetryAfter=00:00:00, ShouldReconnect=False, ShouldWrapIntoTapoProtocolException=False, ShouldInvalidateEndPoint=True}"
    public static readonly TapoDeviceExceptionHandling InvalidateEndPointAndThrow; // = "{ShouldRetry=False, RetryAfter=00:00:00, ShouldReconnect=False, ShouldWrapIntoTapoProtocolException=False, ShouldInvalidateEndPoint=True}"
    public static readonly TapoDeviceExceptionHandling Retry; // = "{ShouldRetry=True, RetryAfter=00:00:00, ShouldReconnect=False, ShouldWrapIntoTapoProtocolException=False, ShouldInvalidateEndPoint=False}"
    public static readonly TapoDeviceExceptionHandling RetryAfterReconnect; // = "{ShouldRetry=True, RetryAfter=00:00:00, ShouldReconnect=True, ShouldWrapIntoTapoProtocolException=False, ShouldInvalidateEndPoint=False}"
    public static readonly TapoDeviceExceptionHandling Throw; // = "{ShouldRetry=False, RetryAfter=00:00:00, ShouldReconnect=False, ShouldWrapIntoTapoProtocolException=False, ShouldInvalidateEndPoint=False}"
    public static readonly TapoDeviceExceptionHandling ThrowAsTapoProtocolException; // = "{ShouldRetry=False, RetryAfter=00:00:00, ShouldReconnect=False, ShouldWrapIntoTapoProtocolException=True, ShouldInvalidateEndPoint=False}"

    public static TapoDeviceExceptionHandling CreateRetry(TimeSpan retryAfter, bool shouldReconnect = false) {}

    public TimeSpan RetryAfter { get; init; }
    public bool ShouldInvalidateEndPoint { get; init; }
    public bool ShouldReconnect { get; init; }
    public bool ShouldRetry { get; init; }
    public bool ShouldWrapIntoTapoProtocolException { get; init; }

    public override string ToString() {}
  }
}

namespace Smdn.TPSmartHomeDevices.Tapo.Credentials {
  public interface ITapoCredential : IDisposable {
    [...] <unknown> WritePasswordPropertyValue(...);
    [...] <unknown> WriteUsernamePropertyValue(...);
  }

  public interface ITapoCredentialIdentity {
    string Name { get; }
  }

  public interface ITapoCredentialProvider {
    ITapoCredential GetCredential(ITapoCredentialIdentity? identity);
  }

  public static class TapoCredentials {
    public const int HexSHA1HashSizeInBytes = 40;

    public static string ToBase64EncodedSHA1DigestString(ReadOnlySpan<char> str) {}
    public static string ToBase64EncodedString(ReadOnlySpan<char> str) {}
    public static bool TryConvertToHexSHA1Hash(ReadOnlySpan<byte> input, Span<byte> destination, out int bytesWritten) {}
  }
}

namespace Smdn.TPSmartHomeDevices.Tapo.Json {
  public sealed class TapoBase16ByteArrayJsonConverter : JsonConverter<byte[]> {
    public TapoBase16ByteArrayJsonConverter() {}

    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {}
    [...] public override <unknown> Write(...) {}
  }

  public sealed class TapoBase64StringJsonConverter : JsonConverter<string> {
    public TapoBase64StringJsonConverter() {}

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {}
    [...] public override <unknown> Write(...) {}
  }

  public sealed class TapoIPAddressJsonConverter : JsonConverter<IPAddress> {
    public TapoIPAddressJsonConverter() {}

    public override IPAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {}
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
    public void Dispose() {}
    public ValueTask<TResponse> SendRequestAsync<TRequest, TResponse>(CancellationToken cancellationToken = default) where TRequest : ITapoPassThroughRequest, new() where TResponse : ITapoPassThroughResponse {}
    public ValueTask<TResponse> SendRequestAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) where TRequest : notnull, ITapoPassThroughRequest where TResponse : ITapoPassThroughResponse {}
  }

  public sealed class TapoSession : IDisposable {
    public DateTime ExpiresOn { get; }
    public bool HasExpired { get; }
    public Uri RequestPathAndQuery { get; }
    public string? SessionId { get; }
    public string? Token { get; }

    public void Dispose() {}
  }

  public static class TapoSessionCookieUtils {
    public static bool TryGetCookie(HttpResponseMessage response, out string? sessionId, out int? sessionTimeout) {}
    public static bool TryGetCookie(IEnumerable<string>? cookieValues, out string? sessionId, out int? sessionTimeout) {}
    public static bool TryParseCookie(ReadOnlySpan<char> cookie, out string? id, out int? timeout) {}
  }

  public readonly struct GetDeviceInfoRequest : ITapoPassThroughRequest {
    [JsonPropertyName("method")]
    [JsonPropertyOrder(0)]
    public string Method { get; }
    [JsonPropertyName("requestTimeMils")]
    public long RequestTimeMilliseconds { get; }
  }

  public readonly struct GetDeviceInfoResponse<TResult> : ITapoPassThroughResponse {
    [JsonPropertyName("error_code")]
    public int ErrorCode { get; init; }
    [JsonPropertyName("result")]
    public TResult Result { get; init; }
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

  public readonly struct SetDeviceInfoResponse : ITapoPassThroughResponse {
    public readonly struct ResponseResult {
      [JsonExtensionData]
      public IDictionary<string, object>? ExtraData { get; init; }
    }

    [JsonPropertyName("error_code")]
    public int ErrorCode { get; init; }
    [JsonPropertyName("result")]
    public SetDeviceInfoResponse.ResponseResult Result { get; init; }
  }
}
// API list generated by Smdn.Reflection.ReverseGenerating.ListApi.MSBuild.Tasks v1.2.2.0.
// Smdn.Reflection.ReverseGenerating.ListApi.Core v1.2.0.0 (https://github.com/smdn/Smdn.Reflection.ReverseGenerating)
