// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

public sealed class PseudoTapoDevice : IDisposable, IAsyncDisposable {
  public abstract class SessionBase {
    public string SessionId { get; }
    public DateTime ExpiresOn { get; }
    internal ICryptoTransform Encryptor { get; }
    internal ICryptoTransform Decryptor { get; }

    protected SessionBase(
      string sessionId,
      DateTime expiresOn,
      ICryptoTransform encryptor,
      ICryptoTransform decryptor
    )
    {
      SessionId = sessionId;
      ExpiresOn = expiresOn;
      Encryptor = encryptor;
      Decryptor = decryptor;
    }
  }

  private class UnauthorizedSession : SessionBase {
    public static UnauthorizedSession Create(
      IPEndPoint remoteEndPoint,
      string sessionId,
      DateTime expiresOn,
      byte[] key,
      byte[] initialVector
    )
    {
      var aes = Aes.Create();

      aes.Padding = PaddingMode.PKCS7;
      aes.Key = key;
      aes.IV = initialVector;

      return new UnauthorizedSession(
        remoteEndPoint,
        sessionId,
        expiresOn,
        aes.CreateEncryptor(),
        aes.CreateDecryptor()
      );
    }

    public IPEndPoint RemoteEndPoint { get; }

    private UnauthorizedSession(
      IPEndPoint remoteEndPoint,
      string sessionId,
      DateTime expiresOn,
      ICryptoTransform encryptor,
      ICryptoTransform decryptor
    )
      : base(
        sessionId,
        expiresOn,
        encryptor,
        decryptor
      )
    {
      RemoteEndPoint = remoteEndPoint;
    }
  }

  private class AuthorizedSession : SessionBase {
    public string Token { get; }

    public AuthorizedSession(string token, UnauthorizedSession session)
      : base(
        session.SessionId,
        session.ExpiresOn,
        session.Encryptor,
        session.Decryptor
      )
    {
      Token = token;
    }
  }

  private static readonly Encoding utf8nobom = new UTF8Encoding(false);

  public IPEndPoint? EndPoint { get; private set; }
  public Uri? EndPointUri => EndPoint is null ? null : new Uri($"http://{EndPoint.Address}:{EndPoint.Port}");
  private HttpListener? listener;
  private Task? taskProcessListener;

  private readonly ConcurrentDictionary<string, UnauthorizedSession> unauthorizedSessions = new();
  private readonly ConcurrentDictionary<string, AuthorizedSession> authorizedSessions = new();

  public Func<SessionBase, string?>? FuncGenerateToken { get; set; }
  public Func<SessionBase, RSA, HandshakeResponse>? FuncGenerateHandshakeResponse { get; set; }
  public Func<SessionBase, string?>? FuncGenerateCookieValue { get; set; }
  public Func<SessionBase, LoginDeviceResponse>? FuncGenerateLoginDeviceResponse { get; set; }
  public Func<SessionBase, string, JsonElement, (ErrorCode, ITapoPassThroughResponse?)>? FuncGeneratePassThroughResponse { get; set; }

  public PseudoTapoDevice()
  {
    EndPoint = null;
  }

  private void ThrowIfDisposed()
  {
    if (listener is null)
      throw new ObjectDisposedException(GetType().FullName);
  }

  public async ValueTask DisposeAsync()
  {
    try {
      listener?.Stop();
      listener = null;

      if (taskProcessListener is not null) {
        await taskProcessListener.ConfigureAwait(false);
        taskProcessListener.Dispose();
        taskProcessListener = null;
      }
    }
    finally {
      Dispose();
    }
  }

  public void Dispose()
  {
    listener?.Stop();
    listener = null;

    taskProcessListener?.Dispose();
    taskProcessListener = null;
  }

  public EndPoint Start(
    int? exceptPort = 0
  )
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
      if (!EndPointUtils.TryFindUnusedPort(exceptPort, out var port))
        throw new InvalidOperationException("could not find unused port");

