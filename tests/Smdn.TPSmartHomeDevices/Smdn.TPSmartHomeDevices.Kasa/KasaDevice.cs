// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Smdn.TPSmartHomeDevices.Kasa.Protocol;

namespace Smdn.TPSmartHomeDevices.Kasa;

[TestFixture]
public class KasaDeviceTests {
  private const int RertyMaxAttemptsForIncompleteResponse = 3;

  [TestCaseSource(typeof(ConcreteKasaDeviceCommonTests), nameof(ConcreteKasaDeviceCommonTests.YiledTestCases_Ctor_ArgumentException))]
  public void Create_ArgumentException(
    Type[] methodParameterTypes,
    object?[] methodParameters,
    Type? expectedExceptionType,
    string expectedParamName
  )
    => ConcreteKasaDeviceCommonTests.TestCreate_ArgumentException(
      typeof(KasaDevice),
      nameof(KasaDevice.Create),
      methodParameterTypes,
      methodParameters,
      expectedExceptionType,
      expectedParamName
    );

  [Test]
  public async Task Create_WithIPAddress()
  {
    using var device = KasaDevice.Create(
      ipAddress: IPAddress.Loopback
    );

    Assert.AreEqual(
      new IPEndPoint(IPAddress.Loopback, KasaClient.DefaultPort),
      await device.ResolveEndPointAsync()
    );
  }

  [Test]
  public async Task Create_WithHostName()
  {
    using var device = KasaDevice.Create(
      host: "localhost"
    );

    Assert.AreEqual(
      new DnsEndPoint("localhost", KasaClient.DefaultPort),
      await device.ResolveEndPointAsync()
    );
  }

  [Test]
  public async Task Create_WithMacAddress_IServiceProvider()
  {
    var services = new ServiceCollection();

    services.AddDeviceEndPointFactory(
      new StaticMacAddressDeviceEndPointFactory(IPAddress.Loopback)
    );

    using var device = KasaDevice.Create(
      macAddress: PhysicalAddress.None,
      serviceProvider: services.BuildServiceProvider()
    );

    Assert.AreEqual(
      new IPEndPoint(IPAddress.Loopback, KasaClient.DefaultPort),
      await device.ResolveEndPointAsync()
    );
  }

  [Test]
  public void Create_WithMacAddress_IServiceProvider_IDeviceEndPointFactoryNotRegistered()
  {
    var services = new ServiceCollection();

    Assert.Throws<InvalidOperationException>(
      () => KasaDevice.Create(
        macAddress: PhysicalAddress.None,
        serviceProvider: services.BuildServiceProvider()
      )
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
      ipAddress: IPAddress.Loopback
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
    public ConcreteKasaDevice(
      IDeviceEndPointProvider deviceEndPointProvider,
      IServiceProvider serviceProvider = null
    )
      : base(
        deviceEndPointProvider: deviceEndPointProvider,
        serviceProvider: serviceProvider
      )
    {
    }

    public ValueTask SendRequestAsync(
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

    public new ValueTask<TMethodResult> SendRequestAsync<TMethodResult>(
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
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
      Assert.Ignore("disconnection of device causes test runner timeout");
      return;
    }

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

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20.0));

    var resultFromEndPoint2 = await device.SendRequestAsync(
      module: JsonEncodedText.Encode("module"),
      method: JsonEncodedText.Encode("method"),
      composeResult: composeResult,
      cancellationToken: cts.Token
    );

    Assert.AreEqual(resultFromEndPoint2, returnValueFromEndPoint2, nameof(resultFromEndPoint2));
    Assert.IsTrue(device.IsConnected, nameof(device.IsConnected));

    // end point should not be marked as invalidated in this scenario
    Assert.IsFalse(provider.HasInvalidated, nameof(provider.HasInvalidated));
  }

  private class HandleAsEndPointUnreachableExceptionHandler : KasaClientExceptionHandler {
    public override KasaClientExceptionHandling DetermineHandling(KasaDevice device, Exception exception, int attempt, ILogger? logger)
      => Default.DetermineHandling(
        device: device,
        // reproduces the case of unreachable condition
        exception: new SocketException(
          errorCode: (int)SocketError.HostUnreachable
        ),
        attempt: attempt,
        logger: logger
      );
  }

