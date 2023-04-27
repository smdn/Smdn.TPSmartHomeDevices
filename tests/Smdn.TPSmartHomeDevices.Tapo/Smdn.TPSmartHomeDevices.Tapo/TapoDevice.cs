// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

[TestFixture]
public partial class TapoDeviceTests {
  private ServiceCollection? services;

  [OneTimeSetUp]
  public void SetUp()
  {
    services = new ServiceCollection();

    services.AddTapoBase64EncodedCredential(
      base64UserNameSHA1Digest: Convert.ToBase64String(Encoding.UTF8.GetBytes("user")),
      base64Password: Convert.ToBase64String(Encoding.UTF8.GetBytes("pass"))
    );

    if (TestEnvironment.IsRunningOnGitHubActionsMacOSRunner) {
      services.AddTapoHttpClient(
        configureClient: client => {
          client.Timeout = TimeSpan.FromMinutes(1.0);
        }
      );
    }
  }

  [TestCaseSource(typeof(ConcreteTapoDeviceCommonTests), nameof(ConcreteTapoDeviceCommonTests.YiledTestCases_Ctor_ArgumentException))]
  public void Create_ArgumentException(
    Type[] methodParameterTypes,
    object?[] methodParameters,
    Type? expectedExceptionType,
    string expectedParamName
  )
    => ConcreteTapoDeviceCommonTests.TestCreate_ArgumentException(
      typeof(TapoDevice),
      nameof(TapoDevice.Create),
      methodParameterTypes,
      methodParameters,
      expectedExceptionType,
      expectedParamName
    );

  [Test]
  public void Create_WithCredentialProvider_ByICredentialProvider()
  {
    Assert.DoesNotThrow(
      () => {
        using var device = TapoDevice.Create(
          deviceEndPoint: new StaticDeviceEndPoint(new IPEndPoint(IPAddress.Loopback, 0)),
          credential: services?.BuildServiceProvider()!.GetRequiredService<ITapoCredentialProvider>()
        );
      }
    );
  }

  [Test]
  public void Create_WithCredentialProvider_ViaIServiceProvider()
  {
    Assert.DoesNotThrow(
      () => {
        using var device = TapoDevice.Create(
          deviceEndPoint: new StaticDeviceEndPoint(new IPEndPoint(IPAddress.Loopback, 0)),
          serviceProvider: services?.BuildServiceProvider()
        );
      }
    );
  }

  [Test]
  public void Create_WithCredentialProvider_CredentialProviderNull()
  {
    Assert.Throws<ArgumentNullException>(
      () => {
        using var device = TapoDevice.Create(
          deviceEndPoint: new StaticDeviceEndPoint(new IPEndPoint(IPAddress.Loopback, 0)),
          credential: null,
          serviceProvider: null
        );
      }
    );
  }

  [Test]
  public void Create_WithCredentialProvider_NoCredentialViaIServiceProvider()
  {
    Assert.Throws<InvalidOperationException>(
      () => {
        using var device = TapoDevice.Create(
          deviceEndPoint: new StaticDeviceEndPoint(new IPEndPoint(IPAddress.Loopback, 0)),
          credential: null,
          serviceProvider: new ServiceCollection().BuildServiceProvider()
        );
      }
    );
  }

  [Test]
  public async Task Create_WithIPAddress()
  {
    using var device = TapoDevice.Create(
      ipAddress: IPAddress.Loopback,
      "user@mail.test",
      "password"
    );

    Assert.AreEqual(
      new IPEndPoint(IPAddress.Loopback, TapoClient.DefaultPort),
      await device.ResolveEndPointAsync()
    );
  }

