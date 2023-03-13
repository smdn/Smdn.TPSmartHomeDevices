// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Smdn.TPSmartHomeDevices.Kasa.Protocol;

namespace Smdn.TPSmartHomeDevices.Kasa;

[TestFixture]
public class KasaDeviceTests {
  private ServiceCollection? services;

  [OneTimeSetUp]
  public void SetUp()
  {
    services = new ServiceCollection();
  }

  private static System.Collections.IEnumerable YiledTestCases_Create_ArgumentNull()
  {
    yield return new object[] {
      new TestDelegate(() => KasaDevice.Create(deviceEndPointProvider: null!)),
      "deviceEndPointProvider"
    };

    yield return new object[] {
      new TestDelegate(() => KasaDevice.Create(deviceAddress: null!)),
      "deviceAddress"
    };
    yield return new object[] {
      new TestDelegate(() => KasaDevice.Create(hostName: null!)),
      "hostName"
    };
  }

  [TestCaseSource(nameof(YiledTestCases_Create_ArgumentNull))]
  public void Create_ArgumentNull(TestDelegate testAction, string expectedParamName)
  {
    var ex = Assert.Throws<ArgumentNullException>(testAction)!;

    Assert.AreEqual(expectedParamName, ex.ParamName, nameof(ex.ParamName));
  }

  [Test]
  public async Task Create_WithDeviceAddress()
  {
    using var device = KasaDevice.Create(
      deviceAddress: IPAddress.Loopback
    );

    Assert.AreEqual(
      new IPEndPoint(IPAddress.Loopback, 9999),
      await device.ResolveEndPointAsync()
    );
  }

  [Test]
  public async Task Create_WithDeviceHostName()
  {
    using var device = KasaDevice.Create(
      hostName: "localhost"
    );

    Assert.AreEqual(
      new DnsEndPoint("localhost", 9999),
      await device.ResolveEndPointAsync()
    );
  }

  [Test]
  public async Task Create_WithEndPointProvider()
  {
    using var device = KasaDevice.Create(
      deviceEndPointProvider: new StaticDeviceEndPointProvider(new DnsEndPoint("localhost", 0))
    );

    Assert.AreEqual(
      new DnsEndPoint("localhost", KasaClient.DefaultPort),
      await device.ResolveEndPointAsync()
    );
  }

  [Test]
  public void Dispose()
  {
    using var device = KasaDevice.Create(
      deviceAddress: IPAddress.Loopback
    );

    Assert.DoesNotThrow(device.Dispose, "dispose");
    Assert.DoesNotThrow(device.Dispose, "dispose again");

    Assert.Throws<ObjectDisposedException>(() => Assert.IsFalse(device.IsConnected), nameof(device.IsConnected));

    Assert.ThrowsAsync<ObjectDisposedException>(async () => await device.ResolveEndPointAsync(), nameof(device.ResolveEndPointAsync));
    Assert.Throws<ObjectDisposedException>(() => device.ResolveEndPointAsync(), nameof(device.ResolveEndPointAsync));
  }

  private static System.Collections.IEnumerable YieldTestCases_ResolveEndPointAsync_ResolveToDefaultPort()
  {
    yield return new object[] { new IPEndPoint(IPAddress.Loopback, 0), new IPEndPoint(IPAddress.Loopback, KasaClient.DefaultPort) };
    yield return new object[] { new DnsEndPoint("localhost", 0), new DnsEndPoint("localhost", KasaClient.DefaultPort) };
    yield return new object[] { new CustomEndPoint(port: 0), new CustomEndPoint(port: 0) };
  }

  [TestCaseSource(nameof(YieldTestCases_ResolveEndPointAsync_ResolveToDefaultPort))]
  public async Task ResolveEndPointAsync_ResolveToDefaultPort(EndPoint endPoint, EndPoint expectedEndPoint)
  {
    using var device = KasaDevice.Create(
      deviceEndPointProvider: new StaticDeviceEndPointProvider(endPoint)
    );

    Assert.AreEqual(
      expectedEndPoint,
      await device.ResolveEndPointAsync()
    );
  }

