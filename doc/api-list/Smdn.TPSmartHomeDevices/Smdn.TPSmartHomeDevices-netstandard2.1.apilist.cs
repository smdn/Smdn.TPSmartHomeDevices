// Smdn.TPSmartHomeDevices.dll (Smdn.TPSmartHomeDevices-1.0.0-preview1)
//   Name: Smdn.TPSmartHomeDevices
//   AssemblyVersion: 1.0.0.0
//   InformationalVersion: 1.0.0-preview1+e06e7ca85c2de4ff93d0da638a7853e7f837e6ce
//   TargetFramework: .NETStandard,Version=v2.1
//   Configuration: Release
//   Referenced assemblies:
//     Microsoft.Extensions.DependencyInjection.Abstractions, Version=7.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Microsoft.Extensions.Http, Version=7.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Microsoft.Extensions.Logging.Abstractions, Version=7.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Smdn.Fundamental.PrintableEncoding.Hexadecimal, Version=3.0.1.0, Culture=neutral
//     Smdn.Net.AddressResolution, Version=1.0.0.0, Culture=neutral
//     System.Net.Http.Json, Version=7.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     System.Text.Encodings.Web, Version=7.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     System.Text.Json, Version=7.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
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
using Smdn.Net.AddressResolution;
using Smdn.TPSmartHomeDevices;
using Smdn.TPSmartHomeDevices.Kasa;
using Smdn.TPSmartHomeDevices.Kasa.Protocol;
using Smdn.TPSmartHomeDevices.Tapo;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices {
  public interface IDeviceEndPointProvider {
    bool IsStaticEndPoint { get; }

    ValueTask<EndPoint?> GetEndPointAsync(CancellationToken cancellationToken = default);
  }

  public class DeviceEndPointResolutionException : Exception {
    public DeviceEndPointResolutionException(IDeviceEndPointProvider deviceEndPointProvider) {}
    public DeviceEndPointResolutionException(IDeviceEndPointProvider deviceEndPointProvider, string message, Exception? innerException) {}

    public IDeviceEndPointProvider EndPointProvider { get; }
  }

  public class MacAddressDeviceEndPointFactory : IDisposable {
    protected class MacAddressDeviceEndPointProvider : IDeviceEndPointProvider {
      public MacAddressDeviceEndPointProvider(IAddressResolver<PhysicalAddress, IPAddress> resolver, PhysicalAddress address, int port) {}

      public bool IsStaticEndPoint { get; }

      public async ValueTask<EndPoint?> GetEndPointAsync(CancellationToken cancellationToken) {}
    }

    protected MacAddressDeviceEndPointFactory(IAddressResolver<PhysicalAddress, IPAddress> resolver, IServiceProvider? serviceProvider = null) {}
    public MacAddressDeviceEndPointFactory(MacAddressResolver resolver, IServiceProvider? serviceProvider = null) {}
    public MacAddressDeviceEndPointFactory(MacAddressResolverOptions? options = null, IServiceProvider? serviceProvider = null) {}

    public virtual IDeviceEndPointProvider Create(PhysicalAddress address, int port = 0) {}
    protected virtual void Dispose(bool disposing) {}
    public void Dispose() {}
    protected void ThrowIfDisposed() {}
  }
}

namespace Smdn.TPSmartHomeDevices.Kasa {
  public class HS105 : KasaDevice {
    public HS105(IDeviceEndPointProvider deviceEndPointProvider, IServiceProvider? serviceProvider = null) {}
    public HS105(IPAddress ipAddress, IServiceProvider? serviceProvider = null) {}
    public HS105(string hostName, IServiceProvider? serviceProvider = null) {}

    public Task<bool> GetOnOffStateAsync(CancellationToken cancellationToken = default) {}
    public Task SetOnOffStateAsync(bool newOnOffState, CancellationToken cancellationToken = default) {}
    public Task TurnOffAsync(CancellationToken cancellationToken = default) {}
    public Task TurnOnAsync(CancellationToken cancellationToken = default) {}
  }

