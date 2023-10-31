// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Smdn.TPSmartHomeDevices.Tapo.Credentials;

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

#pragma warning disable CA2012
    Assert.Throws<ArgumentNullException>(
      () => client.AuthenticateAsync(
        identity: null,
        credential: null!
      )
    );
#pragma warning restore CA2012
    Assert.ThrowsAsync<ArgumentNullException>(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: null!
      )
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
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
    );

    Assert.IsNotNull(client.Session);
    Assert.IsNotNull(client.Session!.Token);
    Assert.AreEqual(token, client.Session.Token);
    Assert.IsNotNull(client.Session!.SessionId);
    Assert.IsNotEmpty(client.Session.SessionId);
    Assert.AreNotEqual(DateTime.MaxValue, client.Session.ExpiresOn);
    Assert.IsFalse(client.Session.HasExpired);
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
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
    );

    Assert.IsNull(client.Session);
    Assert.AreEqual(ex!.EndPoint, device.EndPointUri);
  }

  [Test]
  public async Task AuthenticateAsync_DnsEndPointWithUnspecifiedAddressType()
  {
    const string token = "token";

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => token,
    };

    device.Start();

    using var client = new TapoClient(
      endPoint: new DnsEndPoint(device.EndPoint!.Address.ToString(), device.EndPoint!.Port, AddressFamily.Unspecified)
    );

    Assert.AreEqual(device.EndPointUri, client.EndPointUri, nameof(client.EndPointUri));

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
    );

    Assert.IsNotNull(client.Session);
    Assert.IsNotNull(client.Session!.Token);
    Assert.AreEqual(token, client.Session.Token);
    Assert.IsNotNull(client.Session!.SessionId);
    Assert.IsNotEmpty(client.Session.SessionId);
    Assert.AreNotEqual(DateTime.MaxValue, client.Session.ExpiresOn);
    Assert.IsFalse(client.Session.HasExpired);
  }

  private class TapoCredentialNotFoundException : Exception { }

  private class TapoMultipleCredentialProvider : ITapoCredentialProvider {
    private readonly Dictionary<ITapoCredentialIdentity, ITapoCredential> credentials = new();

    public void AddCredential(ITapoCredentialIdentity identity, ITapoCredential credentialForIdentity)
      => credentials[identity] = credentialForIdentity;

    public ITapoCredential GetCredential(ITapoCredentialIdentity? identity)
    {
      if (identity is null)
        throw new InvalidOperationException("identity must be specified");
      if (!credentials.TryGetValue(identity, out var credential))
        throw new TapoCredentialNotFoundException();

      return credential;
    }
  }

  private class TapoCredential : ITapoCredentialIdentity, ITapoCredential {
    public string Name => $"{nameof(TapoCredential)}:{Username}";
    public string Username { get; }
    public string Password { get; }

    public TapoCredential(string username, string password)
    {
      Username = username;
      Password = password;
    }

    public void Dispose() { } // do nothing

    public void WriteUsernamePropertyValue(Utf8JsonWriter writer)
      => writer.WriteStringValue(Username); // write non-encoded string

    public void WritePasswordPropertyValue(Utf8JsonWriter writer)
      => writer.WriteStringValue(Password); // write non-encoded string
  }

  [Test]
  public async Task AuthenticateAsync_IdentifyCredentialForIdentity()
  {
    var credentialProvider = new TapoMultipleCredentialProvider();
    var user1 = new TapoCredential("user1", "pass1");
    var user2 = new TapoCredential("user2", "pass2");
    var userNotRegisteredInCredentialProvider = new TapoCredential("user3", "pass3");

    credentialProvider.AddCredential(user1, user1);
    credentialProvider.AddCredential(user2, user2);

    await using var device = new PseudoTapoDevice() {
      FuncGenerateLoginDeviceResponse = (_, param) => {
        var username = param.GetProperty("username").GetString();

        Assert.IsNotNull(username, nameof(username));

        return new LoginDeviceResponse() {
          ErrorCode = KnownErrorCodes.Success,
          Result = new LoginDeviceResponse.ResponseResult() {
            Token = username!, // return login username as token
          },
        };
      },
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(
        identity: user1,
        credential: credentialProvider
      ),
      $"select identity {nameof(user1)}"
    );

    Assert.IsNotNull(client.Session);
    Assert.AreEqual(user1.Username, client.Session!.Token);

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(
        identity: user2,
        credential: credentialProvider
      ),
      $"select identity {nameof(user1)}"
    );

    Assert.IsNotNull(client.Session);
    Assert.AreEqual(user2.Username, client.Session.Token);

    Assert.ThrowsAsync<TapoCredentialNotFoundException>(
      async () => await client.AuthenticateAsync(
        identity: userNotRegisteredInCredentialProvider,
        credential: credentialProvider
      ),
      $"cannot select appropriate identity"
    );
  }

  [Test]
  public async Task AuthenticateAsync_Handshake_ErrorResponse()
  {
    const int handshakeErrorCode = -9999;

    await using var device = new PseudoTapoDevice() {
      FuncGenerateHandshakeResponse = static (_, _) => new HandshakeResponse() {
        ErrorCode = handshakeErrorCode,
        Result = new() { Key = null },
      },
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
    );

    Assert.IsInstanceOf<TapoErrorResponseException>(ex!.InnerException, nameof(ex.InnerException));

    var innerErrorResponseException = (TapoErrorResponseException)ex.InnerException!;

    Assert.AreEqual(new Uri(device.EndPointUri!, "/app"), innerErrorResponseException.EndPoint, nameof(innerErrorResponseException.EndPoint));
    Assert.AreEqual("handshake", innerErrorResponseException.RequestMethod, nameof(innerErrorResponseException.RequestMethod));
    Assert.AreEqual(handshakeErrorCode, innerErrorResponseException.RawErrorCode, nameof(innerErrorResponseException.RawErrorCode));

    Assert.IsNull(client.Session);
    Assert.AreEqual(ex!.EndPoint, device.EndPointUri);
  }

  [Test]
  public async Task AuthenticateAsync_Handshake_SuccessResponseWithoutKey()
  {
    await using var device = new PseudoTapoDevice() {
      FuncGenerateHandshakeResponse = static (_, _) => new HandshakeResponse() {
        ErrorCode = KnownErrorCodes.Success,
        Result = new() { Key = null },
      },
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
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
        ErrorCode = KnownErrorCodes.Success,
        Result = new() { Key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(keyLength)) },
      },
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
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
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
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
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
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
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
      Assert.Ignore("disabled this test case because the test runner process crashes");
      return;
    }

    const string token = "token";

    using var cts = new CancellationTokenSource();

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => token,
      FuncGenerateHandshakeResponse = (_, _) => {
        // perform latency
        DelayUtils.Delay(TimeSpan.FromSeconds(5), cts.Token);

        return new HandshakeResponse() {
          ErrorCode = 9999,
          Result = new() { Key = null },
        };
      },
    };

    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    client.Timeout = TimeSpan.FromMilliseconds(1);

    try {
      var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
        async () => await client.AuthenticateAsync(
          identity: null,
          credential: defaultCredentialProvider!
        )
      );

      Assert.IsInstanceOf<TimeoutException>(ex!.InnerException, nameof(ex.InnerException));

      Assert.IsNull(client.Session);
      Assert.AreEqual(ex!.EndPoint, device.EndPointUri);
    }
    finally {
      cts.Cancel();
    }
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
          ErrorCode = KnownErrorCodes.Success,
          Result = new() { Token = token },
        };
      },
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: credential
      )
    );

    Assert.IsNotNull(client.Session);
    Assert.IsNotNull(client.Session!.Token);
    Assert.AreEqual(token, client.Session.Token);
    Assert.IsNotNull(client.Session!.SessionId);
    Assert.IsNotEmpty(client.Session.SessionId);
    Assert.AreNotEqual(DateTime.MaxValue, client.Session.ExpiresOn);
    Assert.IsFalse(client.Session.HasExpired);
  }

  private class DisposableCredential : ITapoCredential {
    private string username;
    private string password;

    public bool IsDisposed => username is null && password is null;

    public DisposableCredential(string username, string password)
    {
      this.username = username;
      this.password = password;
    }

    public void WritePasswordPropertyValue(Utf8JsonWriter writer)
    {
      if (IsDisposed)
        throw new InvalidOperationException();

      writer.WriteStringValue(TapoCredentials.ToBase64EncodedString(password));
    }

    public void WriteUsernamePropertyValue(Utf8JsonWriter writer)
    {
      if (IsDisposed)
        throw new InvalidOperationException();

      writer.WriteStringValue(TapoCredentials.ToBase64EncodedSHA1DigestString(username));
    }

    public void Dispose()
    {
      username = null!;
      password = null!;
    }
  }

  private class DisposableCredentialProvider : ITapoCredentialProvider {
    private readonly ITapoCredential credential;

    public DisposableCredentialProvider(DisposableCredential credential)
    {
      this.credential = credential;
    }

    public ITapoCredential GetCredential(ITapoCredentialIdentity? identity)
      => credential;
  }

  private static System.Collections.IEnumerable YieldTestCases_AuthenticateAsync_LoginDevice_CredentialMustBeDisposedAfterRequestWritten()
  {
    yield return new object?[] { null };
    yield return new object?[] { NullLogger.Instance };
  }

  [TestCaseSource(nameof(YieldTestCases_AuthenticateAsync_LoginDevice_CredentialMustBeDisposedAfterRequestWritten))]
  public async Task AuthenticateAsync_LoginDevice_CredentialMustBeDisposedAfterRequestWritten(ILogger? logger)
  {
    const string username = "user";
    const string password = "pass";
    const string expectedUsernamePropertyValue = "MTJkZWE5NmZlYzIwNTkzNTY2YWI3NTY5MmM5OTQ5NTk2ODMzYWRjOQ=="; // "user"
    const string expectedPasswordPropertyValue = "cGFzcw=="; // "pass"
    const string token = "token";

    var credential = new DisposableCredential(username, password);
    var credentialProvider = new DisposableCredentialProvider(credential);

    await using var device = new PseudoTapoDevice() {
      FuncGenerateLoginDeviceResponse = (session, param) => {
        Assert.IsTrue(credential.IsDisposed, "ITapoCredential.Dispose must be called up to this point");

        Assert.IsTrue(param.TryGetProperty("username", out var propUsername), $"{nameof(param)} must have 'username' property");
        Assert.IsTrue(param.TryGetProperty("password", out var propPassword), $"{nameof(param)} must have 'password' property");

        Assert.AreEqual(expectedUsernamePropertyValue, propUsername.GetString(), nameof(propUsername));
        Assert.AreEqual(expectedPasswordPropertyValue, propPassword.GetString(), nameof(propPassword));

        return new LoginDeviceResponse() {
          ErrorCode = KnownErrorCodes.Minus1501,
          Result = new() { Token = token },
        };
      },
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      logger: logger // ILogger should not affect the operation
    );

    Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: credentialProvider
      )
    );

    Assert.IsTrue(credential.IsDisposed, "ITapoCredential.Dispose must be called");
  }

  [Test]
  public async Task AuthenticateAsync_LoginDevice_ErrorResponse()
  {
    const int loginDeviceErrorCode = -9999;

    await using var device = new PseudoTapoDevice() {
      FuncGenerateLoginDeviceResponse = static (_, _) => new LoginDeviceResponse() {
        ErrorCode = loginDeviceErrorCode,
        Result = new() { Token = string.Empty },
      },
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
    );

    Assert.IsInstanceOf<TapoErrorResponseException>(ex!.InnerException, nameof(ex.InnerException));

    var innerErrorResponseException = (TapoErrorResponseException)ex.InnerException!;

    Assert.AreEqual(new Uri(device.EndPointUri!, "/app"), innerErrorResponseException.EndPoint, nameof(innerErrorResponseException.EndPoint));
    Assert.AreEqual("login_device", innerErrorResponseException.RequestMethod, nameof(innerErrorResponseException.RequestMethod));
    Assert.AreEqual(loginDeviceErrorCode, innerErrorResponseException.RawErrorCode, nameof(innerErrorResponseException.RawErrorCode));

    Assert.IsNull(client.Session);
    Assert.AreEqual(ex!.EndPoint, device.EndPointUri);
  }

  [Test]
  public async Task AuthenticateAsync_LoginDevice_ResponseWithErrorCodeMinus1501()
  {
    await using var device = new PseudoTapoDevice() {
      FuncGenerateLoginDeviceResponse = static (_, _) => new LoginDeviceResponse() {
        ErrorCode = KnownErrorCodes.Minus1501,
        Result = new() { Token = string.Empty },
      },
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
    );

    StringAssert.IsMatch("(C|c)redential.*(I|i)nvalid", ex!.Message, nameof(ex.Message));

    Assert.IsInstanceOf<TapoErrorResponseException>(ex!.InnerException, nameof(ex.InnerException));

    var innerErrorResponseException = (TapoErrorResponseException)ex.InnerException!;

    Assert.AreEqual(new Uri(device.EndPointUri!, "/app"), innerErrorResponseException.EndPoint, nameof(innerErrorResponseException.EndPoint));
    Assert.AreEqual("login_device", innerErrorResponseException.RequestMethod, nameof(innerErrorResponseException.RequestMethod));
    Assert.AreEqual(KnownErrorCodes.Minus1501, innerErrorResponseException.RawErrorCode, nameof(innerErrorResponseException.RawErrorCode));

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
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
    );

    Assert.IsNull(client.Session);
    Assert.AreEqual(ex!.EndPoint, device.EndPointUri);
  }

  [Test]
  public async Task AuthenticateAsync_LoginDevice_Timeout()
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
      Assert.Ignore("disabled this test case because the test runner process crashes");
      return;
    }

    const string token = "token";

    var cts = new CancellationTokenSource();

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => token,
      FuncGenerateLoginDeviceResponse = (_, _) => {
        // perform latency
        DelayUtils.Delay(TimeSpan.FromSeconds(5), cts.Token);

        return new LoginDeviceResponse() {
          ErrorCode = KnownErrorCodes.Success,
          Result = new LoginDeviceResponse.ResponseResult() {
            Token = token,
          }
        };
      },
    };

    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    client.Timeout = TimeSpan.FromMilliseconds(1);

    try {
      var ex = Assert.ThrowsAsync<TapoAuthenticationException>(
        async () => await client.AuthenticateAsync(
          identity: null,
          credential: defaultCredentialProvider!
        )
      );

      Assert.IsInstanceOf<TimeoutException>(ex!.InnerException, nameof(ex.InnerException));

      Assert.IsNull(client.Session);
      Assert.AreEqual(ex!.EndPoint, device.EndPointUri);
    }
    finally {
      cts.Cancel();
    }
  }

  private class Logger : ILogger {
    private readonly List<string> logs = new();
    public IReadOnlyList<string> Logs => logs;

    public IDisposable BeginScope<TState>(TState state) => NullLoggerScope.Instance;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(
      LogLevel logLevel,
      EventId eventId,
      TState state,
      Exception? exception,
      Func<TState,Exception?,string> formatter
    )
    {
      logs.Add(formatter(state, exception));
    }
  }

  [Test]
  public async Task AuthenticateAsync_LoginDevice_MaskedCredentialMustBeOutputToLogger()
  {
    const string token = "token";

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => token,
    };

    var endPoint = device.Start();
    var logger = new Logger();

    const string username = "<username>";
    const string password = "<password>";

    using var client = new TapoClient(
      endPoint: endPoint,
      logger: logger
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
    );

    Assert.IsTrue(logger.Logs.Any(static line => line.Contains("\"username\":\"****\"")), "contains masked username");
    Assert.IsTrue(logger.Logs.Any(static line => line.Contains("\"password\":\"****\"")), "contains masked password");

    Assert.IsTrue(
      logger.Logs.All(static line => !line.Contains(username)),
      "does not contain unmasked username");
    Assert.IsTrue(
      logger.Logs.All(static line => !line.Contains(password)),
      "does not contain unmasked password"
    );

    var encodedUsername = TapoCredentials.ToBase64EncodedSHA1DigestString(username);
    var encodedPassword = TapoCredentials.ToBase64EncodedString(password);

    Assert.IsTrue(
      logger.Logs.All(line => !line.Contains(encodedUsername)),
      "does not contain unmasked-encoded username");
    Assert.IsTrue(
      logger.Logs.All(line => !line.Contains(encodedPassword)),
      "does not contain unmasked-encoded password"
    );
  }
}