  [Test]
  public void ResolveEndPointAsync_FailedToResolve()
  {
    var provider = new UnresolvedDeviceEndPointProvider();

    using var device = KasaDevice.Create(
      deviceEndPointProvider: provider
    );

    var ex = Assert.ThrowsAsync<DeviceEndPointResolutionException>(async () => await device.ResolveEndPointAsync());

    Assert.IsNotNull(ex!.EndPointProvider, nameof(ex.EndPointProvider));
    Assert.AreSame(provider, ex.EndPointProvider, nameof(ex.EndPointProvider));
  }

  [Test]
  public void ResolveEndPointAsync_CancellationRequested()
  {
    using var cts = new CancellationTokenSource();
    using var device = KasaDevice.Create(
      deviceEndPointProvider: new ThrowExceptionDeviceEndPointProvider()
    );

    cts.Cancel();

    Assert.ThrowsAsync<TaskCanceledException>(
      async () => await device.ResolveEndPointAsync(cts.Token)
    );
  }

  private class ConcreteKasaDevice : KasaDevice {
    public ConcreteKasaDevice(IDeviceEndPointProvider deviceEndPointProvider)
      : base(deviceEndPointProvider: deviceEndPointProvider)
    {
    }

    public Task SendRequestAsync(
      JsonEncodedText module,
      JsonEncodedText method,
      CancellationToken cancellationToken = default
    )
      => SendRequestAsync(
        module: module,
        method: method,
        parameters: default(NullParameter),
        cancellationToken: cancellationToken
      );

    public new Task<TMethodResult> SendRequestAsync<TMethodResult>(
      JsonEncodedText module,
      JsonEncodedText method,
      Func<JsonElement, TMethodResult> composeResult,
      CancellationToken cancellationToken = default
    )
      => SendRequestAsync(
        module: module,
        method: method,
        parameters: default(NullParameter),
        composeResult: composeResult,
        cancellationToken: cancellationToken
      );
  }

