// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Kasa.Protocol;

/// <summary>
/// Provides a client implementation that sends requests and receives responses according to the protocol used in the Kasa's communication.
/// </summary>
/// <remarks>
/// This implementation is based on and ported from the following
/// TypeScript implementation by <see href="https://github.com/plasticrake">Donald Patrick Seal</see>:
/// <see href="https://github.com/plasticrake/tplink-smarthome-api">plasticrake/tplink-smarthome-api</see>, published under the MIT License.
/// </remarks>
public sealed partial class KasaClient : IDisposable {
  public const int DefaultPort = 9999;
  internal const int DefaultBufferCapacity = 1536; // 1.5 [kB]

#pragma warning disable SA1114
  private static readonly JsonEncodedText PropertyNameForErrorCode = JsonEncodedText.Encode(
#if LANG_VERSION_11_OR_GREATER
    "err_code"u8
#else
    "err_code"
#endif
  );
#pragma warning restore SA1114

  // Kasa device seems to automatically close connection within approx 30 secs since the lastest request.
  private static readonly TimeSpan connectionRefreshInterval = TimeSpan.FromSeconds(30);

  // The timeout for receiving the rest of the response when the device sends a split partial response.
  private static readonly TimeSpan receiveRestOfBodyTimeout = TimeSpan.FromMilliseconds(500);

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
  [MemberNotNullWhen(false, nameof(endPoint))]
#endif
  private bool IsDisposed => endPoint is null;

  private EndPoint endPoint; // if null, it indicates a 'disposed' state.
  private Socket? socket;
  private DateTime lastSentAt;
  private readonly ILogger? logger;
  private readonly ArrayBufferWriter<byte> buffer;

  public bool IsConnected {
    get {
      ThrowIfDisposed();
      return socket is not null && socket.Connected;
    }
  }

  public EndPoint EndPoint {
    get {
      ThrowIfDisposed();
      return endPoint;
    }
  }

  internal ILogger? Logger {
    get {
      ThrowIfDisposed();
      return logger;
    }
  }

  public KasaClient(
    EndPoint endPoint,
    ILogger? logger = null
  )
    : this(
      endPoint: endPoint ?? throw new ArgumentNullException(nameof(endPoint)),
      buffer: new(initialCapacity: DefaultBufferCapacity),
      logger: logger
    )
  {
  }

  internal KasaClient(
    EndPoint endPoint,
    ArrayBufferWriter<byte> buffer,
    ILogger? logger
  )
  {
    this.endPoint = endPoint switch {
      null => throw new ArgumentNullException(nameof(endPoint)),
      DnsEndPoint dnsEndPoint => dnsEndPoint.Port == 0
        ? new DnsEndPoint(
          host: dnsEndPoint.Host,
          port: DefaultPort
        )
        : dnsEndPoint,
      IPEndPoint ipEndPoint => ipEndPoint.Port == 0
        ? new IPEndPoint(
          address: ipEndPoint.Address,
          port: DefaultPort
        )
        : ipEndPoint,
      _ => endPoint, // TODO: should throw exception?
    };

    this.buffer = buffer;
    this.logger = logger;
    this.logger?.LogTrace("Device end point: {DeviceEndPoint} ({DeviceEndPointAddressFamily})", endPoint, endPoint.AddressFamily);
  }

  private void Dispose(bool disposing)
  {
    if (!disposing)
      return;

    endPoint = null!;

    socket?.Dispose();
    socket = null;
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  private void ThrowIfDisposed()
  {
    if (IsDisposed)
      throw new ObjectDisposedException(GetType().FullName);
  }

#pragma warning disable SA1112
  private async ValueTask<Socket> ConnectAsync(
#if SYSTEM_NET_SOCKETS_SOCKET_CONNECTASYNC_REMOTEEP_CANCELLATIONTOKEN
    CancellationToken cancellationToken
#endif
  )
#pragma warning restore SA1112
  {
    var addressFamily = endPoint.AddressFamily == AddressFamily.Unspecified
      ? Socket.OSSupportsIPv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork
      : endPoint.AddressFamily;
    var s = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);

    try {
      logger?.LogDebug("Connecting");

#pragma warning disable SA1114
      await s.ConnectAsync(
#if SYSTEM_NET_SOCKETS_SOCKET_CONNECTASYNC_REMOTEEP_CANCELLATIONTOKEN
        remoteEP: endPoint,
        cancellationToken: cancellationToken
#else
        remoteEP: endPoint
#endif
      ).ConfigureAwait(false);
#pragma warning restore SA1114

      logger?.LogDebug("Connected");

      return s;
    }
    catch (Exception ex) {
      s.Dispose();

      if (ex is SocketException exSocket)
        logger?.LogError(ex, "Connection failed (NativeErrorCode = {NativeErrorCode})", exSocket.NativeErrorCode);
      else
        logger?.LogCritical(ex, "Connection failed with unexpected exception");
      throw;
    }
  }

