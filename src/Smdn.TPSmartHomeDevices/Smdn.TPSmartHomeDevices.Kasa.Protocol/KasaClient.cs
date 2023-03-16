// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Kasa.Protocol;

/// <remarks>
/// This implementation is based on and ported from the following implementation: <see href="https://github.com/plasticrake/tplink-smarthome-api">plasticrake/tplink-smarthome-api</see>.
/// </remarks>
public sealed partial class KasaClient : IDisposable {
  public const int DefaultPort = 9999;
  internal const int DefaultBufferCapacity = 1024; // TODO: best initial capacity
  private static readonly JsonEncodedText PropertyNameForErrorCode = JsonEncodedText.Encode(
#if LANG_VERSION_11_OR_GREATER
    "err_code"u8
#else
    "err_code"
#endif
  );

  private bool IsDisposed => endPoint is null;

  private EndPoint? endPoint; // if null, it indicates a 'disposed' state.
  private Socket? socket;
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
    IServiceProvider? serviceProvider = null
  )
    : this(
      endPoint: endPoint ?? throw new ArgumentNullException(nameof(endPoint)),
      buffer: new(initialCapacity: DefaultBufferCapacity),
      serviceProvider: serviceProvider
    )
  {
  }

  internal KasaClient(
    EndPoint endPoint,
    ArrayBufferWriter<byte> buffer,
    IServiceProvider? serviceProvider = null
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
      EndPoint ep => ep, // TODO: should throw exception?
    };

    this.buffer = buffer;

    logger = serviceProvider?.GetService<ILoggerFactory>()?.CreateLogger($"{nameof(KasaClient)}({endPoint})"); // TODO: logger category name

    logger?.LogTrace("Device end point: {DeviceEndPoint} ({DeviceEndPointAddressFamily})", endPoint, endPoint.AddressFamily);
  }

  private void Dispose(bool disposing)
  {
    if (!disposing)
      return;

    endPoint = null;

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

  private async Task<Socket> ConnectAsync(
    CancellationToken cancellationToken
  )
  {
    var s = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

    try {
      logger?.LogDebug("Connecting");

      await s.ConnectAsync(
        remoteEP: endPoint
        // TODO: cancellationToken
      ).ConfigureAwait(false);

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

  public Task<TMethodResult> SendAsync<TMethodParameter, TMethodResult>(
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

  private async Task<TMethodResult> SendAsyncCore<TMethodParameter, TMethodResult>(
    JsonEncodedText module,
    JsonEncodedText method,
    TMethodParameter parameter,
    Func<JsonElement, TMethodResult> composeResult,
    CancellationToken cancellationToken = default
  )
  {
    if (socket is null) {
      // ensure socket created and connected
      socket = await ConnectAsync(cancellationToken);

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
          SocketError.ConnectionReset // ECONNRESET
      ) {
        throw new KasaDisconnectedException(ex.Message, endPoint, ex);
      }
    }
    finally {
      buffer.Clear(); // clear buffer state for next use
    }

    /*
    * receive
    */
    try {
      const SocketFlags receiveSocketFlags = default;
      const int receiveBlockSize = 0x100;

      for (; ;) {
        var buf = buffer.GetMemory(receiveBlockSize);

        int len = default;

        try {
          len = await socket.ReceiveAsync(
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

        if (len <= 0)
          break;

        buffer.Advance(len);

        if (len < buf.Length)
          break;
      }

      if (buffer.WrittenCount == 0)
        throw new KasaDisconnectedException("The peer may have dropped connection", endPoint, innerException: null);

      logger?.LogTrace("Received response {ResponseSize} bytes", buffer.WrittenCount);
      logger?.LogTrace("Buffer capacity: {Capacity} bytes", buffer.Capacity);

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

      if (
        result.TryGetProperty(PropertyNameForErrorCode.EncodedUtf8Bytes, out var propErrorCode) &&
        propErrorCode.TryGetInt32(out var errorCode) &&
        errorCode != 0
      ) {
        throw new KasaErrorResponseException(
          deviceEndPoint: endPoint,
          requestModule: Encoding.UTF8.GetString(module.EncodedUtf8Bytes),
          requestMethod: Encoding.UTF8.GetString(method.EncodedUtf8Bytes),
          errorCode: (ErrorCode)errorCode
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
}
