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
  public async Task AuthenticateAsync_ArgumentNull()
  {
    const string token = "token";

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => token,
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    Assert.IsNull(client.Session);

    Assert.Throws<ArgumentNullException>(
      () => client.AuthenticateAsync(credential: null!)
    );
    Assert.ThrowsAsync<ArgumentNullException>(
      async () => await client.AuthenticateAsync(credential: null!)
    );

    Assert.IsNull(client.Session);
  }

  [Test]
  public async Task AuthenticateAsync()
  {
    const string token = "token";

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => token,
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(credential: defaultCredentialProvider)
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
      endPoint: endPoint
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync(credential: defaultCredentialProvider)
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
      endPoint: endPoint
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync(credential: defaultCredentialProvider)
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
      endPoint: endPoint
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync(credential: defaultCredentialProvider)
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
      endPoint: endPoint
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync(credential: defaultCredentialProvider)
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
      endPoint: endPoint
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(credential: defaultCredentialProvider)
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
      endPoint: endPoint
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(credential: defaultCredentialProvider)
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
      httpClientFactory: services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>()
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync(credential: defaultCredentialProvider)
    );

    Assert.IsInstanceOf<TimeoutException>(ex!.InnerException, nameof(ex.InnerException));

    Assert.IsNull(client.Session);
    Assert.AreEqual(ex!.EndPoint, device.EndPointUri);
  }

  private static System.Collections.IEnumerable YieldTestCases_AuthenticateAsync_LoginDevice_CredentialMustBeConverted()
  {
    const string expectedUsernamePropertyValue = "MTJkZWE5NmZlYzIwNTkzNTY2YWI3NTY5MmM5OTQ5NTk2ODMzYWRjOQ=="; // "user"
    const string expectedPasswordPropertyValue = "cGFzcw=="; // "pass"

    var servicesForCaseOfPlainText = new ServiceCollection();

    servicesForCaseOfPlainText.AddTapoCredential("user", "pass");

    yield return new object[] {
      servicesForCaseOfPlainText.BuildServiceProvider().GetRequiredService<ITapoCredentialProvider>(),
      expectedUsernamePropertyValue,
      expectedPasswordPropertyValue
    };

    var servicesForCaseOfBase64Encoded = new ServiceCollection();

    servicesForCaseOfBase64Encoded.AddTapoBase64EncodedCredential(expectedUsernamePropertyValue, expectedPasswordPropertyValue);

    yield return new object[] {
      servicesForCaseOfBase64Encoded.BuildServiceProvider().GetRequiredService<ITapoCredentialProvider>(),
      expectedUsernamePropertyValue,
      expectedPasswordPropertyValue
    };
  }

  [TestCaseSource(nameof(YieldTestCases_AuthenticateAsync_LoginDevice_CredentialMustBeConverted))]
  public async Task AuthenticateAsync_LoginDevice_CredentialMustBeConverted(
    ITapoCredentialProvider credential,
    string expectedUsernamePropertyValue,
    string expectedPasswordPropertyValue
  )
  {
    const string token = "token";

    await using var device = new PseudoTapoDevice() {
      FuncGenerateLoginDeviceResponse = (session, param) => {
        Assert.IsTrue(param.TryGetProperty("username", out var propUsername), $"{nameof(param)} must have 'username' property");
        Assert.IsTrue(param.TryGetProperty("password", out var propPassword), $"{nameof(param)} must have 'password' property");

        Assert.AreEqual(expectedUsernamePropertyValue, propUsername.GetString(), nameof(propUsername));
        Assert.AreEqual(expectedPasswordPropertyValue, propPassword.GetString(), nameof(propPassword));

        return new LoginDeviceResponse() {
          ErrorCode = ErrorCode.Success,
          Result = new LoginDeviceResponse.ResponseResult(Token: token)
        };
      },
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(credential: credential)
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
  public async Task AuthenticateAsync_LoginDevice_ErrorResponse()
  {
    const ErrorCode loginDeviceErrorCode = (ErrorCode)(-9999);

    await using var device = new PseudoTapoDevice() {
      FuncGenerateLoginDeviceResponse = static (_, _) => new LoginDeviceResponse() {
        ErrorCode = loginDeviceErrorCode,
        Result = new LoginDeviceResponse.ResponseResult(Token: string.Empty)
      },
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync(credential: defaultCredentialProvider)
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
      endPoint: endPoint
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync(credential: defaultCredentialProvider)
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
      FuncGenerateLoginDeviceResponse = static (_, _) => {
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
      httpClientFactory: services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>()
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync(credential: defaultCredentialProvider)
    );

    Assert.IsInstanceOf<TimeoutException>(ex!.InnerException, nameof(ex.InnerException));

    Assert.IsNull(client.Session);
    Assert.AreEqual(ex!.EndPoint, device.EndPointUri);
  }
}
