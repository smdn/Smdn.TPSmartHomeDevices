// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
// cSpell:ignore SESSIONID,nobom

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Smdn.IO;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

partial class PseudoTapoDevice {
  private const int SHA256HashSizeInBytes =
#if SYSTEM_SECURITY_CRYPTOGRAPHY_SHA256_HASHSIZEINBYTES
    SHA256.HashSizeInBytes;
#else
    32;
#endif

  private class KlapSessionBase : SessionBase {
    public IPEndPoint RemoteEndPoint { get; }

    public KlapSessionBase(
      object? state,
      IPEndPoint remoteEndPoint,
      string sessionId,
      DateTime expiresOn
    )
      : base(
        state,
        sessionId,
        expiresOn
      )
    {
      RemoteEndPoint = remoteEndPoint;
    }
  }

  private class UnauthorizedKlapSession : KlapSessionBase {
    public ReadOnlyMemory<byte> RemoteSeed { get; }
    public ReadOnlyMemory<byte> LocalSeed { get; }
    public ReadOnlyMemory<byte> AuthHash { get; }

    public UnauthorizedKlapSession(
      object? state,
      IPEndPoint remoteEndPoint,
      string sessionId,
      DateTime expiresOn,
      ReadOnlyMemory<byte> remoteSeed,
      ReadOnlyMemory<byte> localSeed,
      ReadOnlyMemory<byte> authHash
    )
      : base(
        state,
        remoteEndPoint,
        sessionId,
        expiresOn
      )
    {
      RemoteSeed = remoteSeed;
      LocalSeed = localSeed;
      AuthHash = authHash;
    }

    public ReadOnlyMemory<byte> GenerateHandshake1SeedAuthHash()
    {
      // seed_auth_hash = SHA256(remote_seed + local_seed + local_auth_hash)
      var seedAuthHash = new byte[SHA256HashSizeInBytes];

      using var sha256 = SHA256.Create();

      _ = sha256.TryComputeHash(
        seedAuthHash.AsSpan(),
        RemoteSeed.Span,
        LocalSeed.Span,
        AuthHash.Span,
        out _
      );

      return seedAuthHash;
    }

    public ReadOnlyMemory<byte> GenerateHandshake2SeedAuthHash()
    {
      // seed_auth_hash = SHA256(local_seed + remote_seed + local_auth_hash)
      var seedAuthHash = new byte[SHA256HashSizeInBytes];

      using var sha256 = SHA256.Create();

      _ = sha256.TryComputeHash(
        seedAuthHash.AsSpan(),
        LocalSeed.Span,
        RemoteSeed.Span,
        AuthHash.Span,
        out _
      );

      return seedAuthHash;
    }
  }

  private class AuthorizedKlapSession : KlapSessionBase {
    public KlapEncryptionAlgorithm KlapEncryptionAlgorithm { get; }
    public ArrayBufferWriter<byte> DecryptionBuffer { get; } = new(256);
    public ArrayBufferWriter<byte> EncryptionBuffer { get; } = new(1024);

    public AuthorizedKlapSession(
      object? state,
      IPEndPoint remoteEndPoint,
      string sessionId,
      DateTime expiresOn,
      ReadOnlySpan<byte> remoteSeed,
      ReadOnlySpan<byte> localSeed,
      ReadOnlySpan<byte> authHash
    )
      : base(
        state,
        remoteEndPoint,
        sessionId,
        expiresOn
      )
    {
      KlapEncryptionAlgorithm = new(
        localSeed: remoteSeed,
        remoteSeed: localSeed,
        userHash: authHash
      );
    }
  }

  private readonly ConcurrentDictionary<string, UnauthorizedKlapSession> unauthorizedKlapSessions = new();
  private readonly ConcurrentDictionary<string, AuthorizedKlapSession> authorizedKlapSessions = new();

  public Action<IPEndPoint, Memory<byte>, Memory<byte>>? FuncGenerateKlapAuthHash { get; set; }
  public Func<SessionBase, (HttpStatusCode, string)>? FuncGenerateKlapHandshake2Response { get; set; }
  public Func<SessionBase, JsonDocument, int, object?>? FuncGenerateKlapRequestResponse { get; set; }

  private async Task ProcessKlapHandshake1RequestAsync(HttpListenerContext context)
  {
    IPEndPoint? remoteEndPoint = null;

    try {
      remoteEndPoint = context.Request.RemoteEndPoint;
    }
    catch (NullReferenceException) when (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
      throw new ClientDisconnectedException();
    }

    var remoteSeed = context.Request.InputStream.ReadToEnd();

    var localSeed = RandomNumberGenerator.GetBytes(16);
    var authHash = new byte[SHA256HashSizeInBytes];

    FuncGenerateKlapAuthHash?.Invoke(remoteEndPoint, localSeed, authHash);

    var session = new UnauthorizedKlapSession(
      state: State,
      remoteEndPoint: remoteEndPoint!,
      sessionId: Convert.ToHexString(RandomNumberGenerator.GetBytes(16)),
      expiresOn: DateTime.Now + TimeSpan.FromDays(1.0),
      remoteSeed: remoteSeed,
      localSeed: localSeed,
      authHash: authHash
    );

    var cookieValue = FuncGenerateCookieValue?.Invoke(session)
      ?? $"TP_SESSIONID={session.SessionId};TIMEOUT={(int)(session.ExpiresOn - DateTime.Now).TotalMinutes}";

    if (!string.IsNullOrEmpty(cookieValue))
      context.Response.AddHeader("Set-Cookie", cookieValue);

    unauthorizedKlapSessions.AddOrUpdate(session.SessionId, session, (key, existing) => session);

    var seedAuthHash = session.GenerateHandshake1SeedAuthHash();
    var content = new byte[session.LocalSeed.Length + seedAuthHash.Length];

    session.LocalSeed.Span.CopyTo(content.AsSpan(0, session.LocalSeed.Length));
    seedAuthHash.Span.CopyTo(content.AsSpan(session.LocalSeed.Length, seedAuthHash.Length));

    await WriteApplicationOctetStreamContentAsync(
      context.Response,
      HttpStatusCode.OK,
      content
    ).ConfigureAwait(false);
  }

