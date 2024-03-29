// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

partial class TapoClientTests {
  [Test]
  public async Task AuthenticateAsync_ArgumentNull()
  {
    const string Token = "token";

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => Token,
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    Assert.That(client.Session, Is.Null);

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

    Assert.That(client.Session, Is.Null);
  }

  [Test]
  public async Task AuthenticateAsync_InvalidProtocol()
  {
    const TapoSessionProtocol InvalidProtocol = (TapoSessionProtocol)(-9999);
    const string Token = "token";

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => Token,
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

#pragma warning disable CA2012
    Assert.Throws<ArgumentException>(
      () => client.AuthenticateAsync(
        protocol: InvalidProtocol,
        identity: null,
        credential: defaultCredentialProvider!
      )
    );
#pragma warning restore CA2012

    Assert.That(client.Session, Is.Null);
  }

  [Test]
  public async Task AuthenticateAsync()
  {
    const string Token = "token";

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => Token,
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

    Assert.That(client.Session, Is.Not.Null);
    Assert.That(client.Session!.Token, Is.Not.Null);
    Assert.That(client.Session.Token, Is.EqualTo(Token));
    Assert.That(client.Session!.SessionId, Is.Not.Null);
    Assert.That(client.Session.SessionId, Is.Not.Empty);
    Assert.That(client.Session.ExpiresOn, Is.Not.EqualTo(DateTime.MaxValue));
    Assert.That(client.Session.HasExpired, Is.False);
    Assert.That(client.Session.Protocol, Is.EqualTo(TapoSessionProtocol.SecurePassThrough));
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

    Assert.That(client.Session, Is.Null);
    Assert.That(device.EndPointUri, Is.EqualTo(ex!.EndPoint));
  }

  [Test]
  public async Task AuthenticateAsync_DnsEndPointWithUnspecifiedAddressType()
  {
    const string Token = "token";

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => Token,
    };

    device.Start();

    using var client = new TapoClient(
      endPoint: new DnsEndPoint(device.EndPoint!.Address.ToString(), device.EndPoint!.Port, AddressFamily.Unspecified)
    );