  public class KL130 : KasaDevice {
    public KL130(IDeviceEndPointProvider deviceEndPointProvider, IServiceProvider? serviceProvider = null) {}
    public KL130(IPAddress ipAddress, IServiceProvider? serviceProvider = null) {}
    public KL130(string hostName, IServiceProvider? serviceProvider = null) {}

    public Task<KL130LightState> GetLightStateAsync(CancellationToken cancellationToken = default) {}
    public Task<bool> GetOnOffStateAsync(CancellationToken cancellationToken = default) {}
    public Task SetColorAsync(int hue, int saturation, int? brightness = null, TimeSpan? transitionPeriod = null, CancellationToken cancellationToken = default) {}
    public Task SetColorTemperatureAsync(int colorTemperature, int? brightness = null, TimeSpan? transitionPeriod = null, CancellationToken cancellationToken = default) {}
    public Task SetOnOffStateAsync(bool newOnOffState, TimeSpan? transitionPeriod = null, CancellationToken cancellationToken = default) {}
    public Task TurnOffAsync(TimeSpan? transitionPeriod = null, CancellationToken cancellationToken = default) {}
    public Task TurnOnAsync(TimeSpan? transitionPeriod = null, CancellationToken cancellationToken = default) {}
  }

  public class KasaDevice : IDisposable {
    protected readonly struct NullParameter {
    }

    protected static readonly JsonEncodedText MethodTextGetSysInfo;
    protected static readonly JsonEncodedText ModuleTextSystem;

    public static KasaDevice Create(IDeviceEndPointProvider deviceEndPointProvider, IServiceProvider? serviceProvider = null) {}
    public static KasaDevice Create(IPAddress deviceAddress, IServiceProvider? serviceProvider = null) {}
    public static KasaDevice Create(string hostName, IServiceProvider? serviceProvider = null) {}

    protected KasaDevice(IDeviceEndPointProvider deviceEndPointProvider, IServiceProvider? serviceProvider = null) {}
    protected KasaDevice(IPAddress ipAddress, IServiceProvider? serviceProvider = null) {}
    protected KasaDevice(string hostName, IServiceProvider? serviceProvider = null) {}

    public bool IsConnected { get; }
    protected bool IsDisposed { get; }

    protected virtual void Dispose(bool disposing) {}
    public void Dispose() {}
    public ValueTask<EndPoint> ResolveEndPointAsync(CancellationToken cancellationToken = default) {}
    protected Task SendRequestAsync<TMethodParameter>(JsonEncodedText module, JsonEncodedText method, TMethodParameter parameters, CancellationToken cancellationToken) {}
    protected Task<TMethodResult> SendRequestAsync<TMethodParameter, TMethodResult>(JsonEncodedText module, JsonEncodedText method, TMethodParameter parameters, Func<JsonElement, TMethodResult> composeResult, CancellationToken cancellationToken) {}
    protected Task<TMethodResult> SendRequestAsync<TMethodResult>(JsonEncodedText module, JsonEncodedText method, Func<JsonElement, TMethodResult> composeResult, CancellationToken cancellationToken) {}
  }

  public static class KasaDeviceEndPointProvider {
    public static IDeviceEndPointProvider Create(IPAddress ipAddress) {}
    public static IDeviceEndPointProvider Create(string hostName) {}
  }

  public class KasaErrorResponseException : KasaUnexpectedResponseException {
    public KasaErrorResponseException(EndPoint deviceEndPoint, string requestModule, string requestMethod, ErrorCode errorCode) {}

    public ErrorCode ErrorCode { get; }
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
}

namespace Smdn.TPSmartHomeDevices.Kasa.Protocol {
  public enum ErrorCode : int {
    Success = 0,
  }

  public sealed class KasaClient : IDisposable {
    public const int DefaultPort = 9999;

    public KasaClient(EndPoint endPoint, IServiceProvider? serviceProvider = null) {}

    public EndPoint EndPoint { get; }
    public bool IsConnected { get; }

    public void Dispose() {}
    public Task<TMethodResult> SendAsync<TMethodParameter, TMethodResult>(JsonEncodedText module, JsonEncodedText method, TMethodParameter parameter, Func<JsonElement, TMethodResult> composeResult, CancellationToken cancellationToken = default) {}
  }