  private async Task ProcessKlapHandshake2RequestAsync(HttpListenerContext context)
  {
    if (!TapoSessionCookieUtils.TryGetCookie(context.Request.Headers.GetValues("Cookie"), out var sessionId, out _) || string.IsNullOrEmpty(sessionId)) {
      await WritePlainTextContentAsync(
        context.Response,
        HttpStatusCode.Forbidden,
        "Session ID not specified"
      );
      return;
    }

    if (!unauthorizedKlapSessions.TryGetValue(sessionId, out var unauthorizedSession)) {
      await WritePlainTextContentAsync(
        context.Response,
        HttpStatusCode.Forbidden,
        "Proceeding session not found"
      );
      return;
    }

    var remoteSeedAuthHash = context.Request.InputStream.ReadToEnd();
    var localSeedAuthHash = unauthorizedSession.GenerateHandshake2SeedAuthHash();

    if (!localSeedAuthHash.Span.SequenceEqual(remoteSeedAuthHash)) {
      await WritePlainTextContentAsync(
        context.Response,
        HttpStatusCode.Forbidden,
        "Seeded auth hash mismatch"
      );
      return;
    }

    var authorizedSession = new AuthorizedKlapSession(
      state: unauthorizedSession.State,
      remoteEndPoint: unauthorizedSession.RemoteEndPoint,
      sessionId: unauthorizedSession.SessionId,
      expiresOn: unauthorizedSession.ExpiresOn,
      remoteSeed: unauthorizedSession.RemoteSeed.Span,
      localSeed: unauthorizedSession.LocalSeed.Span,
      authHash: unauthorizedSession.AuthHash.Span
    );

    unauthorizedKlapSessions.TryRemove(unauthorizedSession.SessionId, out _);
    authorizedKlapSessions.AddOrUpdate(authorizedSession.SessionId, authorizedSession, (key, existing) => authorizedSession);

    if (FuncGenerateKlapHandshake2Response is null) {
      await WritePlainTextContentAsync(
        context.Response,
        HttpStatusCode.OK,
        "authorized"
      );
    }
    else {
      var (statusCode, content) = FuncGenerateKlapHandshake2Response(authorizedSession);

      await WritePlainTextContentAsync(
        context.Response,
        statusCode,
        content
      );
    }
  }

  private static readonly Regex regexQuerySequenceNumber = new(@"(^|\b)seq=(?<seq>\-?[0-9]+)", RegexOptions.Singleline);

  private async Task ProcessKlapRequestAsync(HttpListenerContext context)
  {
    var request = context.Request;
    var response = context.Response;

    if (request.Url is null || string.IsNullOrEmpty(request.Url.Query)) {
      await WriteBadRequestPlainTextContentAsync(
        response,
        "could not get request URL or query"
      ).ConfigureAwait(false);
      return;
    }

    var match = regexQuerySequenceNumber.Match(request.Url.Query);
    int? sequenceNumber = null;

    if (match.Success && int.TryParse(match.Groups["seq"].Value, out var seq))
      sequenceNumber = seq;

    if (sequenceNumber is null) {
      await WriteBadRequestPlainTextContentAsync(
        response,
        "request does not have the query 'seq'"
      ).ConfigureAwait(false);
      return;
    }

    if (!TapoSessionCookieUtils.TryGetCookie(context.Request.Headers.GetValues("Cookie"), out var sessionId, out _) || string.IsNullOrEmpty(sessionId)) {
      await WriteBadRequestPlainTextContentAsync(
        response,
        "Session ID not specified"
      );
      return;
    }

    if (!authorizedKlapSessions.TryGetValue(sessionId, out var session)) {
      await WriteBadRequestPlainTextContentAsync(
        response,
        "Session not established"
      );
      return;
    }

    session.DecryptionBuffer.Clear();

    session.KlapEncryptionAlgorithm.Decrypt(
      await request.InputStream.ReadToEndAsync().ConfigureAwait(false),
      sequenceNumber.Value,
      session.DecryptionBuffer
    );

    var requestJsonDocument = JsonDocument.Parse(session.DecryptionBuffer.WrittenMemory);

    var contentEncoding = utf8nobom;
    var responseString = JsonSerializer.Serialize(
      FuncGenerateKlapRequestResponse?.Invoke(session, requestJsonDocument, sequenceNumber.Value)
    );

    session.EncryptionBuffer.Clear();

    session.KlapEncryptionAlgorithm.Encrypt(
      rawText: contentEncoding.GetBytes(responseString ?? string.Empty),
      sequenceNumber: sequenceNumber.Value,
      destination: session.EncryptionBuffer
    );

    const string contentType = "application/json";

    try {
      response.StatusCode = (int)HttpStatusCode.OK;
      response.ContentEncoding = contentEncoding;
      response.ContentType = contentType;
    }
    catch (ObjectDisposedException) {
      throw new ClientDisconnectedException();
    }

    try {
      try {
        response.ContentLength64 = session.EncryptionBuffer.WrittenCount;
      }
      catch (InvalidOperationException) {
        throw new ClientDisconnectedException();
      }

      await response.OutputStream.WriteAsync(session.EncryptionBuffer.WrittenMemory).ConfigureAwait(false);
    }
    catch (ObjectDisposedException) {
      throw new ClientDisconnectedException();
    }
  }
}
