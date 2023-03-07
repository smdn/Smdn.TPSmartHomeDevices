// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.TPSmartHomeDevices.Kasa.Protocol;

public sealed class PseudoKasaDevice : IDisposable, IAsyncDisposable {
  public IPEndPoint? EndPoint { get; private set; }
  private Socket? listener;
  private Task? taskProcessListener;
  private CancellationTokenSource? listenerCancellationTokenSource = new();

  public Func<EndPoint, JsonDocument, JsonDocument>? FuncGenerateResponse { get; set; }

  private void ThrowIfDisposed()
  {
    if (listener is null)
      throw new ObjectDisposedException(GetType().FullName);
  }

  public async ValueTask DisposeAsync()
  {
    try {
      listenerCancellationTokenSource?.Cancel();
      listenerCancellationTokenSource?.Dispose();
      listenerCancellationTokenSource = null;

      listener?.Close();
      listener?.Dispose();
      listener = null;

      if (taskProcessListener is not null) {
        try {
          await taskProcessListener.ConfigureAwait(false);
        }
        catch (OperationCanceledException) {
          // expected
        }
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
    listenerCancellationTokenSource?.Cancel();
    listenerCancellationTokenSource?.Dispose();
    listenerCancellationTokenSource = null;

    listener?.Dispose();
    listener = null;

    taskProcessListener?.Dispose();
    taskProcessListener = null;
  }

  public IDeviceEndPointProvider GetEndPointProvider()
    => new StaticDeviceEndPointProvider(EndPoint);

  public IPEndPoint Start(
    int? exceptPort = 0
  )
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
      if (!EndPointUtils.TryFindUnusedPort(exceptPort, out var port))
        throw new InvalidOperationException("could not find unused port");

      listener = CreateListeningSocket(new IPEndPoint(IPAddress.Any, port));
    }
    else {
      foreach (var port in EndPointUtils.EnumerateIANASuggestedDynamicPorts(exceptPort)) {
        try {
          listener = CreateListeningSocket(new IPEndPoint(IPAddress.Any, port));
          break;
        }
        catch (SocketException) {
          continue;
        }
      }

      if (listener is null)
        throw new InvalidOperationException("could not find unused port");
    }

    taskProcessListener = Task.Run(() => ProcessListenerAsync(listenerCancellationTokenSource.Token));

    EndPoint = listener?.LocalEndPoint as IPEndPoint;

    return EndPoint ?? throw new InvalidOperationException("could not get listener end point");

    static Socket CreateListeningSocket(IPEndPoint endPoint)
    {
      Socket? s = null;

      try {
        s = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        s.Bind(endPoint);
        s.Listen(backlog: 2);

        return s;
      }
      catch {
        s?.Dispose();
        throw;
      }
    }
  }

  public async Task StopAsync()
  {
    ThrowIfDisposed();

    await DisposeAsync();
  }

  private async Task ProcessListenerAsync(
    CancellationToken cancellationToken
  )
  {
    for (; ;) {
      if (listener is null)
        break;

      Socket? socket = null;

      try {
        try {
          socket = await listener.AcceptAsync(cancellationToken);
        }
        catch (ObjectDisposedException) {
          break;
        }

        await ProcessClientAsync(socket, cancellationToken);
      }
      finally {
#if false
        if (socket is not null)
          socket.LingerState = new LingerOption(false, 0);
#endif

        socket?.Shutdown(SocketShutdown.Both);
        socket?.Disconnect(true);
        socket?.Close();
        socket?.Dispose();
      }
    }
  }

  private async Task ProcessClientAsync(
    Socket socket,
    CancellationToken cancellationToken
  )
  {
    var buffer = new ArrayBufferWriter<byte>(initialCapacity: 1024);

    for (; ;) {
      JsonDocument? receivedJsonDocument = null;

      try {
        const SocketFlags receiveSocketFlags = default;
        const int receiveBlockSize = 0x100;

        for (; ;) {
          var buf = buffer.GetMemory(receiveBlockSize);

          var len = await socket.ReceiveAsync(
            buf,
            receiveSocketFlags,
            cancellationToken: cancellationToken
          ).ConfigureAwait(false);

          if (len <= 0)
            return; // disconnected

          buffer.Advance(len);

          if (len < buf.Length)
            break;
        }

        var length = BinaryPrimitives.ReadInt32BigEndian(buffer.WrittenSpan);
        var body = new byte[length];

        buffer.WrittenSpan.Slice(4).CopyTo(body);

        KasaJsonSerializer.DecryptInPlace(body);

        receivedJsonDocument = JsonDocument.Parse(body);
      }
      finally {
        buffer.Clear(); // clear buffer state for next use
      }

      var response = FuncGenerateResponse?.Invoke(socket.RemoteEndPoint, receivedJsonDocument);

      if (response is null)
        return;

      byte[]? responseBuffer = null;

      try {
        const SocketFlags sendSocketFlags = default;

        using (var writer = new Utf8JsonWriter(buffer)) {
          response.WriteTo(writer);
        }

        var length = buffer.WrittenCount;

        responseBuffer = ArrayPool<byte>.Shared.Rent(4 + length);

        BinaryPrimitives.WriteInt32BigEndian(responseBuffer.AsSpan(0, 4), length);

        var body = responseBuffer.AsMemory(4, length);

        buffer.WrittenMemory.CopyTo(body);

        KasaJsonSerializer.EncryptInPlace(body.Span);

        await socket.SendAsync(
          responseBuffer.AsMemory(0, 4 + length),
          sendSocketFlags,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
      }
      finally {
        if (responseBuffer is not null)
          ArrayPool<byte>.Shared.Return(responseBuffer);

        buffer.Clear(); // clear buffer state for next use
      }
    }
  }
}