  public ValueTask<TMethodResult> SendAsync<TMethodParameter, TMethodResult>(
    JsonEncodedText module,
    JsonEncodedText method,
    TMethodParameter parameter,
    Func<JsonElement, TMethodResult> composeResult,
    CancellationToken cancellationToken = default
  )
  {
    if (parameter is null)
      throw new ArgumentNullException(nameof(parameter));
    if (composeResult is null)
      throw new ArgumentNullException(nameof(composeResult));

    ThrowIfDisposed();

    return SendAsyncCore(
      module: module,
      method: method,
      parameter: parameter,
      composeResult: composeResult,
      cancellationToken: cancellationToken
    );
  }

  private async ValueTask<TMethodResult> SendAsyncCore<TMethodParameter, TMethodResult>(
    JsonEncodedText module,
    JsonEncodedText method,
    TMethodParameter parameter,
    Func<JsonElement, TMethodResult> composeResult,
    CancellationToken cancellationToken = default
  )
  {
    // If some period of interval has elapsed since the lastest request,
    // dispose the current connection since since it is likely that the
    // connection has already been disconnected.
    if (socket is not null && lastSentAt + connectionRefreshInterval <= DateTime.Now) {
      socket.Dispose();
      socket = null;
    }

    if (socket is null) {
      // ensure socket created and connected
      socket =
#if SYSTEM_NET_SOCKETS_SOCKET_CONNECTASYNC_REMOTEEP_CANCELLATIONTOKEN
        await ConnectAsync(cancellationToken).ConfigureAwait(false);
#else
        await ConnectAsync().ConfigureAwait(false);
#endif

      // clear buffer for the initial use
      buffer.Clear();
    }

    /*
     * send
     */
    try {
      const SocketFlags sendSocketFlags = default;

      KasaJsonSerializer.Serialize(buffer, module, method, parameter, logger);

      logger?.LogTrace("Sending request {RequestSize} bytes", buffer.WrittenCount);

      try {
        await socket.SendAsync(
          buffer.WrittenMemory,
          sendSocketFlags,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
      }
      catch (SocketException ex) when (
        ex.SocketErrorCode is
          SocketError.Shutdown or // EPIPE
          SocketError.ConnectionReset or // ECONNRESET
          SocketError.ConnectionAborted // WSAECONNABORTED
      ) {
        throw new KasaDisconnectedException(ex.Message, endPoint, ex);
      }
    }
    finally {
      buffer.Clear(); // clear buffer state for next use
    }

    try {
      /*
       * receive
       */
      await ReceiveAsync(
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      lastSentAt = DateTime.Now;

      if (buffer.WrittenCount == 0)
        throw new KasaDisconnectedException("The peer may have dropped connection", endPoint, innerException: null);

      logger?.LogTrace("Received response {ResponseSize} bytes", buffer.WrittenCount);
      logger?.LogTrace("Buffer capacity: {Capacity} bytes", buffer.Capacity);

      /*
       * decrypt and parse
       */
      cancellationToken.ThrowIfCancellationRequested();

      JsonElement result = default;

      try {
        result = KasaJsonSerializer.Deserialize(buffer, module, method, logger);
      }
      catch (KasaMessageBodyTooShortException ex) {
        throw new KasaIncompleteResponseException(
          message: "Received incomplete response: " + ex.Message,
          deviceEndPoint: endPoint,
          requestModule: Encoding.UTF8.GetString(module.EncodedUtf8Bytes),
          requestMethod: Encoding.UTF8.GetString(method.EncodedUtf8Bytes),
          innerException: ex
        );
      }
      catch (KasaMessageException ex) {
        throw new KasaUnexpectedResponseException(
          message: "Received unexpected or invalid response",
          deviceEndPoint: endPoint,
          requestModule: Encoding.UTF8.GetString(module.EncodedUtf8Bytes),
          requestMethod: Encoding.UTF8.GetString(method.EncodedUtf8Bytes),
          innerException: ex
        );
      }

      const int ErrorCodeSuccess = 0;

      if (
        result.TryGetProperty(PropertyNameForErrorCode.EncodedUtf8Bytes, out var propErrorCode) &&
        propErrorCode.TryGetInt32(out var errorCode) &&
        errorCode != ErrorCodeSuccess
      ) {
        throw new KasaErrorResponseException(
          deviceEndPoint: endPoint,
          requestModule: Encoding.UTF8.GetString(module.EncodedUtf8Bytes),
          requestMethod: Encoding.UTF8.GetString(method.EncodedUtf8Bytes),
          rawErrorCode: errorCode
        );
      }

      try {
        return composeResult(result);
      }
      catch (Exception ex) {
        throw new KasaUnexpectedResponseException(
          message: "Unexpected method result: " + JsonSerializer.Serialize(result),
          deviceEndPoint: endPoint,
          requestModule: Encoding.UTF8.GetString(module.EncodedUtf8Bytes),
          requestMethod: Encoding.UTF8.GetString(method.EncodedUtf8Bytes),
          ex
        );
      }
    }
    finally {
      buffer.Clear(); // clear buffer state for next use
    }
  }

  private async ValueTask ReceiveAsync(
    CancellationToken cancellationToken
  )
  {
#if DEBUG
    if (socket is null)
      throw new InvalidOperationException($"{nameof(socket)} is null");
#endif

    CancellationTokenSource? cancellationTokenSourceForReceiveRestOfBody = null;
    CancellationTokenSource? linkedCancellationTokenSource = null;

    try {
      const SocketFlags receiveSocketFlags = default;
      const int receiveBlockSize = 0x400;
      int expectedBodyLength = default;

      for (; ;) {
        var buf = buffer.GetMemory(receiveBlockSize);
        int len = default;

        try {
          len = await socket!.ReceiveAsync(
            buf,
            receiveSocketFlags,
            cancellationToken: cancellationToken
          ).ConfigureAwait(false);
        }
        catch (SocketException ex) when (
          ex.SocketErrorCode is
            SocketError.ConnectionReset // ECONNRESET
        ) {
          throw new KasaDisconnectedException(ex.Message, endPoint, ex);
        }
        catch (OperationCanceledException) when (
#pragma warning disable CA1508
          cancellationTokenSourceForReceiveRestOfBody is not null &&
          cancellationTokenSourceForReceiveRestOfBody.IsCancellationRequested
#pragma warning restore CA1508
        ) {
          logger?.LogWarning(
            "Timed out receiving up to expected message size. ({ReceivedBodyLength}/{ExpectedBodyLength} bytes)",
            buffer.WrittenCount - KasaJsonSerializer.SizeOfHeaderInBytes,
            expectedBodyLength
          );

          return; // expected cancellation
        }

        if (len <= 0)
          goto RECEIVE_DONE;

        buffer.Advance(len);

        if (len < buf.Length)
          goto RECEIVE_DONE;

        continue;

      RECEIVE_DONE:
        // If the buffer is not filled up to the expected body size, attempt to receive
        // the rest of body after creating CancellationToken with a specific timeout duration.
        if (
          cancellationTokenSourceForReceiveRestOfBody is null &&
          KasaJsonSerializer.TryReadMessageBodyLength(buffer.WrittenMemory, out expectedBodyLength) &&
          buffer.WrittenMemory.Length < KasaJsonSerializer.SizeOfHeaderInBytes + expectedBodyLength
        ) {
          logger?.LogInformation(
            "Not received up to expected message size, continue receiving. (expect {RestOfBodyLength} more bytes of {ExpectedBodyLength} bytes body, timeout {Timeout} ms)",
            expectedBodyLength - (buffer.WrittenCount - KasaJsonSerializer.SizeOfHeaderInBytes),
            expectedBodyLength,
            receiveRestOfBodyTimeout.TotalMilliseconds
          );

          // create CancellationTokenSource for timeout
          cancellationTokenSourceForReceiveRestOfBody = new CancellationTokenSource(delay: receiveRestOfBodyTimeout);

          // link with the supplied CancellationToken
          linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            cancellationTokenSourceForReceiveRestOfBody.Token
          );

          // replace to linked CancellationToken
          cancellationToken = linkedCancellationTokenSource.Token;

          continue;
        }

        return;
      } // for
    }
    finally {
      cancellationTokenSourceForReceiveRestOfBody?.Dispose();
      linkedCancellationTokenSource?.Dispose();
    }
  }
}