  [Test]
  public async Task Create_WithHostName()
  {
    using var device = TapoDevice.Create(
      host: "localhost",
      "user@mail.test",
      "password"
    );

    Assert.AreEqual(
      new DnsEndPoint("localhost", TapoClient.DefaultPort),
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

    using var device = TapoDevice.Create(
      macAddress: PhysicalAddress.None,
      "user@mail.test",
      "password",
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.AreEqual(
      new IPEndPoint(IPAddress.Loopback, TapoClient.DefaultPort),
      await device.ResolveEndPointAsync()
    );
  }

  [Test]
  public void Create_WithMacAddress_IServiceProvider_IDeviceEndPointFactoryNotRegistered()
  {
    var services = new ServiceCollection();

    Assert.Throws<InvalidOperationException>(
      () => TapoDevice.Create(
        macAddress: PhysicalAddress.None,
      "user@mail.test",
      "password",
        serviceProvider: services!.BuildServiceProvider()
      )
    );
  }

  [Test]
  public async Task Create_WithEndPoint()
  {
    using var device = TapoDevice.Create(
      deviceEndPoint: new StaticDeviceEndPoint(new DnsEndPoint("localhost", 0)),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.AreEqual(
      new DnsEndPoint("localhost", TapoClient.DefaultPort),
      await device.ResolveEndPointAsync()
    );
  }

  private const string ExpectedTerminalUuidRegexPattern = @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$";

#if false
  [TestCase("5b5ab773-e14a-4d2b-bb00-1464a64da330")]
  [TestCase("00000000-0000-0000-0000-000000000000")] // NIL
  public void Create_TerminalUuid_UserSupplied(string uuid)
  {
    using var device = TapoDevice.Create(
      deviceEndPoint: new ThrowExceptionDeviceEndPoint(),
      terminalUuid: new Guid(uuid),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.IsNotEmpty(device.TerminalUuidString, nameof(device.TerminalUuidString));
    StringAssert.IsMatch(ExpectedTerminalUuidRegexPattern, device.TerminalUuidString, nameof(device.TerminalUuidString));
  }
#endif

  [Test]
  public void Create_TerminalUuid_AutoGenerate()
  {
    using var device = TapoDevice.Create(
      ipAddress: IPAddress.Loopback,
      "user@mail.test",
      "password"
    );

    Assert.IsNotEmpty(device.TerminalUuidString, nameof(device.TerminalUuidString));
    StringAssert.IsMatch(ExpectedTerminalUuidRegexPattern, device.TerminalUuidString, nameof(device.TerminalUuidString));
  }

  private readonly struct NullParameters { }
  private readonly struct NullResult { }

  [Test]
  public void Dispose()
  {
    using var device = TapoDevice.Create(
      ipAddress: IPAddress.Loopback,
      "user@mail.test",
      "password"
    );

    Assert.DoesNotThrow(device.Dispose, "dispose");
    Assert.DoesNotThrow(device.Dispose, "dispose again");

    Assert.IsNull(device.Session, nameof(device.Session));

#pragma warning disable CA2012
    Assert.ThrowsAsync<ObjectDisposedException>(async () => await device.ResolveEndPointAsync(), nameof(device.ResolveEndPointAsync));
    Assert.Throws<ObjectDisposedException>(() => device.ResolveEndPointAsync(), nameof(device.ResolveEndPointAsync));

    Assert.ThrowsAsync<ObjectDisposedException>(async () => await device.GetDeviceInfoAsync<NullResult>(), nameof(device.GetDeviceInfoAsync));
    Assert.Throws<ObjectDisposedException>(() => device.GetDeviceInfoAsync<NullResult>(), nameof(device.GetDeviceInfoAsync));

    Assert.ThrowsAsync<ObjectDisposedException>(async () => await device.SetDeviceInfoAsync(default(NullParameters)), nameof(device.SetDeviceInfoAsync));
    Assert.Throws<ObjectDisposedException>(() => device.SetDeviceInfoAsync(default(NullParameters)), nameof(device.SetDeviceInfoAsync));

    Assert.ThrowsAsync<ObjectDisposedException>(async () => await device.TurnOnAsync(), nameof(device.TurnOnAsync));
    Assert.Throws<ObjectDisposedException>(() => device.TurnOnAsync(), nameof(device.TurnOnAsync));

    Assert.ThrowsAsync<ObjectDisposedException>(async () => await device.GetOnOffStateAsync(), nameof(device.GetOnOffStateAsync));
    Assert.Throws<ObjectDisposedException>(() => device.GetOnOffStateAsync(), nameof(device.GetOnOffStateAsync));
#pragma warning restore CA2012
  }

  private static System.Collections.IEnumerable YieldTestCases_ResolveEndPointAsync_ResolveToDefaultPort()
  {
    yield return new object[] { new IPEndPoint(IPAddress.Loopback, 0), new IPEndPoint(IPAddress.Loopback, TapoClient.DefaultPort) };
    yield return new object[] { new DnsEndPoint("localhost", 0), new DnsEndPoint("localhost", TapoClient.DefaultPort) };
    yield return new object[] { new CustomEndPoint(port: 0), new CustomEndPoint(port: 0) };
  }

  [TestCaseSource(nameof(YieldTestCases_ResolveEndPointAsync_ResolveToDefaultPort))]
  public async Task ResolveEndPointAsync_ResolveToDefaultPort(EndPoint endPoint, EndPoint expectedEndPoint)
  {
    using var device = TapoDevice.Create(
      deviceEndPoint: new StaticDeviceEndPoint(endPoint),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.AreEqual(
      expectedEndPoint,
      await device.ResolveEndPointAsync()
    );
  }

  [Test]
  public void ResolveEndPointAsync_FailedToResolve()
  {
    var endPoint = new UnresolvedDeviceEndPoint();

    using var device = TapoDevice.Create(
      deviceEndPoint: endPoint,
      serviceProvider: services!.BuildServiceProvider()
    );

    var ex = Assert.ThrowsAsync<DeviceEndPointResolutionException>(async () => await device.ResolveEndPointAsync());

    Assert.IsNotNull(ex!.DeviceEndPoint, nameof(ex.DeviceEndPoint));
    Assert.AreSame(endPoint, ex.DeviceEndPoint, nameof(ex.DeviceEndPoint));
  }

  [Test]
  public void ResolveEndPointAsync_CancellationRequested()
  {
    using var cts = new CancellationTokenSource();
    using var device = TapoDevice.Create(
      deviceEndPoint: new ThrowExceptionDeviceEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    cts.Cancel();

    Assert.ThrowsAsync<TaskCanceledException>(
      async () => await device.ResolveEndPointAsync(cts.Token)
    );
  }

  private class ConcreteTapoDevice : TapoDevice {
    public ConcreteTapoDevice(
      IDeviceEndPoint deviceEndPoint,
      IServiceProvider? serviceProvider = null
    )
      : base(
        deviceEndPoint: deviceEndPoint,
        serviceProvider: serviceProvider
      )
    {
    }

    public new ValueTask EnsureSessionEstablishedAsync(CancellationToken cancellationToken = default)
      => base.EnsureSessionEstablishedAsync(cancellationToken: cancellationToken);

    public new ValueTask<TResult> SendRequestAsync<TRequest, TResponse, TResult>(
      TRequest request,
      Func<TResponse, TResult> composeResult,
      CancellationToken cancellationToken = default
    )
      where TRequest : ITapoPassThroughRequest
      where TResponse : ITapoPassThroughResponse
      => base.SendRequestAsync(request, composeResult, cancellationToken);
  }

  [Test]
  public void EnsureSessionEstablishedAsync_Disposed()
  {
    using var device = new ConcreteTapoDevice(
      deviceEndPoint: new ThrowExceptionDeviceEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    device.Dispose();

    Assert.ThrowsAsync<ObjectDisposedException>(async () => await device.EnsureSessionEstablishedAsync());

    Assert.IsNull(device.Session);
  }

  [Test]
  public void EnsureSessionEstablishedAsync_FailedToResolveDeviceEndPoint()
  {
    var endPoint = new UnresolvedDeviceEndPoint();

    using var device = new ConcreteTapoDevice(
      deviceEndPoint: endPoint,
      serviceProvider: services!.BuildServiceProvider()
    );

    var ex = Assert.ThrowsAsync<DeviceEndPointResolutionException>(async () => await device.EnsureSessionEstablishedAsync());

    Assert.IsNotNull(ex!.DeviceEndPoint, nameof(ex.DeviceEndPoint));
    Assert.AreSame(endPoint, ex.DeviceEndPoint, nameof(ex.DeviceEndPoint));

    Assert.IsNull(device.Session);
  }

  [Test]
  public void EnsureSessionEstablishedAsync_CancellationRequestedAfterResolution()
  {
    using var cts = new CancellationTokenSource();
    using var device = new ConcreteTapoDevice(
      deviceEndPoint: new RequestCancellationAfterReturnDeviceEndPoint(
        cancellationTokenSource: cts,
        endPoint: new IPEndPoint(IPAddress.Loopback, 0)
      ),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.ThrowsAsync<OperationCanceledException>(
      async () => await device.EnsureSessionEstablishedAsync(cts.Token)
    );
    Assert.IsNull(device.Session);
  }

  [Test]
  public async Task EnsureSessionEstablishedAsync_ResolvedEndPointHasChangedFromPreviousEndPoint()
  {
    const string returnTokenFromEndPoint1 = "token1";
    const string returnTokenFromEndPoint2 = "token2";

    await using var pseudoDeviceEndPoint1 = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => returnTokenFromEndPoint1,
    };
    await using var pseudoDeviceEndPoint2 = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => returnTokenFromEndPoint2,
    };

    pseudoDeviceEndPoint1.Start();

    var endPoint = new DynamicDeviceEndPoint(pseudoDeviceEndPoint1.EndPoint);

    using var device = new ConcreteTapoDevice(
      deviceEndPoint: endPoint,
      serviceProvider: services!.BuildServiceProvider()
    );

    await device.EnsureSessionEstablishedAsync();

    Assert.IsNotNull(device.Session, nameof(device.Session));
    Assert.AreEqual(
      returnTokenFromEndPoint1,
      device.Session!.Token,
      nameof(device.Session.Token)
    );

    var prevSession = device.Session;

    // endpoint changed
    pseudoDeviceEndPoint2.Start(exceptPort: pseudoDeviceEndPoint1.EndPoint!.Port);

    endPoint.EndPoint = pseudoDeviceEndPoint2.EndPoint;

    // dispose endpoint #1
    await pseudoDeviceEndPoint1.DisposeAsync();

    await device.EnsureSessionEstablishedAsync();

    Assert.IsNotNull(device.Session, nameof(device.Session));
    Assert.AreNotSame(device.Session, prevSession, nameof(device.Session));
    Assert.AreEqual(
      returnTokenFromEndPoint2,
      device.Session.Token,
      nameof(device.Session.Token)
    );

    // end point should not be marked as invalidated in this scenario
    Assert.IsFalse(endPoint.HasInvalidated, nameof(endPoint.HasInvalidated));
  }

  [Test]
  public async Task SendRequestAsync_SocketException()
  {
    if (new Version(7, 0) <= Environment.Version && RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
      Assert.Ignore("SocketException is not thrown and a timeout occurs on MacOS + .NET 7.x");
      return;
    }

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
    };

    pseudoDevice.Start();

    using var device = new ConcreteTapoDevice(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    await device.EnsureSessionEstablishedAsync();

    // reuse endpoints to create unresponding states
    var endPoint = pseudoDevice.EndPoint;

    await pseudoDevice.DisposeAsync();

    using var listener = new Socket(endPoint!.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

    listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
    listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

    listener.Bind(endPoint); // bind but does not listen

    // send request
    var ex = Assert.ThrowsAsync<HttpRequestException>(async () => await device.GetDeviceInfoAsync());

    Assert.IsInstanceOf<SocketException>(ex!.InnerException);

    Assert.IsNull(device.Session, nameof(device.Session)); // session must be disposed
  }

  private class HandleAsEndPointUnreachableExceptionHandler : TapoDeviceExceptionHandler {
    public override TapoDeviceExceptionHandling DetermineHandling(TapoDevice device, Exception exception, int attempt, ILogger? logger)
      => Default.DetermineHandling(
        device: device,
        exception: new HttpRequestException(
          message: "reproduces the case of unreachable condition",
          inner: new SocketException(
            errorCode: (int)SocketError.HostUnreachable
          )
        ),
        attempt: attempt,
        logger: logger
      );
  }

  [Test]
  public async Task SendRequestAsync_EndPointUnreachable_StaticEndPoint()
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, _) => {
        Assert.AreEqual("get_device_info", method, "received request method");
        return (
          KnownErrorCodes.Success,
          new GetDeviceInfoResponse<NullResult>()
        );
      }
    };

    pseudoDevice.Start();

    var endPoint = pseudoDevice.GetEndPoint();

    Assert.That(endPoint, Is.AssignableTo<IDeviceEndPoint>(), nameof(endPoint));
    Assert.That(endPoint, Is.Not.AssignableTo<IDynamicDeviceEndPoint>(), nameof(endPoint));

    var serviceCollection = new ServiceCollection();

    serviceCollection.AddTapoBase64EncodedCredential(
      base64UserNameSHA1Digest: Convert.ToBase64String(Encoding.UTF8.GetBytes("user")),
      base64Password: Convert.ToBase64String(Encoding.UTF8.GetBytes("pass"))
    );
    serviceCollection.AddSingleton<TapoDeviceExceptionHandler>(
      new HandleAsEndPointUnreachableExceptionHandler() // handle any exception as 'unreachable' event
    );

    using var device = new ConcreteTapoDevice(
      deviceEndPoint: endPoint,
      serviceProvider: serviceCollection.BuildServiceProvider()
    );

    Assert.DoesNotThrowAsync(async () => await device.GetDeviceInfoAsync());
    Assert.IsNotNull(device.Session, nameof(device.Session));

    // dispose endpoint
    // this causes an exception to be raised in the request to the pseudo device,
    // and the exception will be handled as an 'unreachable' event by HandleAsEndPointUnreachableExceptionHandler
    await pseudoDevice.DisposeAsync();

    var ex = Assert.ThrowsAsync<HttpRequestException>(async () => await device.GetDeviceInfoAsync());

    Assert.IsInstanceOf<SocketException>(ex!.InnerException);

    Assert.IsNull(device.Session, nameof(device.Session));
  }

  [Test]
  public Task SendRequestAsync_EndPointUnreachable_DynamicEndPoint_RetrySuccess()
    => SendRequestAsync_EndPointUnreachable_DynamicEndPoint(caseWhenRetrySuccess: true);

  [Test]
  public Task SendRequestAsync_EndPointUnreachable_DynamicEndPoint_RetryFailure()
    => SendRequestAsync_EndPointUnreachable_DynamicEndPoint(caseWhenRetrySuccess: false);

  private async Task SendRequestAsync_EndPointUnreachable_DynamicEndPoint(bool caseWhenRetrySuccess)
  {
    const string returnTokenFromEndPoint1 = "token1";
    const string returnTokenFromEndPoint2 = "token2";

    await using var pseudoDeviceEndPoint1 = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => returnTokenFromEndPoint1,
      FuncGeneratePassThroughResponse = (_, method, _) => {
        Assert.AreEqual("get_device_info", method, "received request method");
        return (
          KnownErrorCodes.Success,
          new GetDeviceInfoResponse<NullResult>()
        );
      }
    };
    await using var pseudoDeviceEndPoint2 = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => returnTokenFromEndPoint2,
      FuncGeneratePassThroughResponse = (_, method, _) => {
        Assert.AreEqual("get_device_info", method, "received request method");
        return (
          KnownErrorCodes.Success,
          new GetDeviceInfoResponse<NullResult>() {
            ErrorCode = caseWhenRetrySuccess
              ? KnownErrorCodes.Success
              // this causes an exception to be raised in the request to the pseudo device #2,
              // and the exception will be handled as an 'unreachable' event by HandleAsEndPointUnreachableExceptionHandler
              : 9999
          }
        );
      }
    };

    pseudoDeviceEndPoint1.Start();

    var endPoint = new DynamicDeviceEndPoint(pseudoDeviceEndPoint1.EndPoint);
    var serviceCollection = new ServiceCollection();

    serviceCollection.AddTapoBase64EncodedCredential(
      base64UserNameSHA1Digest: Convert.ToBase64String(Encoding.UTF8.GetBytes("user")),
      base64Password: Convert.ToBase64String(Encoding.UTF8.GetBytes("pass"))
    );
    serviceCollection.AddSingleton<TapoDeviceExceptionHandler>(
      new HandleAsEndPointUnreachableExceptionHandler() // handle any exception as 'unreachable' event
    );

    using var device = new ConcreteTapoDevice(
      deviceEndPoint: endPoint,
      serviceProvider: serviceCollection.BuildServiceProvider()
    );

    Assert.DoesNotThrowAsync(async () => await device.GetDeviceInfoAsync(), "request #1");
    Assert.IsNotNull(device.Session, nameof(device.Session));

    var prevSession = device.Session;
    var invalidatedEndPoints = new List<EndPoint>();

    // endpoint changed
    pseudoDeviceEndPoint2.Start(exceptPort: pseudoDeviceEndPoint1.EndPoint!.Port);

    endPoint.Invalidated += (_, _) => {
      invalidatedEndPoints.Add(endPoint.EndPoint!);
      endPoint.EndPoint = pseudoDeviceEndPoint2.EndPoint; // change end point since end point invalidated
    };

    // dispose endpoint #1
    // this causes an exception to be raised in the request to the pseudo device #1,
    // and the exception will be handled as an 'unreachable' event by HandleAsEndPointUnreachableExceptionHandler
    await pseudoDeviceEndPoint1.DisposeAsync();

    if (caseWhenRetrySuccess) {
      Assert.DoesNotThrowAsync(async () => await device.GetDeviceInfoAsync(), "request #2");

      Assert.IsNotNull(device.Session, nameof(device.Session));
      Assert.AreNotSame(device.Session, prevSession, nameof(device.Session));

      // only first endpoint must have been invalidated
      Assert.AreEqual(1, invalidatedEndPoints.Count, nameof(invalidatedEndPoints.Count));
      Assert.AreEqual(pseudoDeviceEndPoint1.EndPoint, invalidatedEndPoints[0], nameof(invalidatedEndPoints) + "[0]");
    }
    else {
      Assert.CatchAsync(async () => await device.GetDeviceInfoAsync(), "request #2");

      Assert.IsNull(device.Session, nameof(device.Session));

      // both of two endpoint must have been invalidated
      Assert.AreEqual(2, invalidatedEndPoints.Count, nameof(invalidatedEndPoints.Count));

      Assert.AreEqual(pseudoDeviceEndPoint1.EndPoint, invalidatedEndPoints[0], nameof(invalidatedEndPoints) + "[0]");
      Assert.AreEqual(pseudoDeviceEndPoint2.EndPoint, invalidatedEndPoints[1], nameof(invalidatedEndPoints) + "[1]");
    }

    Assert.IsTrue(endPoint.HasInvalidated, nameof(endPoint.HasInvalidated));
  }

  [Test]
  public async Task SendRequestAsync_UnexpectedException()
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, _, _) => (
        KnownErrorCodes.Success,
        new GetDeviceInfoResponse<NullResult>() {
          ErrorCode = KnownErrorCodes.Success,
          Result = new(),
        }
      ),
    };