  public static class KasaJsonSerializer {
    public const byte InitialKey = 171;

    public static void DecryptInPlace(Span<byte> body) {}
    public static JsonElement Deserialize(ArrayBufferWriter<byte> buffer, JsonEncodedText module, JsonEncodedText method, ILogger? logger = null) {}
    public static void EncryptInPlace(Span<byte> body) {}
    public static void Serialize<TMethodParameter>(ArrayBufferWriter<byte> buffer, JsonEncodedText module, JsonEncodedText method, TMethodParameter parameter, ILogger? logger = null) {}
  }
}

namespace Smdn.TPSmartHomeDevices.Tapo {
  public interface ITapoCredentialProvider {
    string GetBase64EncodedPassword(string host);
    string GetBase64EncodedUserNameSHA1Digest(string host);
  }

  public class L530 : TapoDevice {
    public L530(IDeviceEndPointProvider deviceEndPointProvider, Guid? terminalUuid = null, ITapoCredentialProvider? credentialProvider = null, IServiceProvider? serviceProvider = null) {}
    public L530(IPAddress ipAddress, string email, string password, IServiceProvider? serviceProvider = null) {}
    public L530(string hostName, IServiceProvider? serviceProvider = null) {}
    public L530(string hostName, string email, string password, IServiceProvider? serviceProvider = null) {}

    public Task SetBrightnessAsync(int brightness, CancellationToken cancellationToken = default) {}
    public Task SetColorAsync(int hue, int saturation, int? brightness = null, CancellationToken cancellationToken = default) {}
    public Task SetColorHueAsync(int hue, int? brightness = null, CancellationToken cancellationToken = default) {}
    public Task SetColorSaturationAsync(int saturation, int? brightness = null, CancellationToken cancellationToken = default) {}
    public Task SetColorTemperatureAsync(int colorTemperature, int? brightness = null, CancellationToken cancellationToken = default) {}
  }

  public class L900 : TapoDevice {
    public L900(IDeviceEndPointProvider deviceEndPointProvider, Guid? terminalUuid = null, ITapoCredentialProvider? credentialProvider = null, IServiceProvider? serviceProvider = null) {}
    public L900(IPAddress ipAddress, string email, string password, IServiceProvider? serviceProvider = null) {}
    public L900(string hostName, IServiceProvider? serviceProvider = null) {}
    public L900(string hostName, string email, string password, IServiceProvider? serviceProvider = null) {}

    public Task SetBrightnessAsync(int brightness, CancellationToken cancellationToken = default) {}
    public Task SetColorAsync(int hue, int saturation, int? brightness = null, CancellationToken cancellationToken = default) {}
    public Task SetColorHueAsync(int hue, int? brightness, CancellationToken cancellationToken = default) {}
    public Task SetColorSaturationAsync(int saturation, int? brightness = null, CancellationToken cancellationToken = default) {}
  }

  public class P105 : TapoDevice {
    public P105(IDeviceEndPointProvider deviceEndPointProvider, Guid? terminalUuid = null, ITapoCredentialProvider? credentialProvider = null, IServiceProvider? serviceProvider = null) {}
    public P105(IPAddress ipAddress, string email, string password, IServiceProvider? serviceProvider = null) {}
    public P105(string hostName, IServiceProvider? serviceProvider = null) {}
    public P105(string hostName, string email, string password, IServiceProvider? serviceProvider = null) {}
  }

  public class TapoAuthenticationException : TapoProtocolException {
    public TapoAuthenticationException(string message, Uri endPoint, Exception? innerException = null) {}
  }

  public static class TapoCredentailProviderServiceCollectionExtensions {
    public static IServiceCollection AddTapoBase64EncodedCredential(this IServiceCollection services, string base64UserNameSHA1Digest, string base64Password) {}
    public static IServiceCollection AddTapoCredential(this IServiceCollection services, string userName, string password) {}
  }