      EndPoint = new IPEndPoint(IPAddress.Loopback, port);

      listener = new HttpListener();
      listener.Prefixes.Add($"http://{EndPoint.Address}:{EndPoint.Port}/");

      listener.Start();
    }
    else {
      foreach (var port in EndPointUtils.EnumerateIANASuggestedDynamicPorts(exceptPort)) {
        var l = new HttpListener();

        try {
          var endPoint = new IPEndPoint(IPAddress.Loopback, port);

          l.Prefixes.Add($"http://{endPoint.Address}:{endPoint.Port}/");
          l.Start();

          EndPoint = endPoint;
          listener = l;
          break;
        }
        catch (HttpListenerException) {
          (l as IDisposable).Dispose();
          continue;
        }
      }

      if (listener is null)
        throw new InvalidOperationException("could not find unused port");
    }

    if (!listener.IsListening)
      throw new InvalidOperationException("not listening");

    taskProcessListener = Task.Run(ProcessListenerAsync);

    return EndPoint!;
  }

  public async Task StopAsync()
  {
    ThrowIfDisposed();

    await DisposeAsync();
  }

  public IDeviceEndPointProvider GetEndPointProvider()
    => new StaticDeviceEndPointProvider(EndPoint);

  private async Task ProcessListenerAsync()
  {
    for (;;) {
      try {
        if (listener is null)
          return; // disposed

        var context = await listener!.GetContextAsync().ConfigureAwait(false);

        if (context is not null)
          await ProcessRequestAsync(context).ConfigureAwait(false);
      }
      catch (ObjectDisposedException) {
        return; // expected exception (listener stopped)
      }
    }
  }

  private static Task WriteBadRequestPlainTextContentAsync(
    HttpListenerResponse response,
    string content
  )
    => WritePlainTextContentAsync(
      response,
      HttpStatusCode.BadRequest,
      content
    );

  private static async Task WritePlainTextContentAsync(
    HttpListenerResponse response,
    HttpStatusCode statusCode,
    string content
  )
  {
    const string contentType = "text/plain";

    response.StatusCode = (int)statusCode;
    response.ContentEncoding = utf8nobom;
    response.ContentType = contentType;

    using var buffer = new MemoryStream();
    using var writer = new StreamWriter(buffer, response.ContentEncoding, 1024, leaveOpen: true);

    await writer.WriteLineAsync(content).ConfigureAwait(false);
    await writer.FlushAsync().ConfigureAwait(false);

    response.ContentLength64 = buffer.Length;

    buffer.Position = 0L;

    await buffer.CopyToAsync(response.OutputStream).ConfigureAwait(false);
  }

  private async Task ProcessRequestAsync(HttpListenerContext context)
  {
    try {
      var request = context.Request;

      // validate request method
      if (!string.Equals("POST", request.HttpMethod, StringComparison.Ordinal)) {
        var response = context.Response;

        response.KeepAlive = false;

        await WriteBadRequestPlainTextContentAsync(
          response,
          "invalid request method"
        ).ConfigureAwait(false);

        return;
      }

      // validate request content type
      if (
        !string.Equals("application/json", request.ContentType, StringComparison.OrdinalIgnoreCase) &&
        !string.Equals("text/json", request.ContentType, StringComparison.OrdinalIgnoreCase)
      ) {
        var response = context.Response;

        response.KeepAlive = false;

        await WriteBadRequestPlainTextContentAsync(
          response,
          "invalid content type"
        ).ConfigureAwait(false);

        return;
      }

      await ProcessPostJsonRequestAsync(context).ConfigureAwait(false);
    }
    finally {
      context?.Response?.OutputStream?.Close();
    }
  }

  private async Task ProcessPostJsonRequestAsync(HttpListenerContext context)
  {
    static Task WriteWrongEndPointForMethodResponseContentAsync(
      HttpListenerResponse response,
      string content
    )
      => WritePlainTextContentAsync(
        response,
        HttpStatusCode.NotFound,
        content
      );

    if (!string.Equals("/app", context.Request.Url?.LocalPath, StringComparison.Ordinal)) {
      await WriteWrongEndPointForMethodResponseContentAsync(context.Response, "wrong end point").ConfigureAwait(false);
      return;
    }

    var requestJsonDocument = await JsonDocument.ParseAsync(
      context.Request.InputStream
    ).ConfigureAwait(false);

    if (
      !requestJsonDocument.RootElement.TryGetProperty("method", out var methodProperty) ||
      methodProperty.ValueKind != JsonValueKind.String
    ) {
      await WriteBadRequestPlainTextContentAsync(context.Response, "method not specified or invalid").ConfigureAwait(false);
      return;
    }

    var method = methodProperty.GetString();

    try {
      if (string.Equals("handshake", method, StringComparison.Ordinal)) {
        var session = await ProcessHandshakeMethodRequestAsync(context, requestJsonDocument).ConfigureAwait(false);
        unauthorizedSessions.AddOrUpdate(session.SessionId, session, (key, existing) => session);
        return;
      }

      if (string.Equals("securePassthrough", method, StringComparison.Ordinal)) {
        var (session, passThroughRequestJsonDocument) = await ProcessSecurePassThroughMethodRequestAsync(context, requestJsonDocument).ConfigureAwait(false);

        if (passThroughRequestJsonDocument is null || session is null)
          return;

        if (
          !passThroughRequestJsonDocument.RootElement.TryGetProperty("method", out var passThroughMethodProperty) ||
          passThroughMethodProperty.ValueKind != JsonValueKind.String
        ) {
          await WriteBadRequestPlainTextContentAsync(context.Response, "pass through method not specified or invalid").ConfigureAwait(false);
          return;
        }

        var passThroughMethod = passThroughMethodProperty.GetString();

        switch (passThroughMethod) {
          case "login_device": {
            if (session is UnauthorizedSession unauthorizedSession) {
              var authorizedSession = await ProcessLoginDeviceMethodRequestAsync(
                context,
                unauthorizedSession,
                passThroughRequestJsonDocument
              ).ConfigureAwait(false);

              if (authorizedSession is not null) {
                unauthorizedSessions.TryRemove(unauthorizedSession.SessionId, out _);
                authorizedSessions.AddOrUpdate(authorizedSession.Token, authorizedSession, (key, existing) => authorizedSession);
              }
            }
            else {
              await WriteBadRequestPlainTextContentAsync(context.Response, "session already authorized or invalid").ConfigureAwait(false);
            }
            return;
          }

          default: {
            if (session is AuthorizedSession authorizedSession) {
              await ProcessPassThroughRequestAsync(
                context,
                authorizedSession,
                passThroughMethod,
                passThroughRequestJsonDocument
              ).ConfigureAwait(false);
            }
            else {
              await WriteBadRequestPlainTextContentAsync(context.Response, "session already authorized or invalid").ConfigureAwait(false);
            }
            return;
          }
        }
      }

      await WriteBadRequestPlainTextContentAsync(context.Response, $"method not supported or unknown: '{method}'").ConfigureAwait(false);
      return;
    }
    catch (Exception ex) {
      Console.Error.WriteLine(ex);
    }
  }

  private static async Task WriteJsonContentAsync<TResponseContent>(
    HttpListenerResponse response,
    TResponseContent content,
    HttpStatusCode statusCode = HttpStatusCode.OK
  )
  {
    const string contentType = "application/json";

    response.StatusCode = (int)statusCode;
    response.ContentEncoding = utf8nobom;
    response.ContentType = contentType;

    using var buffer = new MemoryStream();
    using var writer = new StreamWriter(buffer, response.ContentEncoding, 1024, leaveOpen: true);

    await JsonSerializer.SerializeAsync(buffer, content).ConfigureAwait(false);

    response.ContentLength64 = buffer.Length;

    buffer.Position = 0L;

#if false
    Console.WriteLine(new StreamReader(buffer).ReadToEnd());
    buffer.Position = 0L;
#endif

    await buffer.CopyToAsync(response.OutputStream).ConfigureAwait(false);
  }

  private async Task<UnauthorizedSession> ProcessHandshakeMethodRequestAsync(
    HttpListenerContext context,
    JsonDocument requestJsonDocument
  )
  {
    var parameters = JsonSerializer.Deserialize<HandshakeRequest.RequestParameters>(
      requestJsonDocument.RootElement.GetProperty("params")
    );

    var rsa = RSA.Create(keySizeInBits: 1024);

    rsa.ImportFromPem(parameters.Key);

    var keyAndIv = new byte[32];
    var rng = RandomNumberGenerator.Create();

    rng.GetBytes(keyAndIv);

    var encryptedKeyAndIv = rsa.Encrypt(keyAndIv, RSAEncryptionPadding.Pkcs1);
    var sessionId = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
    var sessionExpiresOn = DateTime.Now + TimeSpan.FromDays(1.0);

    var session = UnauthorizedSession.Create(
      remoteEndPoint: context.Request.RemoteEndPoint,
      sessionId: sessionId,
      expiresOn: sessionExpiresOn,
      key: keyAndIv.AsSpan(0, 16).ToArray(),
      initialVector: keyAndIv.AsSpan(16, 16).ToArray()
    );

    var response = FuncGenerateHandshakeResponse?.Invoke(session, rsa)
      ?? new HandshakeResponse() {
        ErrorCode = ErrorCode.Success,
        Result = new HandshakeResponse.ResponseResult(
          Key: Convert.ToBase64String(encryptedKeyAndIv)
        )
      };

    var cookieValue = FuncGenerateCookieValue?.Invoke(session)
      ?? $"TP_SESSIONID={session.SessionId};TIMEOUT={(int)(session.ExpiresOn - DateTime.Now).TotalMinutes}";

    if (!string.IsNullOrEmpty(cookieValue))
      context.Response.AddHeader("Set-Cookie", cookieValue);

    await WriteJsonContentAsync(context.Response, response, HttpStatusCode.OK).ConfigureAwait(false);

    return session;
  }

  private async Task<(SessionBase?, JsonDocument?)> ProcessSecurePassThroughMethodRequestAsync(
    HttpListenerContext context,
    JsonDocument requestJsonDocument
  )
  {
    var token = context.Request.QueryString["token"];

    SessionBase? session = string.IsNullOrEmpty(token)
      ? TapoSessionCookieUtils.TryGetCookie(context.Request.Headers.GetValues("Cookie"), out var sessionId, out _)
        ? unauthorizedSessions.TryGetValue(sessionId, out var unauthorizedSession)
          ? unauthorizedSession
          : null
        : unauthorizedSessions.FirstOrDefault(pair => pair.Value.RemoteEndPoint == context.Request.RemoteEndPoint).Value // find session by requested remote end point
      : authorizedSessions.TryGetValue(token, out var authorizedSession)
        ? authorizedSession
        : null;

    if (session is null) {
      await WriteBadRequestPlainTextContentAsync(context.Response, "session not initiated").ConfigureAwait(false);
      return default;
    }

    var requestElement = requestJsonDocument.RootElement.GetProperty("params").GetProperty("request");

    if (!requestElement.TryGetBytesFromBase64(out var passThroughRequest)) {
      await WriteBadRequestPlainTextContentAsync(context.Response, "invalid pass through request").ConfigureAwait(false);
      return default;
    }

    using var encryptedPassThroughRequestStream = new MemoryStream(passThroughRequest);
    var passThroughRequestStream = new CryptoStream(
      encryptedPassThroughRequestStream,
      session.Decryptor,
      CryptoStreamMode.Read,
      leaveOpen: true
    );

    var passThroughRequestJsonDocument = await JsonDocument.ParseAsync(passThroughRequestStream).ConfigureAwait(false);

    return (session, passThroughRequestJsonDocument);
  }

  private async Task WriteSecurePassThroughJsonContentAsync(
    HttpListenerResponse response,
    SessionBase session,
    ErrorCode errorCode,
    ITapoPassThroughResponse passThroughResponse
  )
  {
    const string contentType = "application/json";

    response.StatusCode = (int)HttpStatusCode.OK;
    response.ContentEncoding = utf8nobom;
    response.ContentType = contentType;

    using var buffer = new MemoryStream();
    var options = new JsonSerializerOptions();

    options.Converters.Add(
      new SecurePassThroughJsonConverterFactory(
        encryptorForPassThroughRequest: session.Encryptor,
        decryptorForPassThroughResponse: null,
        plainTextJsonSerializerOptions: null
      )
    );

    var typeOfSecurePassThroughResponse = typeof(SecurePassThroughResponse<>).MakeGenericType(passThroughResponse.GetType());
    var securePassThroughResponse = Activator.CreateInstance(
      type: typeOfSecurePassThroughResponse,
      bindingAttr: BindingFlags.Instance | BindingFlags.Public,
      binder: null,
      args: new object[] { errorCode, passThroughResponse },
      culture: null
    )!;

    await JsonSerializer.SerializeAsync(
      utf8Json: buffer,
      value: securePassThroughResponse,
      inputType: typeOfSecurePassThroughResponse,
      options: options
    ).ConfigureAwait(false);

    response.ContentLength64 = buffer.Length;

    buffer.Position = 0L;

#if false
    Console.WriteLine(new StreamReader(buffer).ReadToEnd());
    buffer.Position = 0L;
#endif

    await buffer.CopyToAsync(response.OutputStream).ConfigureAwait(false);
  }

  private async Task<AuthorizedSession?> ProcessLoginDeviceMethodRequestAsync(
    HttpListenerContext context,
    UnauthorizedSession unauthorizedSession,
    JsonDocument loginDeviceMethodJsonDocument
  )
  {
    AuthorizedSession? authorizedSession = null;

    var token = FuncGenerateToken?.Invoke(unauthorizedSession);

    authorizedSession = string.IsNullOrEmpty(token)
      ? null
      : new AuthorizedSession(token, unauthorizedSession);

    var loginDeviceResponse = FuncGenerateLoginDeviceResponse?.Invoke(unauthorizedSession)
      ?? new LoginDeviceResponse() {
        ErrorCode = ErrorCode.Success,
        Result = new LoginDeviceResponse.ResponseResult() {
          Token = authorizedSession?.Token ?? string.Empty,
        }
      };

    await WriteSecurePassThroughJsonContentAsync(
      context.Response,
      (SessionBase?)authorizedSession ?? unauthorizedSession,
      ErrorCode.Success,
      loginDeviceResponse
    ).ConfigureAwait(false);

    return authorizedSession;
  }

  private async Task ProcessPassThroughRequestAsync(
    HttpListenerContext context,
    AuthorizedSession authorizedSession,
    string passThroughMethod,
    JsonDocument passThroughRequestJsonDocument
  )
  {
    passThroughRequestJsonDocument.RootElement.TryGetProperty("params", out var passThroughMethodParamsProperty);

    var (errorCode, passThroughResponse) = FuncGeneratePassThroughResponse?.Invoke(
      authorizedSession,
      passThroughMethod,
      passThroughMethodParamsProperty
    ) ?? default;

    if (passThroughResponse is null) {
      await WriteBadRequestPlainTextContentAsync(context.Response, "response was not generated").ConfigureAwait(false);
      return;
    }

    await WriteSecurePassThroughJsonContentAsync(
      context.Response,
      authorizedSession,
      errorCode,
      passThroughResponse
    ).ConfigureAwait(false);
  }
}