    pseudoDevice.Start();

    using var device = new ConcreteTapoDevice(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.IsNull(device.Session, nameof(device.Session));

    Assert.ThrowsAsync<NotSupportedException>(
      async () => await device.SendRequestAsync<GetDeviceInfoRequest, GetDeviceInfoResponse<NullResult>, int>(
        request: new GetDeviceInfoRequest(),
        composeResult: static resp => throw new NotSupportedException("unexpected exception"),
        cancellationToken: default
      )
    );

    Assert.IsNull(device.Session, nameof(device.Session));
  }

  [Test]
  public async Task SendRequestAsync_ErrorResponse_RetrySuccess()
  {
    const int getDeviceInfoErrorCode = 1234;
    var request = 0;

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = _ => $"token-request{request}",
      FuncGeneratePassThroughResponse = (_, _, _) => (
        KnownErrorCodes.Success,
        new GetDeviceInfoResponse<NullResult>() {
          ErrorCode = request++ == 0 ? getDeviceInfoErrorCode : KnownErrorCodes.Success,
          Result = new(),
        }
      ),
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.IsNull(device.Session, nameof(device.Session));

    Assert.DoesNotThrowAsync(async () => await device.GetDeviceInfoAsync());

    Assert.AreEqual(2, request, nameof(request));
    Assert.IsNotNull(device.Session, nameof(device.Session));
    Assert.AreEqual("token-request1", device.Session!.Token, nameof(device.Session.Token));
  }

