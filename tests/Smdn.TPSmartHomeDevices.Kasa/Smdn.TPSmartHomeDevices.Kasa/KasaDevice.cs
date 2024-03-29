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
public partial class KasaDeviceTests {
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
  public new void ToString()
    => ConcreteKasaDeviceCommonTests.TestToString<KasaDevice>();

  [Test]
  public async Task Create_WithIPAddress()
  {
    using var device = KasaDevice.Create(
      ipAddress: IPAddress.Loopback
    );

    Assert.That(device.EndPoint, Is.Not.Null);
    Assert.That(
      await device.ResolveEndPointAsync(),
      Is.EqualTo(new IPEndPoint(IPAddress.Loopback, KasaClient.DefaultPort))
    );
  }

  [Test]
  public async Task Create_WithHostName()
  {
    using var device = KasaDevice.Create(
      host: "localhost"
    );

    Assert.That(device.EndPoint, Is.Not.Null);
    Assert.That(
      await device.ResolveEndPointAsync(),
      Is.EqualTo(new DnsEndPoint("localhost", KasaClient.DefaultPort))
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

    Assert.That(device.EndPoint, Is.Not.Null);
    Assert.That(
      await device.ResolveEndPointAsync(),
      Is.EqualTo(new IPEndPoint(IPAddress.Loopback, KasaClient.DefaultPort))
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
  public async Task Create_WithEndPoint()
  {
    var endpoint = new StaticDeviceEndPoint(new DnsEndPoint("localhost", 0));

    using var device = KasaDevice.Create(
      deviceEndPoint: endpoint
    );

    Assert.That(device.EndPoint, Is.EqualTo(endpoint));
    Assert.That(
      await device.ResolveEndPointAsync(),
      Is.EqualTo(new DnsEndPoint("localhost", KasaClient.DefaultPort))
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

    Assert.Throws<ObjectDisposedException>(() => Assert.That(device.EndPoint, Is.Not.Null), nameof(device.EndPoint));
    Assert.Throws<ObjectDisposedException>(() => Assert.That(device.IsConnected, Is.False), nameof(device.IsConnected));

    Assert.ThrowsAsync<ObjectDisposedException>(async () => await device.ResolveEndPointAsync(), nameof(device.ResolveEndPointAsync));
#pragma warning disable CA2012
    Assert.Throws<ObjectDisposedException>(() => device.ResolveEndPointAsync(), nameof(device.ResolveEndPointAsync));
#pragma warning restore CA2012
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
      deviceEndPoint: new StaticDeviceEndPoint(endPoint)
    );

    Assert.That(
      await device.ResolveEndPointAsync(),
      Is.EqualTo(expectedEndPoint)
    );
  }

  [Test]
  public void ResolveEndPointAsync_FailedToResolve()
  {
    var endPoint = new UnresolvedDeviceEndPoint();

    using var device = KasaDevice.Create(
      deviceEndPoint: endPoint
    );

    var ex = Assert.ThrowsAsync<DeviceEndPointResolutionException>(async () => await device.ResolveEndPointAsync());

    Assert.That(ex!.DeviceEndPoint, Is.Not.Null, nameof(ex.DeviceEndPoint));
    Assert.That(ex.DeviceEndPoint, Is.SameAs(endPoint), nameof(ex.DeviceEndPoint));
  }

  [Test]
  public void ResolveEndPointAsync_CancellationRequested()
  {
    using var cts = new CancellationTokenSource();
    using var device = KasaDevice.Create(
      deviceEndPoint: new ThrowExceptionDeviceEndPoint()
    );

    cts.Cancel();

    Assert.ThrowsAsync<TaskCanceledException>(
      async () => await device.ResolveEndPointAsync(cts.Token)
    );
  }

  private class ConcreteKasaDevice : KasaDevice {
    public ConcreteKasaDevice(
      IDeviceEndPoint deviceEndPoint,
      IServiceProvider? serviceProvider = null
    )
      : base(
        deviceEndPoint: deviceEndPoint,
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
        Assert.That(
          JsonSerializer.Serialize(request),
          Is.EqualTo(@"{""module"":{""method"":{}}}"),
          nameof(request)
        );

        return JsonDocument.Parse(@"{""module"":{""method"":{""err_code"":0}}}");
      },
    };

    pseudoDevice.Start();

    using var device = new ConcreteKasaDevice(
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    Assert.That(device.IsConnected, Is.False, nameof(device.IsConnected));

    Assert.DoesNotThrowAsync(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method")
      )
    );

    Assert.That(device.IsConnected, Is.True, nameof(device.IsConnected));
  }

  [Test]
  public void SendRequestAsync_Disposed()
  {
    using var device = new ConcreteKasaDevice(
      deviceEndPoint: new ThrowExceptionDeviceEndPoint()
    );

    device.Dispose();

#pragma warning disable CA2012
    Assert.Throws<ObjectDisposedException>(
      () => device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method")
      )
    );
#pragma warning restore CA2012
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
    var endPoint = new UnresolvedDeviceEndPoint();

    using var device = new ConcreteKasaDevice(
      deviceEndPoint: endPoint
    );

    var ex = Assert.ThrowsAsync<DeviceEndPointResolutionException>(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method")
      )
    );

    Assert.That(ex!.DeviceEndPoint, Is.Not.Null, nameof(ex.DeviceEndPoint));
    Assert.That(ex.DeviceEndPoint, Is.SameAs(endPoint), nameof(ex.DeviceEndPoint));
    Assert.That(device.IsConnected, Is.False, nameof(device.IsConnected));
  }

