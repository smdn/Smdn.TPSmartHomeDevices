// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

partial class TapoClientTests {
  [Test]
  public async Task AuthenticateAsync()
  {
    const string token = "token";

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => token,
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      credentialProvider: defaultCredentialProvider
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync()
    );

    Assert.IsNotNull(client.Session);
    Assert.IsNotNull(client.Session!.Token);
    Assert.AreEqual(token, client.Session.Token);
    Assert.IsNotNull(client.Session!.SessionId);
    Assert.IsNotEmpty(client.Session.SessionId);
    Assert.AreNotEqual(DateTime.MaxValue, client.Session.ExpiresOn);
    Assert.IsFalse(client.Session.HasExpired);
    Assert.AreEqual(
      new Uri($"/app?token={token}", UriKind.Relative),
      client.Session.RequestPathAndQuery
    );
  }

  [Test]
  public async Task AuthenticateAsync_AccessTokenNotIssued()
  {
    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => null,
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      credentialProvider: defaultCredentialProvider
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync()
    );

    Assert.IsNull(client.Session);
    Assert.AreEqual(ex!.EndPoint, device.EndPointUri);
  }

  [Test]
  public async Task AuthenticateAsync_Handshake_ErrorResponse()
  {
    const ErrorCode handshakeErrorCode = (ErrorCode)(-9999);

    await using var device = new PseudoTapoDevice() {
      FuncGenerateHandshakeResponse = static (_, _) => new HandshakeResponse() {
        ErrorCode = handshakeErrorCode,
        Result = new HandshakeResponse.ResponseResult(Key: null)
      },
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      credentialProvider: defaultCredentialProvider
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync()
    );

    Assert.IsInstanceOf<TapoErrorResponseException>(ex!.InnerException, nameof(ex.InnerException));

    var innerErrorResponseException = (TapoErrorResponseException)ex.InnerException!;

    Assert.AreEqual(new Uri(device.EndPointUri!, "/app"), innerErrorResponseException.EndPoint, nameof(innerErrorResponseException.EndPoint));
    Assert.AreEqual("handshake", innerErrorResponseException.RequestMethod, nameof(innerErrorResponseException.RequestMethod));
    Assert.AreEqual(handshakeErrorCode, innerErrorResponseException.ErrorCode, nameof(innerErrorResponseException.ErrorCode));

    Assert.IsNull(client.Session);
    Assert.AreEqual(ex!.EndPoint, device.EndPointUri);
  }

  [Test]
  public async Task AuthenticateAsync_Handshake_SuccessResponseWithoutKey()
  {
    await using var device = new PseudoTapoDevice() {
      FuncGenerateHandshakeResponse = static (_, _) => new HandshakeResponse() {
        ErrorCode = ErrorCode.Success,
        Result = new HandshakeResponse.ResponseResult(Key: null)
      },
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      credentialProvider: defaultCredentialProvider
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync()
    );

    Assert.IsNull(client.Session);
    Assert.AreEqual(ex!.EndPoint, device.EndPointUri);
  }

  [Test]
  public async Task AuthenticateAsync_Handshake_UnexpectedLengthOfExchangedKey(
    [Values(64, 256)]int keyLength
  )
  {
    await using var device = new PseudoTapoDevice() {
      FuncGenerateHandshakeResponse = (_, _) => new HandshakeResponse() {
        ErrorCode = ErrorCode.Success,
        Result = new HandshakeResponse.ResponseResult(Key: Convert.ToBase64String(RandomNumberGenerator.GetBytes(keyLength)))
      },
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      credentialProvider: defaultCredentialProvider
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync()
    );

    Assert.IsNull(client.Session);
    Assert.AreEqual(ex!.EndPoint, device.EndPointUri);
  }

  [TestCase("")]
  [TestCase("TP_UNKNOWN=XXXX")]
  [TestCase("TP_SESSIONID=")]
  [TestCase("TP_SESSIONID=;TIMEOUT=1440")]
  public async Task AuthenticateAsync_Handshake_TpSessionIdCookie_InvalidOrNotAdvertised(
    string? tpSessionIdCookieValue
  )
  {
    const string token = "token";

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => token,
      FuncGenerateCookieValue = session => tpSessionIdCookieValue
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      credentialProvider: defaultCredentialProvider
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync()
    );

    Assert.IsNotNull(client.Session);
    Assert.IsNotNull(client.Session!.Token);
    Assert.AreEqual(token, client.Session.Token);
    Assert.IsNull(client.Session!.SessionId);
    Assert.AreEqual(DateTime.MaxValue, client.Session!.ExpiresOn);
    Assert.IsFalse(client.Session.HasExpired);
  }

  [TestCase("")]
  [TestCase(";")]
  [TestCase(";TIMEOUT=")]
  [TestCase(";TIMEOUT=-1")]
  public async Task AuthenticateAsync_Handshake_TpSessionIdCookie_TimeoutAttributeInvalidOrNotAdvertised(
    string timeoutAttribute
  )
  {
    const string token = "token";

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => token,
      FuncGenerateCookieValue = session => $"TP_SESSIONID={session.SessionId}{timeoutAttribute}"
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      credentialProvider: defaultCredentialProvider
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync()
    );

    Assert.IsNotNull(client.Session);
    Assert.IsNotNull(client.Session!.Token);
    Assert.AreEqual(token, client.Session.Token);
    Assert.AreEqual(DateTime.MaxValue, client.Session!.ExpiresOn);
    Assert.IsFalse(client.Session.HasExpired);
  }

  [Test]
  public async Task AuthenticateAsync_Handshake_Timeout()
  {
    const string token = "token";

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => token,
      FuncGenerateHandshakeResponse = static (_, _) => {
        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5)); // perform latency

        return new HandshakeResponse() {
          ErrorCode = (ErrorCode)9999,
          Result = new HandshakeResponse.ResponseResult(
            Key: null
          )
        };
      },
    };

    var services = new ServiceCollection();

    services.AddTapoHttpClient(
      configureClient: static client => client.Timeout = TimeSpan.FromMilliseconds(1)
    );

    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      credentialProvider: defaultCredentialProvider,
      httpClientFactory: services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>()
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync()
    );

    Assert.IsInstanceOf<TimeoutException>(ex!.InnerException, nameof(ex.InnerException));

    Assert.IsNull(client.Session);
    Assert.AreEqual(ex!.EndPoint, device.EndPointUri);
  }

  [Test]
  public async Task AuthenticateAsync_LoginDevice_ErrorResponse()
  {
    const ErrorCode loginDeviceErrorCode = (ErrorCode)(-9999);

    await using var device = new PseudoTapoDevice() {
      FuncGenerateLoginDeviceResponse = static token => new LoginDeviceResponse() {
        ErrorCode = loginDeviceErrorCode,
        Result = new LoginDeviceResponse.ResponseResult(Token: string.Empty)
      },
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      credentialProvider: defaultCredentialProvider
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync()
    );

    Assert.IsInstanceOf<TapoErrorResponseException>(ex!.InnerException, nameof(ex.InnerException));

    var innerErrorResponseException = (TapoErrorResponseException)ex.InnerException!;

    Assert.AreEqual(new Uri(device.EndPointUri!, "/app"), innerErrorResponseException.EndPoint, nameof(innerErrorResponseException.EndPoint));
    Assert.AreEqual("login_device", innerErrorResponseException.RequestMethod, nameof(innerErrorResponseException.RequestMethod));
    Assert.AreEqual(loginDeviceErrorCode, innerErrorResponseException.ErrorCode, nameof(innerErrorResponseException.ErrorCode));

    Assert.IsNull(client.Session);
    Assert.AreEqual(ex!.EndPoint, device.EndPointUri);
  }

  [Test]
  public async Task AuthenticateAsync_LoginDevice_SuccessResponseWithoutToken()
  {
    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => null,
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      credentialProvider: defaultCredentialProvider
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync()
    );

    Assert.IsNull(client.Session);
    Assert.AreEqual(ex!.EndPoint, device.EndPointUri);
  }

  [Test]
  public async Task AuthenticateAsync_LoginDevice_Timeout()
  {
    const string token = "token";

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => token,
      FuncGenerateLoginDeviceResponse = static _ => {
        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5)); // perform latency

        return new LoginDeviceResponse() {
          ErrorCode = ErrorCode.Success,
          Result = new LoginDeviceResponse.ResponseResult() {
            Token = token,
          }
        };
      },
    };

    var services = new ServiceCollection();

    services.AddTapoHttpClient(
      configureClient: static client => client.Timeout = TimeSpan.FromMilliseconds(1)
    );

    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      credentialProvider: defaultCredentialProvider,
      httpClientFactory: services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>()
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync()
    );

    Assert.IsInstanceOf<TimeoutException>(ex!.InnerException, nameof(ex.InnerException));

    Assert.IsNull(client.Session);
    Assert.AreEqual(ex!.EndPoint, device.EndPointUri);
  }
}