  [Test]
  public async Task SendRequestAsync_ErrorResponse_RetryFailedWithErrorResponse()
  {
    const int getDeviceInfoErrorCode = 1234;
    var request = 0;

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = _ => $"token-request{request}",
      FuncGeneratePassThroughResponse = (_, _, _) => {
        request++;

        return (
          KnownErrorCodes.Success,
          new GetDeviceInfoResponse<NullResult>() {
            ErrorCode = getDeviceInfoErrorCode,
            Result = new(),
          }
        );
      },
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.IsNull(device.Session, nameof(device.Session));

    var ex = Assert.ThrowsAsync<TapoErrorResponseException>(async () => await device.GetDeviceInfoAsync());

    Assert.AreEqual(2, request, nameof(request));
    StringAssert.Contains("token=token-request1", ex!.EndPoint!.Query, nameof(ex.EndPoint.Query));
    Assert.IsNull(device.Session, nameof(device.Session));
  }

  [Test]
  public async Task SendRequestAsync_ResponseWithErrorCodeMinus1301_RetrySuccess()
  {
    var request = 0;

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = _ => $"token-request{request}",
      FuncGeneratePassThroughResponse = (_, _, _) => (
        KnownErrorCodes.Success,
        new GetDeviceInfoResponse<NullResult>() {
          ErrorCode = request++ == 0 ? KnownErrorCodes.Minus1301 : KnownErrorCodes.Success,
          Result = new(),
        }
      ),
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.IsNull(device.Session, nameof(device.Session));

    Assert.DoesNotThrowAsync(async () => await device.GetDeviceInfoAsync());

    Assert.AreEqual(2, request, nameof(request));
    Assert.IsNotNull(device.Session, nameof(device.Session));
    Assert.AreEqual("token-request1", device.Session!.Token, nameof(device.Session.Token));
  }