  public class TapoDevice : IDisposable {
    public static TapoDevice Create(IDeviceEndPointProvider deviceEndPointProvider, Guid? terminalUuid = null, ITapoCredentialProvider? credentialProvider = null, IServiceProvider? serviceProvider = null) {}
    public static TapoDevice Create(IPAddress deviceAddress, string email, string password, IServiceProvider? serviceProvider = null) {}
    public static TapoDevice Create(string hostName, string email, string password, IServiceProvider? serviceProvider = null) {}

    protected TapoDevice(IDeviceEndPointProvider deviceEndPointProvider, Guid? terminalUuid = null, ITapoCredentialProvider? credentialProvider = null, IServiceProvider? serviceProvider = null) {}
    protected TapoDevice(IPAddress ipAddress, string email, string password, Guid? terminalUuid = null, IServiceProvider? serviceProvider = null) {}
    protected TapoDevice(string hostName, Guid? terminalUuid = null, IServiceProvider? serviceProvider = null) {}
    protected TapoDevice(string hostName, string email, string password, Guid? terminalUuid = null, IServiceProvider? serviceProvider = null) {}

    protected bool IsDisposed { get; }
    public TapoSession? Session { get; }
    public string TerminalUuidString { get; }

    protected virtual void Dispose(bool disposing) {}
    public void Dispose() {}
    protected ValueTask EnsureSessionEstablishedAsync(CancellationToken cancellationToken = default) {}
    public Task<TapoDeviceInfo> GetDeviceInfoAsync(CancellationToken cancellationToken = default) {}
    public ValueTask<EndPoint> ResolveEndPointAsync(CancellationToken cancellationToken = default) {}
    protected Task<TResult> SendRequestAsync<TRequest, TResponse, TResult>(TRequest request, Func<TResponse, TResult> composeResult, CancellationToken cancellationToken = default) where TRequest : ITapoPassThroughRequest where TResponse : ITapoPassThroughResponse {}
    public Task SetDeviceInfoAsync<TParameters>(TParameters parameters, CancellationToken cancellationToken = default) {}
    public Task SetOnOffStateAsync(bool newOnOffState, CancellationToken cancellationToken = default) {}
    public Task TurnOffAsync(CancellationToken cancellationToken = default) {}
    public Task TurnOnAsync(CancellationToken cancellationToken = default) {}
  }

  public static class TapoDeviceEndPointProvider {
    public static IDeviceEndPointProvider Create(IPAddress ipAddress) {}
    public static IDeviceEndPointProvider Create(string hostName) {}
  }

  public class TapoDeviceInfo {
    public TapoDeviceInfo() {}

    [JsonPropertyName("avatar")]
    public string? Avatar { get; init; }
    [JsonPropertyName("fw_id")]
    public string? FirmwareId { get; init; }
    [JsonPropertyName("fw_ver")]
    public string? FirmwareVersion { get; init; }
    [JsonConverter(typeof(GeolocationInDecimalDegreesJsonConverter))]
    [JsonPropertyName("latitude")]
    public decimal? GeolocationLatitude { get; init; }
    [JsonConverter(typeof(GeolocationInDecimalDegreesJsonConverter))]
    [JsonPropertyName("longitude")]
    public decimal? GeolocationLongitude { get; init; }
    [JsonPropertyName("hw_id")]
    public string? HardwareId { get; init; }
    [JsonPropertyName("specs")]
    public string? HardwareSpecifications { get; init; }
    [JsonPropertyName("hw_ver")]
    public string? HardwareVersion { get; init; }
    [JsonPropertyName("has_set_location_info")]
    public bool HasGeolocationInfoSet { get; init; }
    [JsonConverter(typeof(TapoIPAddressJsonConverter))]
    [JsonPropertyName("ip")]
    public IPAddress? IPAddress { get; init; }
    [JsonPropertyName("device_id")]
    public string? Id { get; init; }
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
    [JsonPropertyName("oem_id")]
    public string? OemId { get; init; }
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
    public TapoErrorResponseException(Uri requestEndPoint, string requestMethod, ErrorCode errorCode) {}

    public ErrorCode ErrorCode { get; }
    public string RequestMethod { get; }
  }

