// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

[TestFixture]
public class TapoDeviceTests {
  private ServiceCollection? services;

  [OneTimeSetUp]
  public void SetUp()
  {
    services = new ServiceCollection();

    services.AddTapoBase64EncodedCredential(
      base64UserNameSHA1Digest: Convert.ToBase64String(Encoding.UTF8.GetBytes("user")),
      base64Password: Convert.ToBase64String(Encoding.UTF8.GetBytes("pass"))
    );
  }

  private static System.Collections.IEnumerable YiledTestCases_Create_ArgumentNull()
  {
    yield return new object[] {
      new TestDelegate(() => TapoDevice.Create(deviceEndPointProvider: null!)),
      "deviceEndPointProvider"
    };

    yield return new object[] {
      new TestDelegate(() => TapoDevice.Create(deviceAddress: null!, email: "user@mail.test", password: "pass")),
      "deviceAddress"
    };
    yield return new object[] {
      new TestDelegate(() => TapoDevice.Create(deviceAddress: IPAddress.Loopback, email: null!, password: "pass")),
      "email"
    };
    yield return new object[] {
      new TestDelegate(() => TapoDevice.Create(deviceAddress: IPAddress.Loopback, email: "user@mail.test", password: null!)),
      "password"
    };

    yield return new object[] {
      new TestDelegate(() => TapoDevice.Create(hostName: null!, email: "user@mail.test", password: "pass")),
      "hostName"
    };
    yield return new object[] {
      new TestDelegate(() => TapoDevice.Create(hostName: "localhost", email: null!, password: "pass")),
      "email"
    };
    yield return new object[] {
      new TestDelegate(() => TapoDevice.Create(hostName: "localhost", email: "user@mail.test", password: null!)),
      "password"
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
    using var device = TapoDevice.Create(
      deviceAddress: IPAddress.Loopback,
      "user@mail.test",
      "password"
    );

    Assert.AreEqual(
      new IPEndPoint(IPAddress.Loopback, 80),
      await device.ResolveEndPointAsync()
    );
  }

  [Test]
  public async Task Create_WithDeviceHostName()
  {
    using var device = TapoDevice.Create(
      hostName: "localhost",
      "user@mail.test",
      "password"
    );

    Assert.AreEqual(
      new DnsEndPoint("localhost", 80),
      await device.ResolveEndPointAsync()
    );
  }

  [Test]
  public async Task Create_WithEndPointProvider()
  {
    using var device = TapoDevice.Create(
      deviceEndPointProvider: new StaticDeviceEndPointProvider(new DnsEndPoint("localhost", 0))
    );

    Assert.AreEqual(
      new DnsEndPoint("localhost", TapoClient.DefaultPort),
      await device.ResolveEndPointAsync()
    );
  }

  private const string ExpectedTerminalUuidRegexPattern = @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$";

  [TestCase("5b5ab773-e14a-4d2b-bb00-1464a64da330")]
  [TestCase("00000000-0000-0000-0000-000000000000")] // NIL
  public void Create_TerminalUuid_UserSupplied(string uuid)
  {
    using var device = TapoDevice.Create(
      deviceEndPointProvider: new ThrowExceptionDeviceEndPointProvider(),
      terminalUuid: new Guid(uuid)
    );

    Assert.IsNotEmpty(device.TerminalUuidString, nameof(device.TerminalUuidString));
    StringAssert.IsMatch(ExpectedTerminalUuidRegexPattern, device.TerminalUuidString, nameof(device.TerminalUuidString));
  }

  [Test]
  public void Create_TerminalUuid_AutoGenerate()
  {
    using var device = TapoDevice.Create(
      deviceAddress: IPAddress.Loopback,
      "user@mail.test",
      "password"
    );

    Assert.IsNotEmpty(device.TerminalUuidString, nameof(device.TerminalUuidString));
    StringAssert.IsMatch(ExpectedTerminalUuidRegexPattern, device.TerminalUuidString, nameof(device.TerminalUuidString));
  }

  private readonly struct NullParameters { }

  [Test]
  public void Dispose()
  {
    using var device = TapoDevice.Create(
      deviceAddress: IPAddress.Loopback,
      "user@mail.test",
      "password"
    );

    Assert.DoesNotThrow(device.Dispose, "dispose");
    Assert.DoesNotThrow(device.Dispose, "dispose again");

    Assert.IsNull(device.Session, nameof(device.Session));

    Assert.ThrowsAsync<ObjectDisposedException>(async () => await device.ResolveEndPointAsync(), nameof(device.ResolveEndPointAsync));
    Assert.Throws<ObjectDisposedException>(() => device.ResolveEndPointAsync(), nameof(device.ResolveEndPointAsync));

    Assert.ThrowsAsync<ObjectDisposedException>(async () => await device.GetDeviceInfoAsync(), nameof(device.GetDeviceInfoAsync));
    Assert.Throws<ObjectDisposedException>(() => device.GetDeviceInfoAsync(), nameof(device.GetDeviceInfoAsync));

    Assert.ThrowsAsync<ObjectDisposedException>(async () => await device.SetDeviceInfoAsync(default(NullParameters)), nameof(device.SetDeviceInfoAsync));
    Assert.Throws<ObjectDisposedException>(() => device.SetDeviceInfoAsync(default(NullParameters)), nameof(device.SetDeviceInfoAsync));

    Assert.ThrowsAsync<ObjectDisposedException>(async () => await device.TurnOnAsync(), nameof(device.TurnOnAsync));
    Assert.Throws<ObjectDisposedException>(() => device.TurnOnAsync(), nameof(device.TurnOnAsync));
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

    using var device = TapoDevice.Create(
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
    using var device = TapoDevice.Create(
      deviceEndPointProvider: new ThrowExceptionDeviceEndPointProvider()
    );

    cts.Cancel();

    Assert.ThrowsAsync<TaskCanceledException>(
      async () => await device.ResolveEndPointAsync(cts.Token)
    );
  }

  private class ConcreteTapoDevice : TapoDevice {
    public ConcreteTapoDevice(
      IDeviceEndPointProvider deviceEndPointProvider,
      IServiceProvider? serviceProvider = null
    )
      : base(
        deviceEndPointProvider: deviceEndPointProvider,
        serviceProvider: serviceProvider
      )
    {
    }

    public new ValueTask EnsureSessionEstablishedAsync(CancellationToken cancellationToken = default)
      => base.EnsureSessionEstablishedAsync(cancellationToken: cancellationToken);

    public new Task<TResult> SendRequestAsync<TRequest, TResponse, TResult>(
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
      deviceEndPointProvider: new ThrowExceptionDeviceEndPointProvider()
    );

    device.Dispose();

    Assert.ThrowsAsync<ObjectDisposedException>(async () => await device.EnsureSessionEstablishedAsync());

    Assert.IsNull(device.Session);
  }

  [Test]
  public void EnsureSessionEstablishedAsync_FailedToResolveDeviceEndPoint()
  {
    var provider = new UnresolvedDeviceEndPointProvider();

    using var device = new ConcreteTapoDevice(
      deviceEndPointProvider: provider
    );

    var ex = Assert.ThrowsAsync<DeviceEndPointResolutionException>(async () => await device.EnsureSessionEstablishedAsync());

    Assert.IsNotNull(ex!.EndPointProvider, nameof(ex.EndPointProvider));
    Assert.AreSame(provider, ex.EndPointProvider, nameof(ex.EndPointProvider));

    Assert.IsNull(device.Session);
  }

  [Test]
  public void EnsureSessionEstablishedAsync_CancellationRequestedAfterResolution()
  {
    using var cts = new CancellationTokenSource();
    using var device = new ConcreteTapoDevice(
      deviceEndPointProvider: new RequestCancellationAfterReturnDeviceEndPointProvider(
        cancellationTokenSource: cts,
        endPoint: new IPEndPoint(IPAddress.Loopback, 0)
      ),
      serviceProvider: services.BuildServiceProvider()
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

    var provider = new DynamicDeviceEndPointProvider(pseudoDeviceEndPoint1.EndPoint);

    using var device = new ConcreteTapoDevice(
      deviceEndPointProvider: provider,
      serviceProvider: services.BuildServiceProvider()
    );

    await device.EnsureSessionEstablishedAsync();

    Assert.IsNotNull(device.Session, nameof(device.Session));
    Assert.AreEqual(
      returnTokenFromEndPoint1,
      device.Session.Token,
      nameof(device.Session.Token)
    );

    var prevSession = device.Session;

    // endpoint changed
    pseudoDeviceEndPoint2.Start(exceptPort: pseudoDeviceEndPoint1.EndPoint.Port);

    provider.EndPoint = pseudoDeviceEndPoint2.EndPoint;

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
  }

  [Test]
  public async Task SendRequestAsync_SocketException()
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
    };

    pseudoDevice.Start();

    using var device = new ConcreteTapoDevice(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    await device.EnsureSessionEstablishedAsync();

    // reuse endpoints to create unresponding states
    var endPoint = pseudoDevice.EndPoint;

    await pseudoDevice.DisposeAsync();

    using var listener = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

    listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
    listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

    listener.Bind(endPoint); // bind but does not listen

    // send request
    var ex = Assert.ThrowsAsync<HttpRequestException>(async () => await device.GetDeviceInfoAsync());

    Assert.IsInstanceOf<SocketException>(ex!.InnerException);

    Assert.IsNull(device.Session, nameof(device.Session), "session must be disposed");
  }

  [Test]
  public async Task SendRequestAsync_EndPointUnreachable_StaticEndPoint()
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, _) => {
        Assert.AreEqual("get_device_info", method, "received request method");
        return (
          ErrorCode.Success,
          new GetDeviceInfoResponse()
        );
      }
    };