  [Test]
  public async Task SendRequestAsync()
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, request) => {
        Assert.AreEqual(
          @"{""module"":{""method"":{}}}",
          JsonSerializer.Serialize(request),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""module"":{""method"":{""err_code"":0}}}");
      },
    };

    pseudoDevice.Start();

    using var device = new ConcreteKasaDevice(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider()
    );

    Assert.IsFalse(device.IsConnected, nameof(device.IsConnected));

    Assert.DoesNotThrowAsync(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method")
      )
    );

    Assert.IsTrue(device.IsConnected, nameof(device.IsConnected));
  }

  [Test]
  public void SendRequestAsync_Disposed()
  {
    using var device = new ConcreteKasaDevice(
      deviceEndPointProvider: new ThrowExceptionDeviceEndPointProvider()
    );

    device.Dispose();

    Assert.Throws<ObjectDisposedException>(
      () => device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method")
      )
    );
    Assert.ThrowsAsync<ObjectDisposedException>(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method")
      )
    );
  }

  [Test]
  public void SendRequestAsync_FailedToResolveDeviceEndPoint()
  {
    var provider = new UnresolvedDeviceEndPointProvider();

    using var device = new ConcreteKasaDevice(
      deviceEndPointProvider: provider
    );

    var ex = Assert.ThrowsAsync<DeviceEndPointResolutionException>(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method")
      )
    );

    Assert.IsNotNull(ex!.EndPointProvider, nameof(ex.EndPointProvider));
    Assert.AreSame(provider, ex.EndPointProvider, nameof(ex.EndPointProvider));
    Assert.IsFalse(device.IsConnected, nameof(device.IsConnected));
  }

  [Test]
  public void SendRequestAsync_SocketException()
  {
    using var device = new ConcreteKasaDevice(
      deviceEndPointProvider: new StaticDeviceEndPointProvider(
        new DnsEndPoint("device.invalid", KasaClient.DefaultPort, AddressFamily.InterNetwork)
      )
    );

    var ex = Assert.ThrowsAsync<SocketException>(
      async() => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method")
      )
    );

    Assert.That(
      ex!.SocketErrorCode,
      Is.AnyOf(SocketError.HostNotFound, SocketError.TryAgain),
      nameof(ex.SocketErrorCode)
    );

    Assert.IsFalse(device.IsConnected, "inner client must be disposed");
  }

  [Test]
  public void SendRequestAsync_CancellationRequestedAfterResolution()
  {
    using var cts = new CancellationTokenSource();
    using var device = new ConcreteKasaDevice(
      deviceEndPointProvider: new RequestCancellationAfterReturnDeviceEndPointProvider(
        cts,
        new IPEndPoint(IPAddress.Loopback, 0)
      )
    );

    Assert.ThrowsAsync<OperationCanceledException>(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method"),
        cancellationToken: cts.Token
      )
    );

    Assert.IsFalse(device.IsConnected, nameof(device.IsConnected));
  }

  [Test]
  public async Task SendRequestAsync_ResolvedEndPointHasChangedFromPreviousEndPoint()
  {
    static JsonDocument GenerateResponse(string result)
      => JsonDocument.Parse(
        @$"{{""module"":{{""method"":{{""err_code"":0,""result"":""{result}""}}}}}}"
      );

    Func<JsonElement, string> composeResult = static jsonElement => jsonElement.GetProperty("result").GetString()!;

    const string returnValueFromEndPoint1 = "endpoint #1";
    const string returnValueFromEndPoint2 = "endpoint #2";

    await using var pseudoDeviceEndPoint1 = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, _) => GenerateResponse(returnValueFromEndPoint1),
    };
    await using var pseudoDeviceEndPoint2 = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, _) => GenerateResponse(returnValueFromEndPoint2),
    };

    pseudoDeviceEndPoint1.Start();

    var provider = new DynamicDeviceEndPointProvider(pseudoDeviceEndPoint1.EndPoint);

    using var device = new ConcreteKasaDevice(
      deviceEndPointProvider: provider
    );

    var resultFromEndPoint1 = await device.SendRequestAsync(
      module: JsonEncodedText.Encode("module"),
      method: JsonEncodedText.Encode("method"),
      composeResult: composeResult
    );

    Assert.AreEqual(resultFromEndPoint1, returnValueFromEndPoint1, nameof(resultFromEndPoint1));
    Assert.IsTrue(device.IsConnected, nameof(device.IsConnected));

    // endpoint changed
    pseudoDeviceEndPoint2.Start(exceptPort: pseudoDeviceEndPoint1.EndPoint.Port);

    provider.EndPoint = pseudoDeviceEndPoint2.EndPoint;

    // dispose endpoint #1
    await pseudoDeviceEndPoint1.DisposeAsync();

    var resultFromEndPoint2 = await device.SendRequestAsync(
      module: JsonEncodedText.Encode("module"),
      method: JsonEncodedText.Encode("method"),
      composeResult: composeResult
    );

    Assert.AreEqual(resultFromEndPoint2, returnValueFromEndPoint2, nameof(resultFromEndPoint2));
    Assert.IsTrue(device.IsConnected, nameof(device.IsConnected));
  }

  [Test]
  public async Task SendRequestAsync_EndPointUnreachable_StaticEndPoint()
  {
    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, _) => JsonDocument.Parse(
        @"{""module"":{""method"":{""err_code"":0}}}"
      ),
    };

    pseudoDevice.Start();

    var endPoint = pseudoDevice.GetEndPointProvider();

    Assert.IsTrue(endPoint.IsStaticEndPoint, nameof(endPoint.IsStaticEndPoint));

    using var device = new ConcreteKasaDevice(
      deviceEndPointProvider: endPoint
    );

    Assert.DoesNotThrowAsync(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method"),
        composeResult: static _ => 0
      )
    );
    Assert.IsTrue(device.IsConnected, nameof(device.IsConnected));

    // set to unreachable state
    await pseudoDevice.StopAsync();

    var ex = Assert.ThrowsAsync<SocketException>(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method"),
        composeResult: static _ => 0
      )
    );

    Assert.AreEqual(SocketError.ConnectionRefused, ex!.SocketErrorCode, nameof(ex.SocketErrorCode));

    Assert.IsFalse(device.IsConnected, "inner client must be disposed");
  }

  [Test]
  public async Task SendRequestAsync_EndPointUnreachable_DynamicEndPoint()
  {
    await using var pseudoDeviceEndPoint1 = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, req) => {
        Assert.IsTrue(req.RootElement.TryGetProperty("module1", out var _));

        return JsonDocument.Parse(
          @"{""module1"":{""method"":{""err_code"":0,""endpoint"":1}}}"
        );
      },
    };
    await using var pseudoDeviceEndPoint2 = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, req) => {
        Assert.IsTrue(req.RootElement.TryGetProperty("module2", out var _));

        return JsonDocument.Parse(
          @"{""module2"":{""method"":{""err_code"":0,""endpoint"":2}}}"
        );
      },
    };

    pseudoDeviceEndPoint1.Start();

    var provider = new DynamicDeviceEndPointProvider(pseudoDeviceEndPoint1.EndPoint);

    using var device = new ConcreteKasaDevice(
      deviceEndPointProvider: provider
    );

    Assert.DoesNotThrowAsync(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module1"),
        method: JsonEncodedText.Encode("method"),
        composeResult: static _ => 0
      ),
      "request #1"
    );
    Assert.IsTrue(device.IsConnected, nameof(device.IsConnected));

    // endpoint changed
    pseudoDeviceEndPoint2.Start(exceptPort: pseudoDeviceEndPoint1.EndPoint.Port);

    provider.EndPoint = pseudoDeviceEndPoint2.EndPoint;

    // dispose endpoint #1
    await pseudoDeviceEndPoint1.DisposeAsync();

    Assert.DoesNotThrowAsync(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module2"),
        method: JsonEncodedText.Encode("method"),
        composeResult: static _ => 0
      ),
      "request #2"
    );
    Assert.IsTrue(device.IsConnected, nameof(device.IsConnected));
  }

  private static byte[] EncryptedResponseDocument(JsonDocument document)
  {
    var buffer = new ArrayBufferWriter<byte>(256);

    using (var writer = new Utf8JsonWriter(buffer)) {
      document.WriteTo(writer);
    }

    var length = buffer.WrittenCount;
    var encryptedResponse = new byte[4 + length];

    BinaryPrimitives.WriteInt32BigEndian(encryptedResponse.AsSpan(0, 4), length);

    var body = encryptedResponse.AsMemory(4, length);

    buffer.WrittenMemory.CopyTo(body);

    KasaJsonSerializer.EncryptInPlace(body.Span);

    return encryptedResponse;
  }

  [Test]
  public async Task SendRequestAsync_ReceivedIncompleteResponse_RetrySuccess()
  {
    int request = 0;

    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, _) => JsonDocument.Parse(@"{""module"":{""method"":{""err_code"":0}}}"),
      FuncEncryptResponse = responseDocument => {
        var resp = EncryptedResponseDocument(responseDocument);

        if (request++ == 0) {
          return resp.AsSpan(0, resp.Length - 1).ToArray(); // truncate response message body
        }
        else {
          return resp;
        }
      }
    };

    pseudoDevice.Start();

    using var device = new ConcreteKasaDevice(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider()
    );

    Assert.IsFalse(device.IsConnected, nameof(device.IsConnected));

    Assert.DoesNotThrowAsync(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method")
      )
    );

    Assert.IsTrue(device.IsConnected, nameof(device.IsConnected));
    Assert.AreEqual(2, request, nameof(request));
  }

  [Test]
  public async Task SendRequestAsync_ReceivedIncompleteResponse_RetryFailure()
  {
    int request = 0;

    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, _) => JsonDocument.Parse(@"{""module"":{""method"":{""err_code"":0}}}"),
      FuncEncryptResponse = responseDocument => {
        request++;

        var resp = EncryptedResponseDocument(responseDocument);

        return resp.AsSpan(0, resp.Length - 1).ToArray(); // truncate response message body
      }
    };

    pseudoDevice.Start();

    using var device = new ConcreteKasaDevice(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider()
    );

    Assert.IsFalse(device.IsConnected, nameof(device.IsConnected));

    Assert.ThrowsAsync<KasaIncompleteResponseException>(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method")
      )
    );

    Assert.AreEqual(2, request, nameof(request));
    Assert.IsFalse(device.IsConnected, "inner client must be disposed");
  }
}