  [Test]
  public async Task SendRequestAsync_EndPointUnreachable_StaticEndPoint()
  {
    var request = 0;

    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, _) =>
        request++ == 0
          ? JsonDocument.Parse(
              @"{""module"":{""method"":{""err_code"":0}}}"
            )
          // this causes an exception to be raised in the request to the pseudo device,
          // and the exception will be handled as an 'unreachable' event by HandleAsEndPointUnreachableExceptionHandler
          : JsonDocument.Parse(
              @"{""module"":{""method"":{""err_code"":9999}}}"
            )
    };

    pseudoDevice.Start();

    var endPoint = pseudoDevice.GetEndPointProvider();

    Assert.That(endPoint, Is.AssignableTo<IDeviceEndPointProvider>(), nameof(endPoint));
    Assert.That(endPoint, Is.Not.AssignableTo<IDynamicDeviceEndPointProvider>(), nameof(endPoint));

    var services = new ServiceCollection();

    services.AddSingleton<KasaClientExceptionHandler>(
      new HandleAsEndPointUnreachableExceptionHandler() // handle any exception as 'unreachable' event
    );

    using var device = new ConcreteKasaDevice(
      deviceEndPointProvider: endPoint,
      serviceProvider: services.BuildServiceProvider()
    );

    Assert.DoesNotThrowAsync(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method"),
        composeResult: static _ => 0
      )
    );
    Assert.IsTrue(device.IsConnected, nameof(device.IsConnected));

    Assert.CatchAsync(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method"),
        composeResult: static _ => 0
      )
    );

    Assert.IsFalse(device.IsConnected, "inner client must be disposed");
  }

  [Test]
  public Task SendRequestAsync_EndPointUnreachable_DynamicEndPoint_RetrySuccess()
    => SendRequestAsync_EndPointUnreachable_DynamicEndPoint(caseWhenRetrySuccess: true);

  [Test]
  public Task SendRequestAsync_EndPointUnreachable_DynamicEndPoint_RetryFailure()
    => SendRequestAsync_EndPointUnreachable_DynamicEndPoint(caseWhenRetrySuccess: false);

  private async Task SendRequestAsync_EndPointUnreachable_DynamicEndPoint(bool caseWhenRetrySuccess)
  {
    await using var pseudoDeviceEndPoint1 = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, req) => {
        if (req.RootElement.TryGetProperty("module1", out var _)) {
          return JsonDocument.Parse(
            @"{""module1"":{""method"":{""err_code"":0,""endpoint"":1}}}"
          );
        }
        else {
          // this causes an exception to be raised in the request to the pseudo device #1,
          // and the exception will be handled as an 'unreachable' event by HandleAsEndPointUnreachableExceptionHandler
          return JsonDocument.Parse(
            @"{""module1"":{""method"":{""err_code"":9999,""endpoint"":1}}}"
          );
        }
      },
    };
    await using var pseudoDeviceEndPoint2 = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, req) => {
        Assert.IsTrue(req.RootElement.TryGetProperty("module2", out var _));

        if (caseWhenRetrySuccess) {
          return JsonDocument.Parse(
            @"{""module2"":{""method"":{""err_code"":0,""endpoint"":2}}}"
          );
        }
        else {
          // this causes an exception to be raised in the request to the pseudo device #2,
          // and the exception will be handled as an 'unreachable' event by HandleAsEndPointUnreachableExceptionHandler
          return JsonDocument.Parse(
            @"{""module2"":{""method"":{""err_code"":9999,""endpoint"":2}}}"
          );
        }
      },
    };

    pseudoDeviceEndPoint1.Start();

    var endPoint = new DynamicDeviceEndPointProvider(pseudoDeviceEndPoint1.EndPoint);
    var services = new ServiceCollection();

    services.AddSingleton<KasaClientExceptionHandler>(
      new HandleAsEndPointUnreachableExceptionHandler() // handle any exception as 'unreachable' event
    );

    using var device = new ConcreteKasaDevice(
      deviceEndPointProvider: endPoint,
      serviceProvider: services.BuildServiceProvider()
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

    var invalidatedEndPoints = new List<EndPoint>();

    endPoint.Invalidated += (_, _) => {
      invalidatedEndPoints.Add(endPoint.EndPoint);
      endPoint.EndPoint = pseudoDeviceEndPoint2.EndPoint; // change end point since end point invalidated
    };

    if (caseWhenRetrySuccess) {
      Assert.DoesNotThrowAsync(
        async () => await device.SendRequestAsync(
          module: JsonEncodedText.Encode("module2"),
          method: JsonEncodedText.Encode("method"),
          composeResult: static _ => 0
        ),
        "request #2"
      );
      Assert.IsTrue(device.IsConnected, nameof(device.IsConnected));

      // only first endpoint must have been invalidated
      Assert.AreEqual(1, invalidatedEndPoints.Count, nameof(invalidatedEndPoints.Count));
      Assert.AreEqual(pseudoDeviceEndPoint1.EndPoint, invalidatedEndPoints[0], nameof(invalidatedEndPoints) + "[0]");
    }
    else {
      Assert.CatchAsync(
        async () => await device.SendRequestAsync(
          module: JsonEncodedText.Encode("module2"),
          method: JsonEncodedText.Encode("method"),
          composeResult: static _ => 0
        ),
        "request #2"
      );
      Assert.IsFalse(device.IsConnected, nameof(device.IsConnected));

      // both of two endpoint must have been invalidated
      Assert.AreEqual(2, invalidatedEndPoints.Count, nameof(invalidatedEndPoints.Count));

      Assert.AreEqual(pseudoDeviceEndPoint1.EndPoint, invalidatedEndPoints[0], nameof(invalidatedEndPoints) + "[0]");
      Assert.AreEqual(pseudoDeviceEndPoint2.EndPoint, invalidatedEndPoints[1], nameof(invalidatedEndPoints) + "[1]");
    }

    Assert.IsTrue(endPoint.HasInvalidated, nameof(endPoint.HasInvalidated));
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

        if (++request < RertyMaxAttemptsForIncompleteResponse) {
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
    Assert.AreEqual(RertyMaxAttemptsForIncompleteResponse, request, nameof(request));
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

    Assert.AreEqual(RertyMaxAttemptsForIncompleteResponse, request, nameof(request));
    Assert.IsFalse(device.IsConnected, "inner client must be disposed");
  }

  private class AssertOperationCanceledMustNotBeHandledExceptionHandler : KasaClientExceptionHandler {
    public override KasaClientExceptionHandling DetermineHandling(KasaDevice device, Exception exception, int attempt, ILogger? logger)
    {
      Assert.IsNotAssignableFrom<OperationCanceledException>(exception);

      return Default.DetermineHandling(
        device: device,
        exception: exception,
        attempt: attempt,
        logger: logger
      );
    }
  }

  [Test]
  public async Task SendRequestAsync_ExceptionHandlerMustNotHandleOperationCanceledException()
  {
    var ctsRequest = new CancellationTokenSource();

    await using var pseudoDevice = new PseudoKasaDevice() {
      FuncGenerateResponse = (_, request) => {
        ctsRequest.Cancel();
        return JsonDocument.Parse(@"{""module"":{""method"":{""err_code"":0}}}");
      }
    };

    pseudoDevice.Start();

    var services = new ServiceCollection();

    services.AddSingleton<KasaClientExceptionHandler>(
      // asserts that the OperationCanceledException must not be handled
      new AssertOperationCanceledMustNotBeHandledExceptionHandler()
    );

    using var device = new ConcreteKasaDevice(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider()
    );

    Assert.IsFalse(device.IsConnected, nameof(device.IsConnected));

    var ex = Assert.CatchAsync(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method"),
        cancellationToken: ctsRequest.Token
      )
    );

    Assert.That(ex, Is.InstanceOf<OperationCanceledException>().Or.InstanceOf<TaskCanceledException>());

    Assert.IsFalse(device.IsConnected, nameof(device.IsConnected));
  }
}