  [Test]
  public async Task SendRequestAsync_ResponseWithErrorCodeMinus1301_RetryFailedWithErrorResponse()
  {
    var request = 0;

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = _ => $"token-request{request}",
      FuncGeneratePassThroughResponse = (_, _, _) => {
        request++;

        return (
          KnownErrorCodes.Success,
          new GetDeviceInfoResponse<NullResult>() {
            ErrorCode = KnownErrorCodes.Minus1301,
            Result = new(),
          }
        );
      },
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.IsNull(device.Session, nameof(device.Session));

    var ex = Assert.ThrowsAsync<TapoErrorResponseException>(async () => await device.GetDeviceInfoAsync());

    Assert.AreEqual(2, request, nameof(request));
    Assert.AreEqual(KnownErrorCodes.Minus1301, ex!.RawErrorCode, nameof(ex.RawErrorCode));
    StringAssert.Contains("token=token-request1", ex.EndPoint!.Query, nameof(ex.EndPoint.Query));
    Assert.IsNull(device.Session, nameof(device.Session));
  }

  [Test]
  public Task SendRequestAsync_Timeout_RetrySuccess_SetTimeoutWithIHttpClientFactory()
    => SendRequestAsync_Timeout_RetrySuccess(setTimeoutViaIHttpClientFactory: true);