  public static class TapoHttpClientFactoryServiceCollectionExtensions {
    public static IHttpClientBuilder AddTapoHttpClient(this IServiceCollection services, string name, Action<IServiceProvider, HttpClient>? configureClient = null) {}
    public static IHttpClientBuilder AddTapoHttpClient(this IServiceCollection services, string name, TimeSpan timeout, Action<IServiceProvider, HttpClient>? configureClient = null) {}
  }

  public abstract class TapoProtocolException : InvalidOperationException {
    protected TapoProtocolException(string message, Uri endPoint, Exception? innerException) {}

    public Uri EndPoint { get; }
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
    ErrorCode ErrorCode { get; init; }
  }

  public enum ErrorCode : int {
    Success = 0,
  }

  public sealed class SecurePassThroughJsonConverterFactory :
    JsonConverterFactory,
    IDisposable
  {
    public SecurePassThroughJsonConverterFactory(ICryptoTransform? encryptorForPassThroughRequest, ICryptoTransform? decryptorForPassThroughResponse, JsonSerializerOptions? plainTextJsonSerializerOptions, ILogger? logger = null) {}

    public override bool CanConvert(Type typeToConvert) {}
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {}
    public void Dispose() {}
  }

  public sealed class TapoClient : IDisposable {
    public const int DefaultPort = 80;

    public TapoClient(EndPoint endPoint, ITapoCredentialProvider? credentialProvider = null, IServiceProvider? serviceProvider = null) {}

    public Uri EndPointUri { get; }
    public TapoSession? Session { get; }

    public Task AuthenticateAsync(CancellationToken cancellationToken = default) {}
    public void Dispose() {}
    public Task<TResponse> SendRequestAsync<TRequest, TResponse>(CancellationToken cancellationToken = default) where TRequest : ITapoPassThroughRequest, new() where TResponse : ITapoPassThroughResponse {}
    public Task<TResponse> SendRequestAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) where TRequest : ITapoPassThroughRequest where TResponse : ITapoPassThroughResponse {}
  }

  public static class TapoCredentialUtils {
    public static string ToBase64EncodedSHA1DigestString(ReadOnlySpan<char> str) {}
    public static string ToBase64EncodedString(ReadOnlySpan<char> str) {}
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

  public readonly struct GetDeviceInfoResponse : ITapoPassThroughResponse {
    [JsonPropertyName("error_code")]
    public ErrorCode ErrorCode { get; init; }
    [JsonPropertyName("result")]
    public TapoDeviceInfo Result { get; init; }
  }

  public readonly struct HandshakeRequest : ITapoRequest {
    public readonly struct RequestParameters : IEquatable<RequestParameters> {
      [CompilerGenerated]
      public static bool operator == (HandshakeRequest.RequestParameters left, HandshakeRequest.RequestParameters right) {}
      [CompilerGenerated]
      public static bool operator != (HandshakeRequest.RequestParameters left, HandshakeRequest.RequestParameters right) {}

      public RequestParameters(string Key) {}

      [JsonPropertyName("key")]
      public string Key { get; init; }
      [JsonPropertyName("requestTimeMils")]
      public long RequestTimeMilliseconds { get; }

      [CompilerGenerated]
      public void Deconstruct(out string Key) {}
      [CompilerGenerated]
      public bool Equals(HandshakeRequest.RequestParameters other) {}
      [CompilerGenerated]
      public override bool Equals(object obj) {}
      [CompilerGenerated]
      public override int GetHashCode() {}
      [CompilerGenerated]
      public override string ToString() {}
    }

    public HandshakeRequest(string key) {}

    [JsonPropertyName("method")]
    [JsonPropertyOrder(0)]
    public string Method { get; }
    [JsonPropertyName("params")]
    public HandshakeRequest.RequestParameters Parameters { get; }
  }