  [Test]
  public void SendRequestAsync_SocketException()
  {
    using var device = new ConcreteKasaDevice(
      deviceEndPoint: new StaticDeviceEndPoint(
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

    Assert.That(device.IsConnected, Is.False, "inner client must be disposed");
  }

  [Test]
  public void SendRequestAsync_CancellationRequestedAfterResolution()
  {
    using var cts = new CancellationTokenSource();
    using var device = new ConcreteKasaDevice(
      deviceEndPoint: new RequestCancellationAfterReturnDeviceEndPoint(
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

    Assert.That(device.IsConnected, Is.False, nameof(device.IsConnected));
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

    static string ComposeResult(JsonElement jsonElement) => jsonElement.GetProperty("result").GetString()!;

    const string ReturnValueFromEndPoint1 = "endpoint #1";
    const string ReturnValueFromEndPoint2 = "endpoint #2";

    await using var pseudoDeviceEndPoint1 = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, _) => GenerateResponse(ReturnValueFromEndPoint1),
    };
    await using var pseudoDeviceEndPoint2 = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, _) => GenerateResponse(ReturnValueFromEndPoint2),
    };

    pseudoDeviceEndPoint1.Start();

    var endPoint = new DynamicDeviceEndPoint(pseudoDeviceEndPoint1.EndPoint);

    using var device = new ConcreteKasaDevice(
      deviceEndPoint: endPoint
    );

    var resultFromEndPoint1 = await device.SendRequestAsync(
      module: JsonEncodedText.Encode("module"),
      method: JsonEncodedText.Encode("method"),
      composeResult: ComposeResult
    );

    Assert.That(resultFromEndPoint1, Is.EqualTo(ReturnValueFromEndPoint1), nameof(resultFromEndPoint1));
    Assert.That(device.IsConnected, Is.True, nameof(device.IsConnected));

    // endpoint changed
    pseudoDeviceEndPoint2.Start(exceptPort: pseudoDeviceEndPoint1.EndPoint!.Port);

    endPoint.EndPoint = pseudoDeviceEndPoint2.EndPoint;

    // dispose endpoint #1
    await pseudoDeviceEndPoint1.DisposeAsync();

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20.0));

    var resultFromEndPoint2 = await device.SendRequestAsync(
      module: JsonEncodedText.Encode("module"),
      method: JsonEncodedText.Encode("method"),
      composeResult: ComposeResult,
      cancellationToken: cts.Token
    );

    Assert.That(resultFromEndPoint2, Is.EqualTo(ReturnValueFromEndPoint2), nameof(resultFromEndPoint2));
    Assert.That(device.IsConnected, Is.True, nameof(device.IsConnected));

    // end point should not be marked as invalidated in this scenario
    Assert.That(endPoint.HasInvalidated, Is.False, nameof(endPoint.HasInvalidated));
  }

  private class HandleAsEndPointUnreachableExceptionHandler : KasaDeviceExceptionHandler {
    public override KasaDeviceExceptionHandling DetermineHandling(KasaDevice device, Exception exception, int attempt, ILogger? logger)
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

    var endPoint = pseudoDevice.GetEndPoint();

    Assert.That(endPoint, Is.AssignableTo<IDeviceEndPoint>(), nameof(endPoint));
    Assert.That(endPoint, Is.Not.AssignableTo<IDynamicDeviceEndPoint>(), nameof(endPoint));

    var services = new ServiceCollection();

    services.AddSingleton<KasaDeviceExceptionHandler>(
      new HandleAsEndPointUnreachableExceptionHandler() // handle any exception as 'unreachable' event
    );

    using var device = new ConcreteKasaDevice(
      deviceEndPoint: endPoint,
      serviceProvider: services.BuildServiceProvider()
    );

    Assert.DoesNotThrowAsync(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method"),
        composeResult: static _ => 0
      )
    );
    Assert.That(device.IsConnected, Is.True, nameof(device.IsConnected));

    Assert.CatchAsync(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method"),
        composeResult: static _ => 0
      )
    );

    Assert.That(device.IsConnected, Is.False, "inner client must be disposed");
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
        Assert.That(req.RootElement.TryGetProperty("module2", out var _), Is.True);

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

    var endPoint = new DynamicDeviceEndPoint(pseudoDeviceEndPoint1.EndPoint);
    var services = new ServiceCollection();

    services.AddSingleton<KasaDeviceExceptionHandler>(
      new HandleAsEndPointUnreachableExceptionHandler() // handle any exception as 'unreachable' event
    );

    using var device = new ConcreteKasaDevice(
      deviceEndPoint: endPoint,
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
    Assert.That(device.IsConnected, Is.True, nameof(device.IsConnected));

    // endpoint changed
    pseudoDeviceEndPoint2.Start(exceptPort: pseudoDeviceEndPoint1.EndPoint!.Port);

    var invalidatedEndPoints = new List<EndPoint>();

    endPoint.Invalidated += (_, _) => {
      invalidatedEndPoints.Add(endPoint.EndPoint!);
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
      Assert.That(device.IsConnected, Is.True, nameof(device.IsConnected));

      // only first endpoint must have been invalidated
      Assert.That(invalidatedEndPoints.Count, Is.EqualTo(1), nameof(invalidatedEndPoints.Count));
      Assert.That(invalidatedEndPoints[0], Is.EqualTo(pseudoDeviceEndPoint1.EndPoint), nameof(invalidatedEndPoints) + "[0]");
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
      Assert.That(device.IsConnected, Is.False, nameof(device.IsConnected));

      // both of two endpoint must have been invalidated
      Assert.That(invalidatedEndPoints.Count, Is.EqualTo(2), nameof(invalidatedEndPoints.Count));

      Assert.That(invalidatedEndPoints[0], Is.EqualTo(pseudoDeviceEndPoint1.EndPoint), nameof(invalidatedEndPoints) + "[0]");
      Assert.That(invalidatedEndPoints[1], Is.EqualTo(pseudoDeviceEndPoint2.EndPoint), nameof(invalidatedEndPoints) + "[1]");
    }

    Assert.That(endPoint.HasInvalidated, Is.True, nameof(endPoint.HasInvalidated));
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
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    Assert.That(device.IsConnected, Is.False, nameof(device.IsConnected));

    Assert.DoesNotThrowAsync(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method")
      )
    );

    Assert.That(device.IsConnected, Is.True, nameof(device.IsConnected));
    Assert.That(request, Is.EqualTo(RertyMaxAttemptsForIncompleteResponse), nameof(request));
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
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    Assert.That(device.IsConnected, Is.False, nameof(device.IsConnected));

    Assert.ThrowsAsync<KasaIncompleteResponseException>(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method")
      )
    );

    Assert.That(request, Is.EqualTo(RertyMaxAttemptsForIncompleteResponse), nameof(request));
    Assert.That(device.IsConnected, Is.False, "inner client must be disposed");
  }

  private class AssertOperationCanceledMustNotBeHandledExceptionHandler : KasaDeviceExceptionHandler {
    public override KasaDeviceExceptionHandling DetermineHandling(KasaDevice device, Exception exception, int attempt, ILogger? logger)
    {
      Assert.That(exception, Is.Not.AssignableFrom<OperationCanceledException>());

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

    services.AddSingleton<KasaDeviceExceptionHandler>(
      // asserts that the OperationCanceledException must not be handled
      new AssertOperationCanceledMustNotBeHandledExceptionHandler()
    );

    using var device = new ConcreteKasaDevice(
      deviceEndPoint: pseudoDevice.GetEndPoint()
    );

    Assert.That(device.IsConnected, Is.False, nameof(device.IsConnected));

    var ex = Assert.CatchAsync(
      async () => await device.SendRequestAsync(
        module: JsonEncodedText.Encode("module"),
        method: JsonEncodedText.Encode("method"),
        cancellationToken: ctsRequest.Token
      )
    );

    Assert.That(ex, Is.InstanceOf<OperationCanceledException>().Or.InstanceOf<TaskCanceledException>());

    Assert.That(device.IsConnected, Is.False, nameof(device.IsConnected));
  }
}