    Assert.That(client.EndPointUri, Is.EqualTo(device.EndPointUri), nameof(client.EndPointUri));

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
    );

    Assert.That(client.Session, Is.Not.Null);
    Assert.That(client.Session!.Token, Is.Not.Null);
    Assert.That(client.Session.Token, Is.EqualTo(Token));
    Assert.That(client.Session!.SessionId, Is.Not.Null);
    Assert.That(client.Session.SessionId, Is.Not.Empty);
    Assert.That(client.Session.ExpiresOn, Is.Not.EqualTo(DateTime.MaxValue));
    Assert.That(client.Session.HasExpired, Is.False);
    Assert.That(client.Session.Protocol, Is.EqualTo(TapoSessionProtocol.SecurePassThrough));
  }

  private class TapoCredentialNotFoundException : Exception { }

  private class TapoMultipleCredentialProvider : ITapoCredentialProvider {
    private readonly Dictionary<ITapoCredentialIdentity, TapoCredential?> credentials = new();

    public void AddCredential(ITapoCredentialIdentity identity, TapoCredential? credentialForIdentity)
      => credentials[identity] = credentialForIdentity;

    private TapoCredential? GetTapoCredential(ITapoCredentialIdentity? identity)
    {
      if (identity is null)
        throw new InvalidOperationException("identity must be specified");
      if (!credentials.TryGetValue(identity, out var credential))
        throw new TapoCredentialNotFoundException();

      return credential!;
    }

    public ITapoCredential GetCredential(ITapoCredentialIdentity? identity)
      => GetTapoCredential(identity)!; // return null as is even if null.

    public ITapoKlapCredential GetKlapCredential(ITapoCredentialIdentity? identity)
      => GetTapoCredential(identity)!; // return null as is even if null.
  }

  private class TapoCredential : ITapoCredentialIdentity, ITapoCredential, ITapoKlapCredential {
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

    public void WriteLocalAuthHash(Span<byte> destination)
      => _ = TapoCredentials.TryComputeKlapLocalAuthHash(
        Encoding.ASCII.GetBytes(Username),
        Encoding.ASCII.GetBytes(Password),
        destination,
        out _
      );
  }

  private static System.Collections.IEnumerable YieldTestCases_AuthenticateAsync_IdentifyCredentialForIdentity()
  {
    var credentialProvider = new TapoMultipleCredentialProvider();
    var user1 = new TapoCredential("user1", "pass1");
    var user2 = new TapoCredential("user2", "pass2");
    var user3 = new TapoCredential("user3", "pass3");
    var userNotRegisteredInCredentialProvider = new TapoCredential("user3", "pass3");

    credentialProvider.AddCredential(user1, user1);
    credentialProvider.AddCredential(user2, user2);
    credentialProvider.AddCredential(user3, null);

    yield return new object?[] { credentialProvider, user1, user1, null };
    yield return new object?[] { credentialProvider, user2, user2, null };
    yield return new object?[] { credentialProvider, user3, user3, typeof(InvalidOperationException) };
    yield return new object?[] { credentialProvider, userNotRegisteredInCredentialProvider, userNotRegisteredInCredentialProvider, typeof(TapoCredentialNotFoundException) };
  }

  [TestCaseSource(nameof(YieldTestCases_AuthenticateAsync_IdentifyCredentialForIdentity))]
  public async Task AuthenticateAsync_IdentifyCredentialForIdentity(
    ITapoCredentialProvider credentialProvider,
    ITapoCredentialIdentity identity,
    ITapoKlapCredential _,
    Type? typeOfExpectedException
  )
  {
    await using var device = new PseudoTapoDevice() {
      FuncGenerateLoginDeviceResponse = (_, param) => {
        var username = param.GetProperty("username").GetString();

        Assert.That(username, Is.Not.Null, nameof(username));

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

    await Assert.ThatAsync(
      async () => await client.AuthenticateAsync(
        identity: identity,
        credential: credentialProvider
      ),
      typeOfExpectedException is null ? Throws.Nothing : Throws.TypeOf(typeOfExpectedException)
    );

    if (typeOfExpectedException is null) {
      Assert.That(client.Session, Is.Not.Null);
      Assert.That(client.Session!.Token, Is.EqualTo((identity as TapoCredential)!.Username));
    }
    else {
      Assert.That(client.Session, Is.Null);
    }
  }

  [Test]
  public async Task AuthenticateAsync_Handshake_ErrorResponse()
  {
    const int HandshakeErrorCode = -9999;

    await using var device = new PseudoTapoDevice() {
      FuncGenerateHandshakeResponse = static (_, _) => new HandshakeResponse() {
        ErrorCode = HandshakeErrorCode,
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

    Assert.That(ex!.InnerException, Is.InstanceOf<TapoErrorResponseException>(), nameof(ex.InnerException));

    var innerErrorResponseException = (TapoErrorResponseException)ex.InnerException!;

    Assert.That(innerErrorResponseException.EndPoint, Is.EqualTo(new Uri(device.EndPointUri!, "/app")), nameof(innerErrorResponseException.EndPoint));
    Assert.That(innerErrorResponseException.RequestMethod, Is.EqualTo("handshake"), nameof(innerErrorResponseException.RequestMethod));
    Assert.That(innerErrorResponseException.RawErrorCode, Is.EqualTo(HandshakeErrorCode), nameof(innerErrorResponseException.RawErrorCode));

    Assert.That(client.Session, Is.Null);
    Assert.That(device.EndPointUri, Is.EqualTo(ex!.EndPoint));
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

    Assert.That(client.Session, Is.Null);
    Assert.That(device.EndPointUri, Is.EqualTo(ex!.EndPoint));
  }

  [Test]
  public async Task AuthenticateAsync_Handshake_UnexpectedLengthOfExchangedKey(
    [Values(64, 256)] int keyLength
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

    Assert.That(client.Session, Is.Null);
    Assert.That(device.EndPointUri, Is.EqualTo(ex!.EndPoint));
  }

  [TestCase("")]
  [TestCase("TP_UNKNOWN=XXXX")]
  [TestCase("TP_SESSIONID=")]
  [TestCase("TP_SESSIONID=;TIMEOUT=1440")]
  public async Task AuthenticateAsync_Handshake_TpSessionIdCookie_InvalidOrNotAdvertised(
    string? tpSessionIdCookieValue
  )
  {
    const string Token = "token";

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => Token,
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

    Assert.That(client.Session, Is.Not.Null);
    Assert.That(client.Session!.Token, Is.Not.Null);
    Assert.That(client.Session.Token, Is.EqualTo(Token));
    Assert.That(client.Session!.SessionId, Is.Null);
    Assert.That(client.Session!.ExpiresOn, Is.EqualTo(DateTime.MaxValue));
    Assert.That(client.Session.HasExpired, Is.False);
    Assert.That(client.Session.Protocol, Is.EqualTo(TapoSessionProtocol.SecurePassThrough));
  }

  [TestCase("")]
  [TestCase(";")]
  [TestCase(";TIMEOUT=")]
  [TestCase(";TIMEOUT=-1")]
  public async Task AuthenticateAsync_Handshake_TpSessionIdCookie_TimeoutAttributeInvalidOrNotAdvertised(
    string timeoutAttribute
  )
  {
    const string Token = "token";

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => Token,
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

    Assert.That(client.Session, Is.Not.Null);
    Assert.That(client.Session!.Token, Is.Not.Null);
    Assert.That(client.Session.Token, Is.EqualTo(Token));
    Assert.That(client.Session!.ExpiresOn, Is.EqualTo(DateTime.MaxValue));
    Assert.That(client.Session.HasExpired, Is.False);
    Assert.That(client.Session.Protocol, Is.EqualTo(TapoSessionProtocol.SecurePassThrough));
  }

  [Test]
  public async Task AuthenticateAsync_Handshake_Timeout()
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
      Assert.Ignore("disabled this test case because the test runner process crashes");
      return;
    }

    const string Token = "token";

    using var cts = new CancellationTokenSource();

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => Token,
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

      Assert.That(ex!.InnerException, Is.InstanceOf<TimeoutException>(), nameof(ex.InnerException));

      Assert.That(client.Session, Is.Null);
      Assert.That(device.EndPointUri, Is.EqualTo(ex!.EndPoint));
    }
    finally {
      cts.Cancel();
    }
  }

  private static System.Collections.IEnumerable YieldTestCases_AuthenticateAsync_LoginDevice_CredentialMustBeConverted()
  {
    const string ExpectedUsernamePropertyValue = "MTJkZWE5NmZlYzIwNTkzNTY2YWI3NTY5MmM5OTQ5NTk2ODMzYWRjOQ=="; // "user"
    const string ExpectedPasswordPropertyValue = "cGFzcw=="; // "pass"

    var servicesForCaseOfPlainText = new ServiceCollection();

    servicesForCaseOfPlainText.AddTapoCredential("user", "pass");

    yield return new object[] {
      servicesForCaseOfPlainText.BuildServiceProvider().GetRequiredService<ITapoCredentialProvider>(),
      ExpectedUsernamePropertyValue,
      ExpectedPasswordPropertyValue
    };

    var servicesForCaseOfBase64Encoded = new ServiceCollection();

    servicesForCaseOfBase64Encoded.AddTapoBase64EncodedCredential(ExpectedUsernamePropertyValue, ExpectedPasswordPropertyValue);

    yield return new object[] {
      servicesForCaseOfBase64Encoded.BuildServiceProvider().GetRequiredService<ITapoCredentialProvider>(),
      ExpectedUsernamePropertyValue,
      ExpectedPasswordPropertyValue
    };
  }

  [TestCaseSource(nameof(YieldTestCases_AuthenticateAsync_LoginDevice_CredentialMustBeConverted))]
  public async Task AuthenticateAsync_LoginDevice_CredentialMustBeConverted(
    ITapoCredentialProvider credential,
    string expectedUsernamePropertyValue,
    string expectedPasswordPropertyValue
  )
  {
    const string Token = "token";

    await using var device = new PseudoTapoDevice() {
      FuncGenerateLoginDeviceResponse = (session, param) => {
        Assert.That(param.TryGetProperty("username", out var propUsername), Is.True, $"{nameof(param)} must have 'username' property");
        Assert.That(param.TryGetProperty("password", out var propPassword), Is.True, $"{nameof(param)} must have 'password' property");

        Assert.That(propUsername.GetString(), Is.EqualTo(expectedUsernamePropertyValue), nameof(propUsername));
        Assert.That(propPassword.GetString(), Is.EqualTo(expectedPasswordPropertyValue), nameof(propPassword));

        return new LoginDeviceResponse() {
          ErrorCode = KnownErrorCodes.Success,
          Result = new() { Token = Token },
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

    Assert.That(client.Session, Is.Not.Null);
    Assert.That(client.Session!.Token, Is.Not.Null);
    Assert.That(client.Session.Token, Is.EqualTo(Token));
    Assert.That(client.Session!.SessionId, Is.Not.Null);
    Assert.That(client.Session.SessionId, Is.Not.Empty);
    Assert.That(client.Session.ExpiresOn, Is.Not.EqualTo(DateTime.MaxValue));
    Assert.That(client.Session.HasExpired, Is.False);
    Assert.That(client.Session.Protocol, Is.EqualTo(TapoSessionProtocol.SecurePassThrough));
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

    public ITapoKlapCredential GetKlapCredential(ITapoCredentialIdentity? identity)
      => throw new NotImplementedException();
  }

  private static System.Collections.IEnumerable YieldTestCases_AuthenticateAsync_LoginDevice_CredentialMustBeDisposedAfterRequestWritten()
  {
    yield return new object?[] { null };
    yield return new object?[] { NullLogger.Instance };
  }

  [TestCaseSource(nameof(YieldTestCases_AuthenticateAsync_LoginDevice_CredentialMustBeDisposedAfterRequestWritten))]
  public async Task AuthenticateAsync_LoginDevice_CredentialMustBeDisposedAfterRequestWritten(ILogger? logger)
  {
    const string Username = "user";
    const string Password = "pass";
    const string ExpectedUsernamePropertyValue = "MTJkZWE5NmZlYzIwNTkzNTY2YWI3NTY5MmM5OTQ5NTk2ODMzYWRjOQ=="; // "user"
    const string ExpectedPasswordPropertyValue = "cGFzcw=="; // "pass"
    const string Token = "token";

    var credential = new DisposableCredential(Username, Password);
    var credentialProvider = new DisposableCredentialProvider(credential);

    await using var device = new PseudoTapoDevice() {
      FuncGenerateLoginDeviceResponse = (session, param) => {
        Assert.That(credential.IsDisposed, Is.True, "ITapoCredential.Dispose must be called up to this point");

        Assert.That(param.TryGetProperty("username", out var propUsername), Is.True, $"{nameof(param)} must have 'username' property");
        Assert.That(param.TryGetProperty("password", out var propPassword), Is.True, $"{nameof(param)} must have 'password' property");

        Assert.That(propUsername.GetString(), Is.EqualTo(ExpectedUsernamePropertyValue), nameof(propUsername));
        Assert.That(propPassword.GetString(), Is.EqualTo(ExpectedPasswordPropertyValue), nameof(propPassword));

        return new LoginDeviceResponse() {
          ErrorCode = KnownErrorCodes.Minus1501,
          Result = new() { Token = Token },
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

    Assert.That(credential.IsDisposed, Is.True, "ITapoCredential.Dispose must be called");
  }

  [Test]
  public async Task AuthenticateAsync_LoginDevice_ErrorResponse()
  {
    const int LoginDeviceErrorCode = -9999;

    await using var device = new PseudoTapoDevice() {
      FuncGenerateLoginDeviceResponse = static (_, _) => new LoginDeviceResponse() {
        ErrorCode = LoginDeviceErrorCode,
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

    Assert.That(ex!.InnerException, Is.InstanceOf<TapoErrorResponseException>(), nameof(ex.InnerException));

    var innerErrorResponseException = (TapoErrorResponseException)ex.InnerException!;

    Assert.That(innerErrorResponseException.EndPoint, Is.EqualTo(new Uri(device.EndPointUri!, "/app")), nameof(innerErrorResponseException.EndPoint));
    Assert.That(innerErrorResponseException.RequestMethod, Is.EqualTo("login_device"), nameof(innerErrorResponseException.RequestMethod));
    Assert.That(innerErrorResponseException.RawErrorCode, Is.EqualTo(LoginDeviceErrorCode), nameof(innerErrorResponseException.RawErrorCode));

    Assert.That(client.Session, Is.Null);
    Assert.That(device.EndPointUri, Is.EqualTo(ex!.EndPoint));
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

    Assert.That(ex!.Message, Does.Match("(C|c)redential.*(I|i)nvalid"), nameof(ex.Message));

    Assert.That(ex!.InnerException, Is.InstanceOf<TapoErrorResponseException>(), nameof(ex.InnerException));

    var innerErrorResponseException = (TapoErrorResponseException)ex.InnerException!;

    Assert.That(innerErrorResponseException.EndPoint, Is.EqualTo(new Uri(device.EndPointUri!, "/app")), nameof(innerErrorResponseException.EndPoint));
    Assert.That(innerErrorResponseException.RequestMethod, Is.EqualTo("login_device"), nameof(innerErrorResponseException.RequestMethod));
    Assert.That(innerErrorResponseException.RawErrorCode, Is.EqualTo(KnownErrorCodes.Minus1501), nameof(innerErrorResponseException.RawErrorCode));

    Assert.That(client.Session, Is.Null);
    Assert.That(device.EndPointUri, Is.EqualTo(ex!.EndPoint));
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

    Assert.That(client.Session, Is.Null);
    Assert.That(device.EndPointUri, Is.EqualTo(ex!.EndPoint));
  }

  [Test]
  public async Task AuthenticateAsync_LoginDevice_Timeout()
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
      Assert.Ignore("disabled this test case because the test runner process crashes");
      return;
    }

    const string Token = "token";

    var cts = new CancellationTokenSource();

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => Token,
      FuncGenerateLoginDeviceResponse = (_, _) => {
        // perform latency
        DelayUtils.Delay(TimeSpan.FromSeconds(5), cts.Token);

        return new LoginDeviceResponse() {
          ErrorCode = KnownErrorCodes.Success,
          Result = new LoginDeviceResponse.ResponseResult() {
            Token = Token,
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

      Assert.That(ex!.InnerException, Is.InstanceOf<TimeoutException>(), nameof(ex.InnerException));

      Assert.That(client.Session, Is.Null);
      Assert.That(device.EndPointUri, Is.EqualTo(ex!.EndPoint));
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
      Func<TState, Exception?, string> formatter
    )
    {
      logs.Add(formatter(state, exception));
    }
  }

  [Test]
  public async Task AuthenticateAsync_LoginDevice_MaskedCredentialMustBeOutputToLogger()
  {
    const string Token = "token";

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => Token,
    };

    var endPoint = device.Start();
    var logger = new Logger();

    const string Username = "<username>";
    const string Password = "<password>";

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

    Assert.That(logger.Logs.Any(static line => line.Contains("\"username\":\"****\"")), Is.True, "contains masked username");
    Assert.That(logger.Logs.Any(static line => line.Contains("\"password\":\"****\"")), Is.True, "contains masked password");

    Assert.That(
      logger.Logs.All(static line => !line.Contains(Username)),
      Is.True,
      "does not contain unmasked username");
    Assert.That(
      logger.Logs.All(static line => !line.Contains(Password)),
      Is.True,
      "does not contain unmasked password"
    );

    var encodedUsername = TapoCredentials.ToBase64EncodedSHA1DigestString(Username);
    var encodedPassword = TapoCredentials.ToBase64EncodedString(Password);

    Assert.That(
      logger.Logs.All(line => !line.Contains(encodedUsername)),
      Is.True,
      "does not contain unmasked-encoded username");
    Assert.That(
      logger.Logs.All(line => !line.Contains(encodedPassword)),
      Is.True,
      "does not contain unmasked-encoded password"
    );
  }
}