  public readonly struct HandshakeResponse : ITapoResponse {
    public readonly struct ResponseResult : IEquatable<ResponseResult> {
      [CompilerGenerated]
      public static bool operator == (HandshakeResponse.ResponseResult left, HandshakeResponse.ResponseResult right) {}
      [CompilerGenerated]
      public static bool operator != (HandshakeResponse.ResponseResult left, HandshakeResponse.ResponseResult right) {}

      public ResponseResult(string? Key) {}

      [JsonPropertyName("key")]
      public string? Key { get; init; }

      [CompilerGenerated]
      public void Deconstruct(out string? Key) {}
      [CompilerGenerated]
      public bool Equals(HandshakeResponse.ResponseResult other) {}
      [CompilerGenerated]
      public override bool Equals(object obj) {}
      [CompilerGenerated]
      public override int GetHashCode() {}
      [CompilerGenerated]
      public override string ToString() {}
    }

    [JsonPropertyName("error_code")]
    public ErrorCode ErrorCode { get; init; }
    [JsonPropertyName("result")]
    public HandshakeResponse.ResponseResult Result { get; init; }
  }

  public readonly struct LoginDeviceRequest : ITapoPassThroughRequest {
    public readonly struct RequestParameters : IEquatable<RequestParameters> {
      [CompilerGenerated]
      public static bool operator == (LoginDeviceRequest.RequestParameters left, LoginDeviceRequest.RequestParameters right) {}
      [CompilerGenerated]
      public static bool operator != (LoginDeviceRequest.RequestParameters left, LoginDeviceRequest.RequestParameters right) {}

      public RequestParameters(string Password, string UserName) {}

      [JsonPropertyName("password")]
      public string Password { get; init; }
      [JsonPropertyName("username")]
      public string UserName { get; init; }

      [CompilerGenerated]
      public void Deconstruct(out string Password, out string UserName) {}
      [CompilerGenerated]
      public bool Equals(LoginDeviceRequest.RequestParameters other) {}
      [CompilerGenerated]
      public override bool Equals(object obj) {}
      [CompilerGenerated]
      public override int GetHashCode() {}
      [CompilerGenerated]
      public override string ToString() {}
    }

    public LoginDeviceRequest(string password, string userName) {}

    [JsonPropertyName("method")]
    [JsonPropertyOrder(0)]
    public string Method { get; }
    [JsonPropertyName("params")]
    public LoginDeviceRequest.RequestParameters Parameters { get; }
    [JsonPropertyName("requestTimeMils")]
    public long RequestTimeMilliseconds { get; }
  }

  public readonly struct LoginDeviceResponse : ITapoPassThroughResponse {
    public readonly struct ResponseResult : IEquatable<ResponseResult> {
      [CompilerGenerated]
      public static bool operator == (LoginDeviceResponse.ResponseResult left, LoginDeviceResponse.ResponseResult right) {}
      [CompilerGenerated]
      public static bool operator != (LoginDeviceResponse.ResponseResult left, LoginDeviceResponse.ResponseResult right) {}

      public ResponseResult(string Token) {}

      [JsonPropertyName("token")]
      public string Token { get; init; }

      [CompilerGenerated]
      public void Deconstruct(out string Token) {}
      [CompilerGenerated]
      public bool Equals(LoginDeviceResponse.ResponseResult other) {}
      [CompilerGenerated]
      public override bool Equals(object obj) {}
      [CompilerGenerated]
      public override int GetHashCode() {}
      [CompilerGenerated]
      public override string ToString() {}
    }

    [JsonPropertyName("error_code")]
    public ErrorCode ErrorCode { get; init; }
    [JsonPropertyName("result")]
    public LoginDeviceResponse.ResponseResult Result { get; init; }
  }