  [Test]
  public Task SendRequestAsync_Timeout_RetrySuccess_SetTimeoutWithDeviceTimeoutProperty()
    => SendRequestAsync_Timeout_RetrySuccess(setTimeoutViaIHttpClientFactory: false);

  private async Task SendRequestAsync_Timeout_RetrySuccess(bool setTimeoutViaIHttpClientFactory)
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
      Assert.Ignore("disabled this test case because the test runner process crashes");
      return;
    }

    const int maxRetry = 3;

    using var cts = new CancellationTokenSource();
    var pseudoDevices = new List<PseudoTapoDevice>(capacity: maxRetry);

    for (var attempt = 0; attempt < maxRetry; attempt++) {
      var state = Tuple.Create(
        attempt < maxRetry - 1
          ? TimeSpan.FromSeconds(60)
          : TimeSpan.Zero,
        $"token-request{attempt}"
      );

      pseudoDevices.Add(
        new(state: state) {
          FuncGenerateToken = static session => (string)(session.State as Tuple<TimeSpan, string>)!.Item2,
          FuncGeneratePassThroughResponse = (session, _, _) => {
            // perform latency
            DelayUtils.Delay((session.State as Tuple<TimeSpan, string>)!.Item1, cts.Token);

            return (
              KnownErrorCodes.Success,
              new GetDeviceInfoResponse<NullResult>() {
                ErrorCode = KnownErrorCodes.Success,
                Result = new(),
              }
            );
          },
        }
      );

      pseudoDevices[attempt].Start();
    }

    static TimeSpan GetTimeout()
      => TestEnvironment.IsRunningOnGitHubActionsMacOSRunner
        ? TimeSpan.FromSeconds(20)
        : TimeSpan.FromMilliseconds(200);

    if (setTimeoutViaIHttpClientFactory) {
      services!.AddTapoHttpClient(
        configureClient: static client => client.Timeout = GetTimeout()
      );
    }

    using var device = TapoDevice.Create(
      deviceEndPoint: new TransitionalDeviceEndPoint(
        pseudoDevices.Select(pseudoDevice => pseudoDevice.EndPoint!)
      ),
      serviceProvider: services!.BuildServiceProvider()
    );

    if (!setTimeoutViaIHttpClientFactory)
      device.Timeout = GetTimeout();

    try {
      Assert.IsNull(device.Session, nameof(device.Session));

      Assert.DoesNotThrowAsync(async () => await device.GetDeviceInfoAsync());

      Assert.IsNotNull(device.Session, nameof(device.Session));
      Assert.AreEqual("token-request2", device.Session!.Token, nameof(device.Session.Token));
    }
    finally {
      cts.Cancel();

      foreach (var pseudoDevice in pseudoDevices) {
        await pseudoDevice.DisposeAsync();
      }
    }
  }

  [Test]
  public async Task SendRequestAsync_Timeout_RetryFailedWithTimeout()
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
      Assert.Ignore("disabled this test case because the test runner process crashes");
      return;
    }

    const int maxRetry = 3;

    using var cts = new CancellationTokenSource();
    var pseudoDevices = new List<PseudoTapoDevice>(capacity: maxRetry);

    for (var attempt = 0; attempt < maxRetry; attempt++) {
      var state = Tuple.Create(
        TimeSpan.FromSeconds(60),
        $"token-request{attempt}"
      );

      pseudoDevices.Add(
        new(state: state) {
          FuncGenerateToken = static session => (string)(session.State as Tuple<TimeSpan, string>)!.Item2,
          FuncGeneratePassThroughResponse = (session, _, _) => {
            // perform latency
            DelayUtils.Delay((session.State as Tuple<TimeSpan, string>)!.Item1, cts.Token);

            return (
              KnownErrorCodes.Success,
              new GetDeviceInfoResponse<NullResult>() {
                ErrorCode = KnownErrorCodes.Success,
                Result = new(),
              }
            );
          },
        }
      );

      pseudoDevices[attempt].Start();
    }

    using var device = TapoDevice.Create(
      deviceEndPoint: new TransitionalDeviceEndPoint(
        pseudoDevices.Select(pseudoDevice => pseudoDevice.EndPoint!)
      ),
      serviceProvider: services!.BuildServiceProvider()
    );

    device.Timeout = TestEnvironment.IsRunningOnGitHubActionsMacOSRunner
      ? TimeSpan.FromSeconds(20)
      : TimeSpan.FromMilliseconds(200);

    try {
      Assert.IsNull(device.Session, nameof(device.Session));

      var ex = Assert.ThrowsAsync<TapoProtocolException>(async () => await device.GetDeviceInfoAsync());

      Assert.IsInstanceOf<TimeoutException>(ex!.InnerException, nameof(ex.InnerException));

      Assert.IsNull(device.Session, nameof(device.Session));
    }
    finally {
      cts.Cancel();

      foreach (var pseudoDevice in pseudoDevices) {
        await pseudoDevice.DisposeAsync();
      }
    }
  }

  private class AssertOperationCanceledMustNotBeHandledExceptionHandler : TapoDeviceExceptionHandler {
    public override TapoDeviceExceptionHandling DetermineHandling(TapoDevice device, Exception exception, int attempt, ILogger? logger)
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

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, passThroughMethod, _) => {
        ctsRequest.Cancel();

        return new(
          KnownErrorCodes.Success,
          new GetDeviceInfoResponse<NullResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new(),
          }
        );
      },
    };

    pseudoDevice.Start();

    services!.AddSingleton<TapoDeviceExceptionHandler>(
      // asserts that the OperationCanceledException must not be handled
      new AssertOperationCanceledMustNotBeHandledExceptionHandler()
    );

    using var device = new ConcreteTapoDevice(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.IsNull(device.Session, nameof(device.Session));

    var ex = Assert.CatchAsync(
      async () => await device.GetDeviceInfoAsync(cancellationToken: ctsRequest.Token)
    );

    Assert.That(ex, Is.InstanceOf<OperationCanceledException>().Or.InstanceOf<TaskCanceledException>());

    Assert.IsNull(device.Session, nameof(device.Session));
  }

  private readonly struct GetDeviceInfoResult {
    [JsonPropertyName("device_on")]
    public bool IsOn { get; init; }

    [JsonPropertyName("device_id")]
    public string? Id { get; init; }
  }

  [Test]
  public async Task GetDeviceInfoAsync()
  {
    var requestSequenceNumber = 0;

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, _) => {
        Assert.AreEqual("get_device_info", method, "received request method");
        return (
          KnownErrorCodes.Success,
          new GetDeviceInfoResponse<GetDeviceInfoResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new() {
              // response #0: IsOn = true
              // response #1: IsOn = false
              IsOn = requestSequenceNumber++ == 0 ? true : false,
            },
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.IsNull(device.Session, nameof(device.Session));

    // call GetDeviceInfoAsync in the state that the session has not been established
    var deviceInfo0 = await device.GetDeviceInfoAsync<GetDeviceInfoResult>();

    Assert.IsNotNull(deviceInfo0, nameof(deviceInfo0));
    Assert.IsTrue(deviceInfo0.IsOn, nameof(deviceInfo0.IsOn));
    Assert.IsNotNull(device.Session, nameof(device.Session));

    // call GetDeviceInfoAsync in the state that the session has been established
    var deviceInfo1 = await device.GetDeviceInfoAsync<GetDeviceInfoResult>();

    Assert.IsNotNull(deviceInfo1, nameof(deviceInfo1));
    Assert.IsFalse(deviceInfo1.IsOn, nameof(deviceInfo1.IsOn));
    Assert.AreNotSame(deviceInfo0, deviceInfo1, nameof(deviceInfo1));
    Assert.IsNotNull(device.Session, nameof(device.Session));
  }

  [Test]
  public async Task GetDeviceInfoAsync_ComposeResult()
  {
    const string deviceId = "device-id";
    const bool isOn = true;

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, _) => {
        Assert.AreEqual("get_device_info", method, "received request method");
        return (
          KnownErrorCodes.Success,
          new GetDeviceInfoResponse<GetDeviceInfoResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new() {
              IsOn = isOn,
              Id = deviceId,
            },
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    var (resultIsOn, resultId) = await device.GetDeviceInfoAsync<GetDeviceInfoResult, (bool, string?)>(
      composeResult: result => (result.IsOn, result.Id)
    );

    Assert.AreEqual(isOn, resultIsOn, nameof(resultIsOn));
    Assert.AreEqual(deviceId, resultId, nameof(resultId));
  }

  [Test]
  public async Task GetDeviceInfoAsync_ComposeResult_Null()
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.IsNull(device.Session, nameof(device.Session));

    Assert.ThrowsAsync<ArgumentNullException>(
      async () => await device.GetDeviceInfoAsync<GetDeviceInfoResult, bool>(composeResult: null!)
    );

    Assert.IsNull(device.Session, nameof(device.Session));
  }

  private readonly struct GetDeviceInfoOnOffStateResult {
    [JsonPropertyName("device_on")]
    public bool IsOn { get; init; }
  }

  [Test]
  public async Task GetDeviceInfoAsync_ResponseWithExtraData()
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = static (_, _, _) => {
        return (
          KnownErrorCodes.Success,
          new GetDeviceInfoResponse<GetDeviceInfoResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new() {
              IsOn = true,
              Id = "device-id", // as an extra data
            },
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    var deviceInfo = await device.GetDeviceInfoAsync<GetDeviceInfoOnOffStateResult>();

    Assert.IsNotNull(deviceInfo, nameof(deviceInfo));
  }

  [Test]
  public async Task GetDeviceInfoAsync_ErrorResponse()
  {
    const int getDeviceInfoErrorCode = 1234;

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = static (_, _, _) => (
        KnownErrorCodes.Success,
        new GetDeviceInfoResponse<NullResult>() {
          ErrorCode = getDeviceInfoErrorCode,
          Result = new(),
        }
      ),
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.IsNull(device.Session, $"{nameof(device.Session)} before GetDeviceInfoAsync");

    var ex = Assert.ThrowsAsync<TapoErrorResponseException>(
      async () => await device.GetDeviceInfoAsync()
    );
    Assert.AreEqual("get_device_info", ex!.RequestMethod);
    Assert.AreEqual(getDeviceInfoErrorCode, ex.RawErrorCode);
    Assert.IsNull(device.Session, $"{nameof(device.Session)} after GetDeviceInfoAsync");
  }

  [Test]
  public async Task SetDeviceInfoAsync()
  {
    var requestSequenceNumber = 0;

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        Assert.AreEqual("set_device_info", method, "received request method");

        switch (requestSequenceNumber++) {
          case 0:
            Assert.IsTrue(requestParams.GetProperty("device_on")!.GetBoolean());
            break;

          case 1:
            Assert.IsFalse(requestParams.GetProperty("device_on")!.GetBoolean());
            break;

          default:
            throw new InvalidOperationException("unexpected request");
        }

        return (
          KnownErrorCodes.Success,
          new SetDeviceInfoResponse() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new(),
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.IsNull(device.Session, $"{nameof(device.Session)} before SetDeviceInfoAsync");

    // call SetDeviceInfoAsync in the state that the session has not been established
    await device.SetDeviceInfoAsync(new { device_on = true });

    Assert.IsNotNull(device.Session, $"{nameof(device.Session)} after SetDeviceInfoAsync #1");

    // call SetDeviceInfoAsync in the state that the session has been established
    await device.SetDeviceInfoAsync(new { device_on = false });

    Assert.IsNotNull(device.Session, $"{nameof(device.Session)} after SetDeviceInfoAsync #2");

    Assert.AreEqual(2, requestSequenceNumber, nameof(requestSequenceNumber));
  }

  [Test]
  public async Task SetDeviceInfoAsync_ErrorResponse()
  {
    const int setDeviceInfoErrorCode = 1234;

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, _) => {
        Assert.AreEqual("set_device_info", method, "received request method");
        return (
          KnownErrorCodes.Success,
          new SetDeviceInfoResponse() {
            ErrorCode = setDeviceInfoErrorCode,
            Result = new(),
          }
        );
      },
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.IsNull(device.Session, $"{nameof(device.Session)} before SetDeviceInfoAsync");

    var ex = Assert.ThrowsAsync<TapoErrorResponseException>(
      async () => await device.SetDeviceInfoAsync(new { device_on = true })
    );
    Assert.AreEqual("set_device_info", ex!.RequestMethod);
    Assert.AreEqual(setDeviceInfoErrorCode, ex.RawErrorCode);

    Assert.IsNull(device.Session, $"{nameof(device.Session)} after SetDeviceInfoAsync");
  }
}