    pseudoDevice.Start();

    var endPoint = pseudoDevice.GetEndPointProvider();

    Assert.IsTrue(endPoint.IsStaticEndPoint, nameof(endPoint.IsStaticEndPoint));

    using var device = new ConcreteTapoDevice(
      deviceEndPointProvider: endPoint,
      serviceProvider: services.BuildServiceProvider()
    );

    Assert.DoesNotThrowAsync(async () => await device.GetDeviceInfoAsync());
    Assert.IsNotNull(device.Session, nameof(device.Session));

    // set to unreachable state
    await pseudoDevice.StopAsync();

    var ex = Assert.ThrowsAsync<HttpRequestException>(async () => await device.GetDeviceInfoAsync());

    Assert.IsInstanceOf<SocketException>(ex!.InnerException);

    Assert.IsNull(device.Session, nameof(device.Session));
  }

  [Test]
  public async Task SendRequestAsync_EndPointUnreachable_DynamicEndPoint()
  {
    const string returnTokenFromEndPoint1 = "token1";
    const string returnTokenFromEndPoint2 = "token2";

    await using var pseudoDeviceEndPoint1 = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => returnTokenFromEndPoint1,
      FuncGeneratePassThroughResponse = (_, method, _) => {
        Assert.AreEqual("get_device_info", method, "received request method");
        return (
          ErrorCode.Success,
          new GetDeviceInfoResponse()
        );
      }
    };
    await using var pseudoDeviceEndPoint2 = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => returnTokenFromEndPoint2,
      FuncGeneratePassThroughResponse = (_, method, _) => {
        Assert.AreEqual("get_device_info", method, "received request method");
        return (
          ErrorCode.Success,
          new GetDeviceInfoResponse()
        );
      }
    };

    pseudoDeviceEndPoint1.Start();

    var provider = new DynamicDeviceEndPointProvider(pseudoDeviceEndPoint1.EndPoint);

    using var device = new ConcreteTapoDevice(
      deviceEndPointProvider: provider,
      serviceProvider: services.BuildServiceProvider()
    );

    Assert.DoesNotThrowAsync(async () => await device.GetDeviceInfoAsync(), "request #1");
    Assert.IsNotNull(device.Session, nameof(device.Session));

    var prevSession = device.Session;

    // endpoint changed
    pseudoDeviceEndPoint2.Start(exceptPort: pseudoDeviceEndPoint1.EndPoint.Port);

    provider.EndPoint = pseudoDeviceEndPoint2.EndPoint;

    // dispose endpoint #1
    await pseudoDeviceEndPoint1.DisposeAsync();

    Assert.DoesNotThrowAsync(async () => await device.GetDeviceInfoAsync(), "request #2");
    Assert.IsNotNull(device.Session, nameof(device.Session));
    Assert.AreNotSame(device.Session, prevSession, nameof(device.Session));
  }

  [Test]
  public async Task SendRequestAsync_UnexpectedException()
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, _, _) => (
        ErrorCode.Success,
        new GetDeviceInfoResponse() {
          ErrorCode = ErrorCode.Success,
          Result = new(),
        }
      ),
    };

    pseudoDevice.Start();

    using var device = new ConcreteTapoDevice(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    Assert.IsNull(device.Session, nameof(device.Session));

    Assert.ThrowsAsync<NotSupportedException>(
      async () => await device.SendRequestAsync<GetDeviceInfoRequest, GetDeviceInfoResponse, int>(
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
    const ErrorCode getDeviceInfoErrorCode = (ErrorCode)1234;
    var request = 0;

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = _ => $"token-request{request}",
      FuncGeneratePassThroughResponse = (_, _, _) => (
        ErrorCode.Success,
        new GetDeviceInfoResponse() {
          ErrorCode = request++ == 0 ? getDeviceInfoErrorCode : ErrorCode.Success,
          Result = new(),
        }
      ),
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    Assert.IsNull(device.Session, nameof(device.Session));

    Assert.DoesNotThrowAsync(async () => await device.GetDeviceInfoAsync());

    Assert.AreEqual(2, request, nameof(request));
    Assert.IsNotNull(device.Session, nameof(device.Session));
    Assert.AreEqual("token-request1", device.Session.Token, nameof(device.Session));
  }

  [Test]
  public async Task SendRequestAsync_ErrorResponse_RetryFailedWithErrorResponse()
  {
    const ErrorCode getDeviceInfoErrorCode = (ErrorCode)1234;
    var request = 0;

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = _ => $"token-request{request}",
      FuncGeneratePassThroughResponse = (_, _, _) => {
        request++;

        return (
          ErrorCode.Success,
          new GetDeviceInfoResponse() {
            ErrorCode = getDeviceInfoErrorCode,
            Result = new(),
          }
        );
      },
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    Assert.IsNull(device.Session, nameof(device.Session));

    var ex = Assert.ThrowsAsync<TapoErrorResponseException>(async () => await device.GetDeviceInfoAsync());

    Assert.AreEqual(2, request, nameof(request));
    StringAssert.Contains("token=token-request1", ex.EndPoint.Query, nameof(ex.EndPoint.Query));
    Assert.IsNull(device.Session, nameof(device.Session));
  }

  [Test]
  public async Task SendRequestAsync_ResponseWithErrorCodeMinus1301_RetrySuccess()
  {
    const ErrorCode errorCodeMinus1301 = (ErrorCode)(-1301);
    var request = 0;

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = _ => $"token-request{request}",
      FuncGeneratePassThroughResponse = (_, _, _) => (
        ErrorCode.Success,
        new GetDeviceInfoResponse() {
          ErrorCode = request++ == 0 ? errorCodeMinus1301 : ErrorCode.Success,
          Result = new(),
        }
      ),
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    Assert.IsNull(device.Session, nameof(device.Session));

    Assert.DoesNotThrowAsync(async () => await device.GetDeviceInfoAsync());

    Assert.AreEqual(2, request, nameof(request));
    Assert.IsNotNull(device.Session, nameof(device.Session));
    Assert.AreEqual("token-request1", device.Session.Token, nameof(device.Session));
  }

  [Test]
  public async Task SendRequestAsync_ResponseWithErrorCodeMinus1301_RetryFailedWithErrorResponse()
  {
    const ErrorCode errorCodeMinus1301 = (ErrorCode)(-1301);
    var request = 0;

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = _ => $"token-request{request}",
      FuncGeneratePassThroughResponse = (_, _, _) => {
        request++;

        return (
          ErrorCode.Success,
          new GetDeviceInfoResponse() {
            ErrorCode = errorCodeMinus1301,
            Result = new(),
          }
        );
      },
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    Assert.IsNull(device.Session, nameof(device.Session));

    var ex = Assert.ThrowsAsync<TapoErrorResponseException>(async () => await device.GetDeviceInfoAsync());

    Assert.AreEqual(2, request, nameof(request));
    Assert.AreEqual(errorCodeMinus1301, ex.ErrorCode, nameof(ex.ErrorCode));
    StringAssert.Contains("token=token-request1", ex.EndPoint.Query, nameof(ex.EndPoint.Query));
    Assert.IsNull(device.Session, nameof(device.Session));
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
          ErrorCode.Success,
          new GetDeviceInfoResponse() {
            ErrorCode = ErrorCode.Success,
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
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    Assert.IsNull(device.Session, nameof(device.Session));

    // call GetDeviceInfoAsync in the state that the session has not been established
    var deviceInfo0 = await device.GetDeviceInfoAsync();

    Assert.IsNotNull(deviceInfo0, nameof(deviceInfo0));
    Assert.IsTrue(deviceInfo0.IsOn, nameof(deviceInfo0.IsOn));
    Assert.IsNotNull(device.Session, nameof(device.Session));

    // call GetDeviceInfoAsync in the state that the session has been established
    var deviceInfo1 = await device.GetDeviceInfoAsync();

    Assert.IsNotNull(deviceInfo1, nameof(deviceInfo1));
    Assert.IsFalse(deviceInfo1.IsOn, nameof(deviceInfo1.IsOn));
    Assert.AreNotSame(deviceInfo0, deviceInfo1, nameof(deviceInfo1));
    Assert.IsNotNull(device.Session, nameof(device.Session));
  }

  [Test]
  public async Task GetDeviceInfoAsync_ResponseWithExtraData()
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = static (_, _, _) => {
        return (
          ErrorCode.Success,
          new GetDeviceInfoResponse() {
            ErrorCode = ErrorCode.Success,
            Result = new() {
              // cannnot construct with extra data
            },
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    var deviceInfo = await device.GetDeviceInfoAsync();

    Assert.IsNotNull(deviceInfo, nameof(deviceInfo));
  }

  [Test]
  public async Task GetDeviceInfoAsync_ErrorResponse()
  {
    const ErrorCode getDeviceInfoErrorCode = (ErrorCode)1234;

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = static (_, _, _) => (
        ErrorCode.Success,
        new GetDeviceInfoResponse() {
          ErrorCode = getDeviceInfoErrorCode,
          Result = new(),
        }
      ),
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    Assert.IsNull(device.Session, $"{nameof(device.Session)} before GetDeviceInfoAsync");

    var ex = Assert.ThrowsAsync<TapoErrorResponseException>(
      async () => await device.GetDeviceInfoAsync()
    );
    Assert.AreEqual("get_device_info", ex!.RequestMethod);
    Assert.AreEqual(getDeviceInfoErrorCode, ex.ErrorCode);
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
          ErrorCode.Success,
          new SetDeviceInfoResponse() {
            ErrorCode = ErrorCode.Success,
            Result = new(),
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
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
    const ErrorCode setDeviceInfoErrorCode = (ErrorCode)1234;

    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, _) => {
        Assert.AreEqual("set_device_info", method, "received request method");
        return (
          ErrorCode.Success,
          new SetDeviceInfoResponse() {
            ErrorCode = setDeviceInfoErrorCode,
            Result = new(),
          }
        );
      },
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    Assert.IsNull(device.Session, $"{nameof(device.Session)} before SetDeviceInfoAsync");

    var ex = Assert.ThrowsAsync<TapoErrorResponseException>(
      async () => await device.SetDeviceInfoAsync(new { device_on = true })
    );
    Assert.AreEqual("set_device_info", ex!.RequestMethod);
    Assert.AreEqual(setDeviceInfoErrorCode, ex.ErrorCode);

    Assert.IsNull(device.Session, $"{nameof(device.Session)} after SetDeviceInfoAsync");
  }

  [Test]
  public async Task TurnOnAsync()
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        Assert.AreEqual("set_device_info", method, "received request method");
        Assert.IsTrue(requestParams.GetProperty("device_on")!.GetBoolean());
        return (
          ErrorCode.Success,
          new SetDeviceInfoResponse() {
            ErrorCode = ErrorCode.Success,
            Result = new(),
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    await device.TurnOnAsync();
  }

  [Test]
  public async Task TurnOffAsync()
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        Assert.AreEqual("set_device_info", method, "received request method");
        Assert.IsFalse(requestParams.GetProperty("device_on")!.GetBoolean());
        return (
          ErrorCode.Success,
          new SetDeviceInfoResponse() {
            ErrorCode = ErrorCode.Success,
            Result = new(),
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    await device.TurnOffAsync();
  }

  [TestCase(true)]
  [TestCase(false)]
  public async Task SetOnOffStateAsync(bool newState)
  {
    await using var pseudoDevice = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = (_, method, requestParams) => {
        Assert.AreEqual("set_device_info", method, "received request method");
        Assert.AreEqual(newState, requestParams.GetProperty("device_on")!.GetBoolean());
        return (
          ErrorCode.Success,
          new SetDeviceInfoResponse() {
            ErrorCode = ErrorCode.Success,
            Result = new(),
          }
        );
      }
    };

    pseudoDevice.Start();

    using var device = TapoDevice.Create(
      deviceEndPointProvider: pseudoDevice.GetEndPointProvider(),
      serviceProvider: services.BuildServiceProvider()
    );

    await device.SetOnOffStateAsync(newState);
  }
}