  public readonly struct SecurePassThroughRequest<TPassThroughRequest> : ITapoRequest where TPassThroughRequest : ITapoPassThroughRequest {
    public readonly struct RequestParams : IEquatable<RequestParams> where TPassThroughRequest : ITapoPassThroughRequest {
      [CompilerGenerated]
      public static bool operator == (SecurePassThroughRequest<TPassThroughRequest>.RequestParams left, SecurePassThroughRequest<TPassThroughRequest>.RequestParams right) {}
      [CompilerGenerated]
      public static bool operator != (SecurePassThroughRequest<TPassThroughRequest>.RequestParams left, SecurePassThroughRequest<TPassThroughRequest>.RequestParams right) {}

      public RequestParams(TPassThroughRequest PassThroughRequest) {}

      [JsonPropertyName("request")]
      public TPassThroughRequest PassThroughRequest { get; init; }

      [CompilerGenerated]
      public void Deconstruct(out TPassThroughRequest PassThroughRequest) {}
      [CompilerGenerated]
      public bool Equals(SecurePassThroughRequest<TPassThroughRequest>.RequestParams other) {}
      [CompilerGenerated]
      public override bool Equals(object obj) {}
      [CompilerGenerated]
      public override int GetHashCode() {}
      [CompilerGenerated]
      public override string ToString() {}
    }

    public SecurePassThroughRequest(TPassThroughRequest passThroughRequest) {}

    [JsonPropertyName("method")]
    [JsonPropertyOrder(0)]
    public string Method { get; }
    [JsonPropertyName("params")]
    public SecurePassThroughRequest<TPassThroughRequest>.RequestParams Params { get; }
  }

  public readonly struct SecurePassThroughResponse<TPassThroughResponse> : ITapoResponse where TPassThroughResponse : ITapoPassThroughResponse {
    public readonly struct ResponseResult : IEquatable<ResponseResult> where TPassThroughResponse : ITapoPassThroughResponse {
      [CompilerGenerated]
      public static bool operator == (SecurePassThroughResponse<TPassThroughResponse>.ResponseResult left, SecurePassThroughResponse<TPassThroughResponse>.ResponseResult right) {}
      [CompilerGenerated]
      public static bool operator != (SecurePassThroughResponse<TPassThroughResponse>.ResponseResult left, SecurePassThroughResponse<TPassThroughResponse>.ResponseResult right) {}

      public ResponseResult(TPassThroughResponse PassThroughResponse) {}

      [JsonPropertyName("response")]
      public TPassThroughResponse PassThroughResponse { get; init; }

      [CompilerGenerated]
      public void Deconstruct(out TPassThroughResponse PassThroughResponse) {}
      [CompilerGenerated]
      public bool Equals(SecurePassThroughResponse<TPassThroughResponse>.ResponseResult other) {}
      [CompilerGenerated]
      public override bool Equals(object obj) {}
      [CompilerGenerated]
      public override int GetHashCode() {}
      [CompilerGenerated]
      public override string ToString() {}
    }

    public SecurePassThroughResponse(ErrorCode errorCode, TPassThroughResponse passThroughResponse) {}

    [JsonPropertyName("error_code")]
    public ErrorCode ErrorCode { get; init; }
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
    public readonly struct ResponseResult : IEquatable<ResponseResult> {
      [CompilerGenerated]
      public static bool operator == (SetDeviceInfoResponse.ResponseResult left, SetDeviceInfoResponse.ResponseResult right) {}
      [CompilerGenerated]
      public static bool operator != (SetDeviceInfoResponse.ResponseResult left, SetDeviceInfoResponse.ResponseResult right) {}

      public ResponseResult(IDictionary<string, object>? ExtraData) {}

      [JsonExtensionData]
      public IDictionary<string, object>? ExtraData { get; init; }

      [CompilerGenerated]
      public void Deconstruct(out IDictionary<string, object>? ExtraData) {}
      [CompilerGenerated]
      public bool Equals(SetDeviceInfoResponse.ResponseResult other) {}
      [CompilerGenerated]
      public override bool Equals(object obj) {}
      [CompilerGenerated]
      public override int GetHashCode() {}
      [CompilerGenerated]
      public override string ToString() {}
    }

    [JsonPropertyName("error_code")]
    public ErrorCode ErrorCode { get; init; }
    [JsonPropertyName("result")]
    public SetDeviceInfoResponse.ResponseResult Result { get; init; }
  }
}
// API list generated by Smdn.Reflection.ReverseGenerating.ListApi.MSBuild.Tasks v1.2.1.0.
// Smdn.Reflection.ReverseGenerating.ListApi.Core v1.2.0.0 (https://github.com/smdn/Smdn.Reflection.ReverseGenerating)
