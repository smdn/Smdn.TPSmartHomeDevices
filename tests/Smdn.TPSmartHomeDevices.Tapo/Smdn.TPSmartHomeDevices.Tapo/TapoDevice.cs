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
[NonParallelizable]
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

  [Test]
  public new void ToString()
    => ConcreteTapoDeviceCommonTests.TestToString<TapoDevice>();

  [TestCaseSource(typeof(ConcreteTapoDeviceCommonTests), nameof(ConcreteTapoDeviceCommonTests.YieldTestCases_Ctor_ArgumentException))]
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

    Assert.That(device.EndPoint, Is.Not.Null);
    Assert.That(
      await device.ResolveEndPointAsync(),
      Is.EqualTo(new IPEndPoint(IPAddress.Loopback, TapoClient.DefaultPort))
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

    Assert.That(device.EndPoint, Is.Not.Null);
    Assert.That(
      await device.ResolveEndPointAsync(),
      Is.EqualTo(new DnsEndPoint("localhost", TapoClient.DefaultPort))
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

    Assert.That(device.EndPoint, Is.Not.Null);
    Assert.That(
      await device.ResolveEndPointAsync(),
      Is.EqualTo(new IPEndPoint(IPAddress.Loopback, TapoClient.DefaultPort))
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
    var endpoint = new StaticDeviceEndPoint(new DnsEndPoint("localhost", 0));

    using var device = TapoDevice.Create(
      deviceEndPoint: endpoint,
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.That(device.EndPoint, Is.EqualTo(endpoint));
    Assert.That(
      await device.ResolveEndPointAsync(),
      Is.EqualTo(new DnsEndPoint("localhost", TapoClient.DefaultPort))
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

    Assert.That(device.TerminalUuidString, Is.Not.Empty, nameof(device.TerminalUuidString));
    Assert.That(device.TerminalUuidString, Does.Match(ExpectedTerminalUuidRegexPattern), nameof(device.TerminalUuidString));
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

    Assert.That(device.Session, Is.Null, nameof(device.Session));

    Assert.Throws<ObjectDisposedException>(() => Assert.That(device.EndPoint, Is.Not.Null), nameof(device.EndPoint));

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

    Assert.That(
      await device.ResolveEndPointAsync(),
      Is.EqualTo(expectedEndPoint)
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

    Assert.That(ex!.DeviceEndPoint, Is.Not.Null, nameof(ex.DeviceEndPoint));
    Assert.That(ex.DeviceEndPoint, Is.SameAs(endPoint), nameof(ex.DeviceEndPoint));
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
        credential: null,
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

    Assert.That(device.Session, Is.Null);
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

    Assert.That(ex!.DeviceEndPoint, Is.Not.Null, nameof(ex.DeviceEndPoint));
    Assert.That(ex.DeviceEndPoint, Is.SameAs(endPoint), nameof(ex.DeviceEndPoint));

    Assert.That(device.Session, Is.Null);
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
    Assert.That(device.Session, Is.Null);
  }

  [Test]
  public async Task EnsureSessionEstablishedAsync_ResolvedEndPointHasChangedFromPreviousEndPoint()
  {
    const string ReturnTokenFromEndPoint1 = "token1";
    const string ReturnTokenFromEndPoint2 = "token2";

    await using var pseudoDeviceEndPoint1 = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => ReturnTokenFromEndPoint1,
    };
    await using var pseudoDeviceEndPoint2 = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => ReturnTokenFromEndPoint2,
    };

    pseudoDeviceEndPoint1.Start();

    var endPoint = new DynamicDeviceEndPoint(pseudoDeviceEndPoint1.EndPoint);

    using var device = new ConcreteTapoDevice(
      deviceEndPoint: endPoint,
      serviceProvider: services!.BuildServiceProvider()
    );

    await device.EnsureSessionEstablishedAsync();

    Assert.That(device.Session, Is.Not.Null, nameof(device.Session));
    Assert.That(
      device.Session!.Token, Is.EqualTo(ReturnTokenFromEndPoint1),
      nameof(device.Session.Token)
    );

    var prevSession = device.Session;

    // endpoint changed
    pseudoDeviceEndPoint2.Start(exceptPort: pseudoDeviceEndPoint1.EndPoint!.Port);

    endPoint.EndPoint = pseudoDeviceEndPoint2.EndPoint;

    // dispose endpoint #1
    await pseudoDeviceEndPoint1.DisposeAsync();

    await device.EnsureSessionEstablishedAsync();

    Assert.That(device.Session, Is.Not.Null, nameof(device.Session));
    Assert.That(prevSession, Is.Not.SameAs(device.Session), nameof(device.Session));
    Assert.That(
      device.Session.Token, Is.EqualTo(ReturnTokenFromEndPoint2),
      nameof(device.Session.Token)
    );

    // end point should not be marked as invalidated in this scenario
    Assert.That(endPoint.HasInvalidated, Is.False, nameof(endPoint.HasInvalidated));
  }

  private class ConstantTapoSessionProtocolSelector(TapoSessionProtocol? protocol) : TapoSessionProtocolSelector {
    public override TapoSessionProtocol? SelectProtocol(TapoDevice device) => protocol;
  }

  [TestCase(TapoSessionProtocol.SecurePassThrough, TapoSessionProtocol.SecurePassThrough)]
  [TestCase(TapoSessionProtocol.Klap, TapoSessionProtocol.Klap)]
  [TestCase(null, TapoSessionProtocol.SecurePassThrough)] // select default protocol
  public async Task EnsureSessionEstablishedAsync_PerformAuthenticationWithSelectedProtocol(
    TapoSessionProtocol? protocolToBeSelected,
    TapoSessionProtocol expectedProtocol
  )
  {
    var services = new ServiceCollection();

    services.AddTapoCredential("user", "pass");
    services.AddTapoProtocolSelector(new ConstantTapoSessionProtocolSelector(protocolToBeSelected));

    var credentialProvider = services.BuildServiceProvider().GetRequiredService<ITapoCredentialProvider>();

    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token",
      funcGenerateKlapAuthHash: (_, _, authHash) => credentialProvider.GetKlapCredential(null).WriteLocalAuthHash(authHash.Span)
    );

    using var device = new ConcreteTapoDevice(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.That(async () => await device.EnsureSessionEstablishedAsync(), Throws.Nothing);

    Assert.That(device.Session, Is.Not.Null, nameof(device.Session));
    Assert.That(device.Session.Protocol, Is.EqualTo(expectedProtocol));
  }

  [Test]
  public async Task SendRequestAsync_SocketException()
  {
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

    Assert.That(ex!.InnerException, Is.InstanceOf<SocketException>());

    Assert.That(device.Session, Is.Null, nameof(device.Session)); // session must be disposed
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
        Assert.That(method, Is.EqualTo("get_device_info"), "received request method");
        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<NullResult>()
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
    Assert.That(device.Session, Is.Not.Null, nameof(device.Session));

    // dispose endpoint
    // this causes an exception to be raised in the request to the pseudo device,
    // and the exception will be handled as an 'unreachable' event by HandleAsEndPointUnreachableExceptionHandler
    await pseudoDevice.DisposeAsync();

    var ex = Assert.ThrowsAsync<HttpRequestException>(async () => await device.GetDeviceInfoAsync());

    Assert.That(ex!.InnerException, Is.InstanceOf<SocketException>());

    Assert.That(device.Session, Is.Null, nameof(device.Session));
  }

  [Test]
  public Task SendRequestAsync_EndPointUnreachable_DynamicEndPoint_RetrySuccess()
    => SendRequestAsync_EndPointUnreachable_DynamicEndPoint(caseWhenRetrySuccess: true);

  [Test]
  public Task SendRequestAsync_EndPointUnreachable_DynamicEndPoint_RetryFailure()
    => SendRequestAsync_EndPointUnreachable_DynamicEndPoint(caseWhenRetrySuccess: false);

  private async Task SendRequestAsync_EndPointUnreachable_DynamicEndPoint(bool caseWhenRetrySuccess)
  {
    const string ReturnTokenFromEndPoint1 = "token1";
    const string ReturnTokenFromEndPoint2 = "token2";

    await using var pseudoDeviceEndPoint1 = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => ReturnTokenFromEndPoint1,
      FuncGeneratePassThroughResponse = (_, method, _) => {
        Assert.That(method, Is.EqualTo("get_device_info"), "received request method");
        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<NullResult>()
        );
      }
    };
    await using var pseudoDeviceEndPoint2 = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => ReturnTokenFromEndPoint2,
      FuncGeneratePassThroughResponse = (_, method, _) => {
        Assert.That(method, Is.EqualTo("get_device_info"), "received request method");
        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<NullResult>() {
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
    Assert.That(device.Session, Is.Not.Null, nameof(device.Session));

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

      Assert.That(device.Session, Is.Not.Null, nameof(device.Session));
      Assert.That(prevSession, Is.Not.SameAs(device.Session), nameof(device.Session));

      // only first endpoint must have been invalidated
      Assert.That(invalidatedEndPoints.Count, Is.EqualTo(1), nameof(invalidatedEndPoints.Count));
      Assert.That(invalidatedEndPoints[0], Is.EqualTo(pseudoDeviceEndPoint1.EndPoint), nameof(invalidatedEndPoints) + "[0]");
    }
    else {
      Assert.CatchAsync(async () => await device.GetDeviceInfoAsync(), "request #2");

      Assert.That(device.Session, Is.Null, nameof(device.Session));

      // both of two endpoint must have been invalidated
      Assert.That(invalidatedEndPoints.Count, Is.EqualTo(2), nameof(invalidatedEndPoints.Count));

      Assert.That(invalidatedEndPoints[0], Is.EqualTo(pseudoDeviceEndPoint1.EndPoint), nameof(invalidatedEndPoints) + "[0]");
      Assert.That(invalidatedEndPoints[1], Is.EqualTo(pseudoDeviceEndPoint2.EndPoint), nameof(invalidatedEndPoints) + "[1]");
    }

    Assert.That(endPoint.HasInvalidated, Is.True, nameof(endPoint.HasInvalidated));
  }

  [Test]
  public async Task SendRequestAsync_UnexpectedException()
  {
    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token",
      funcGeneratePassThroughResponse: (_, _, _) => (
        KnownErrorCodes.Success,
        new PassThroughResponse<NullResult>() {
          ErrorCode = KnownErrorCodes.Success,
          Result = new(),
        }
      )
    );

    using var device = new ConcreteTapoDevice(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.That(device.Session, Is.Null, nameof(device.Session));

    Assert.ThrowsAsync<NotSupportedException>(
      async () => await device.SendRequestAsync<GetDeviceInfoRequest, PassThroughResponse<NullResult>, int>(
        request: new GetDeviceInfoRequest(),
        composeResult: static resp => throw new NotSupportedException("unexpected exception"),
        cancellationToken: default
      )
    );

    Assert.That(device.Session, Is.Null, nameof(device.Session));
  }

  [Test]
  public async Task SendRequestAsync_ErrorResponse_RetrySuccess()
  {
    const int GetDeviceInfoErrorCode = 1234;
    var request = 0;

    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: _ => $"token-request{request}",
      funcGeneratePassThroughResponse: (_, _, _) => (
        KnownErrorCodes.Success,
        new PassThroughResponse<NullResult>() {
          ErrorCode = request++ == 0 ? GetDeviceInfoErrorCode : KnownErrorCodes.Success,
          Result = new(),
        }
      )
    );

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.That(device.Session, Is.Null, nameof(device.Session));

    Assert.DoesNotThrowAsync(async () => await device.GetDeviceInfoAsync());

    Assert.That(request, Is.EqualTo(2), nameof(request));
    Assert.That(device.Session, Is.Not.Null, nameof(device.Session));
    Assert.That(device.Session!.Token, Is.EqualTo("token-request1"), nameof(device.Session.Token));
  }

  [Test]
  public async Task SendRequestAsync_ErrorResponse_RetryFailedWithErrorResponse()
  {
    const int GetDeviceInfoErrorCode = 1234;
    var request = 0;

    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: _ => $"token-request{request}",
      funcGeneratePassThroughResponse: (_, _, _) => {
        request++;

        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<NullResult>() {
            ErrorCode = GetDeviceInfoErrorCode,
            Result = new(),
          }
        );
      }
    );

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.That(device.Session, Is.Null, nameof(device.Session));

    var ex = Assert.ThrowsAsync<TapoErrorResponseException>(async () => await device.GetDeviceInfoAsync());

    Assert.That(request, Is.EqualTo(2), nameof(request));
    Assert.That(ex!.EndPoint!.Query, Does.Contain("token=token-request1"), nameof(ex.EndPoint.Query));
    Assert.That(device.Session, Is.Null, nameof(device.Session));
  }

  [Test]
  public async Task SendRequestAsync_ResponseWithErrorCodeMinus1301_RetrySuccess()
  {
    var request = 0;

    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: _ => $"token-request{request}",
      funcGeneratePassThroughResponse: (_, _, _) => (
        KnownErrorCodes.Success,
        new PassThroughResponse<NullResult>() {
          ErrorCode = request++ == 0 ? KnownErrorCodes.Minus1301 : KnownErrorCodes.Success,
          Result = new(),
        }
      )
    );

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.That(device.Session, Is.Null, nameof(device.Session));

    Assert.DoesNotThrowAsync(async () => await device.GetDeviceInfoAsync());

    Assert.That(request, Is.EqualTo(2), nameof(request));
    Assert.That(device.Session, Is.Not.Null, nameof(device.Session));
    Assert.That(device.Session!.Token, Is.EqualTo("token-request1"), nameof(device.Session.Token));
  }

  [Test]
  public async Task SendRequestAsync_ResponseWithErrorCodeMinus1301_RetryFailedWithErrorResponse()
  {
    var request = 0;

    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: _ => $"token-request{request}",
      funcGeneratePassThroughResponse: (_, _, _) => {
        request++;

        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<NullResult>() {
            ErrorCode = KnownErrorCodes.Minus1301,
            Result = new(),
          }
        );
      }
    );

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.That(device.Session, Is.Null, nameof(device.Session));

    var ex = Assert.ThrowsAsync<TapoErrorResponseException>(async () => await device.GetDeviceInfoAsync());

    Assert.That(request, Is.EqualTo(2), nameof(request));
    Assert.That(ex!.RawErrorCode, Is.EqualTo(KnownErrorCodes.Minus1301), nameof(ex.RawErrorCode));
    Assert.That(ex.EndPoint!.Query, Does.Contain("token=token-request1"), nameof(ex.EndPoint.Query));
    Assert.That(device.Session, Is.Null, nameof(device.Session));
  }

  [Test]
  public async Task SendRequestAsync_ResponseWithErrorCodeMinus1002_RetryMustNotBeAttempted()
  {
    var request = 0;

    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: _ => $"token-request{request}",
      funcGeneratePassThroughResponse: (_, _, _) => {
        request++;

        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<NullResult>() {
            ErrorCode = KnownErrorCodes.Minus1002,
            Result = new(),
          }
        );
      }
    );

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.That(device.Session, Is.Null, nameof(device.Session));

    var ex = Assert.ThrowsAsync<TapoErrorResponseException>(async () => await device.GetDeviceInfoAsync());

    Assert.That(request, Is.EqualTo(1), nameof(request)); // retry not performed
    Assert.That(ex!.RawErrorCode, Is.EqualTo(KnownErrorCodes.Minus1002), nameof(ex.RawErrorCode));
    Assert.That(ex.EndPoint!.Query, Does.Contain("token=token-request0"), nameof(ex.EndPoint.Query));
    Assert.That(device.Session, Is.Null, nameof(device.Session));
  }

  [Test]
  public Task SendRequestAsync_Timeout_RetrySuccess_SetTimeoutWithIHttpClientFactory()
    => SendRequestAsync_Timeout_RetrySuccess(setTimeoutViaIHttpClientFactory: true);

  [Test]
  public Task SendRequestAsync_Timeout_RetrySuccess_SetTimeoutWithDeviceTimeoutProperty()
    => SendRequestAsync_Timeout_RetrySuccess(setTimeoutViaIHttpClientFactory: false);

  private async Task SendRequestAsync_Timeout_RetrySuccess(bool setTimeoutViaIHttpClientFactory)
  {
    if (TestEnvironment.IsRunningOnGitHubActionsMacOSRunner && setTimeoutViaIHttpClientFactory) {
      Assert.Ignore("Skip this test case on GitHub Actions macOS runners because it takes about 2 minutes for the timeout to trigger.");
      return;
      // [dotnet test output]
      // passed SendRequestAsync_Timeout_RetrySuccess_SetTimeoutWithIHttpClientFactory (2m 15s 399ms)
      //   from /Users/runner/work/Smdn.TPSmartHomeDevices/Smdn.TPSmartHomeDevices/tests/Smdn.TPSmartHomeDevices.Tapo/bin/Debug/net8.0/Smdn.TPSmartHomeDevices.Tapo.Tests.dll (net8.0|arm64)
      // passed SendRequestAsync_Timeout_RetrySuccess_SetTimeoutWithIHttpClientFactory (2m 15s 236ms)
      //   from /Users/runner/work/Smdn.TPSmartHomeDevices/Smdn.TPSmartHomeDevices/tests/Smdn.TPSmartHomeDevices.Tapo/bin/Debug/net10.0/Smdn.TPSmartHomeDevices.Tapo.Tests.dll (net10.0|arm64)
    }

    const int MaxRetry = 3;

    using var cts = new CancellationTokenSource();
    var pseudoDevices = new List<PseudoTapoDevice>(capacity: MaxRetry);

    for (var attempt = 0; attempt < MaxRetry; attempt++) {
      var state = Tuple.Create(
        attempt < MaxRetry - 1
          ? TimeSpan.FromSeconds(60)
          : TimeSpan.Zero,
        $"token-request{attempt}"
      );

      pseudoDevices.Add(
        new(state: state) {
          FuncGenerateToken = static session => (session.State as Tuple<TimeSpan, string>)!.Item2,
          FuncGeneratePassThroughResponse = (session, _, _) => {
            // perform latency
            DelayUtils.Delay((session.State as Tuple<TimeSpan, string>)!.Item1, cts.Token);

            return (
              KnownErrorCodes.Success,
              new PassThroughResponse<NullResult>() {
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
      => TimeSpan.FromMilliseconds(200);

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
      Assert.That(device.Session, Is.Null, nameof(device.Session));

      Assert.DoesNotThrowAsync(async () => await device.GetDeviceInfoAsync());

      Assert.That(device.Session, Is.Not.Null, nameof(device.Session));
      Assert.That(device.Session!.Token, Is.EqualTo("token-request2"), nameof(device.Session.Token));
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
    const int MaxRetry = 3;

    using var cts = new CancellationTokenSource();
    var pseudoDevices = new List<PseudoTapoDevice>(capacity: MaxRetry);

    for (var attempt = 0; attempt < MaxRetry; attempt++) {
      var state = Tuple.Create(
        TimeSpan.FromSeconds(60),
        $"token-request{attempt}"
      );

      pseudoDevices.Add(
        new(state: state) {
          FuncGenerateToken = static session => (session.State as Tuple<TimeSpan, string>)!.Item2,
          FuncGeneratePassThroughResponse = (session, _, _) => {
            // perform latency
            DelayUtils.Delay((session.State as Tuple<TimeSpan, string>)!.Item1, cts.Token);

            return (
              KnownErrorCodes.Success,
              new PassThroughResponse<NullResult>() {
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

    device.Timeout = TimeSpan.FromMilliseconds(200);

    try {
      Assert.That(device.Session, Is.Null, nameof(device.Session));

      var ex = Assert.ThrowsAsync<TapoProtocolException>(async () => await device.GetDeviceInfoAsync());

      Assert.That(ex!.InnerException, Is.InstanceOf<TimeoutException>(), nameof(ex.InnerException));

      Assert.That(device.Session, Is.Null, nameof(device.Session));
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

    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token",
      funcGeneratePassThroughResponse: (_, passThroughMethod, _) => {
        ctsRequest.Cancel();

        return new(
          KnownErrorCodes.Success,
          new PassThroughResponse<NullResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new(),
          }
        );
      }
    );

    services!.AddSingleton<TapoDeviceExceptionHandler>(
      // asserts that the OperationCanceledException must not be handled
      new AssertOperationCanceledMustNotBeHandledExceptionHandler()
    );

    using var device = new ConcreteTapoDevice(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.That(device.Session, Is.Null, nameof(device.Session));

    var ex = Assert.CatchAsync(
      async () => await device.GetDeviceInfoAsync(cancellationToken: ctsRequest.Token)
    );

    Assert.That(ex, Is.InstanceOf<OperationCanceledException>().Or.InstanceOf<TaskCanceledException>());

    Assert.That(device.Session, Is.Null, nameof(device.Session));
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

    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token",
      funcGeneratePassThroughResponse: (_, method, _) => {
        Assert.That(method, Is.EqualTo("get_device_info"), "received request method");
        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<GetDeviceInfoResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new() {
              // response #0: IsOn = true
              // response #1: IsOn = false
              IsOn = requestSequenceNumber++ == 0,
            },
          }
        );
      }
    );

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.That(device.Session, Is.Null, nameof(device.Session));

    // call GetDeviceInfoAsync in the state that the session has not been established
    var deviceInfo0 = await device.GetDeviceInfoAsync<GetDeviceInfoResult>();

    Assert.That(deviceInfo0.IsOn, Is.True, nameof(deviceInfo0.IsOn));
    Assert.That(device.Session, Is.Not.Null, nameof(device.Session));

    // call GetDeviceInfoAsync in the state that the session has been established
    var deviceInfo1 = await device.GetDeviceInfoAsync<GetDeviceInfoResult>();

    Assert.That(deviceInfo1.IsOn, Is.False, nameof(deviceInfo1.IsOn));
    Assert.That(device.Session, Is.Not.Null, nameof(device.Session));
  }

  [Test]
  public async Task GetDeviceInfoAsync_ComposeResult()
  {
    const string DeviceId = "device-id";
    const bool IsOn = true;

    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token",
      funcGeneratePassThroughResponse: (_, method, _) => {
        Assert.That(method, Is.EqualTo("get_device_info"), "received request method");
        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<GetDeviceInfoResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new() {
              IsOn = IsOn,
              Id = DeviceId,
            },
          }
        );
      }
    );

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    var (resultIsOn, resultId) = await device.GetDeviceInfoAsync<GetDeviceInfoResult, (bool, string?)>(
      composeResult: result => (result.IsOn, result.Id)
    );

    Assert.That(resultIsOn, Is.EqualTo(IsOn), nameof(resultIsOn));
    Assert.That(resultId, Is.EqualTo(DeviceId), nameof(resultId));
  }

  [Test]
  public async Task GetDeviceInfoAsync_ComposeResult_Null()
  {
    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token"
    );

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.That(device.Session, Is.Null, nameof(device.Session));

    Assert.ThrowsAsync<ArgumentNullException>(
      async () => await device.GetDeviceInfoAsync<GetDeviceInfoResult, bool>(composeResult: null!)
    );

    Assert.That(device.Session, Is.Null, nameof(device.Session));
  }

  private readonly struct GetDeviceInfoOnOffStateResult {
    [JsonPropertyName("device_on")]
    public bool IsOn { get; init; }
  }

  [Test]
  public async Task GetDeviceInfoAsync_ResponseWithExtraData()
  {
    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token",
      funcGeneratePassThroughResponse: static (_, _, _) => {
        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<GetDeviceInfoResult>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = new() {
              IsOn = true,
              Id = "device-id", // as an extra data
            },
          }
        );
      }
    );

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.DoesNotThrowAsync(async () => await device.GetDeviceInfoAsync<GetDeviceInfoOnOffStateResult>());
  }

  [Test]
  public async Task GetDeviceInfoAsync_ErrorResponse()
  {
    const int GetDeviceInfoErrorCode = 1234;

    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token",
      funcGeneratePassThroughResponse: static (_, _, _) => (
        KnownErrorCodes.Success,
        new PassThroughResponse<NullResult>() {
          ErrorCode = GetDeviceInfoErrorCode,
          Result = new(),
        }
      )
    );

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.That(device.Session, Is.Null, $"{nameof(device.Session)} before GetDeviceInfoAsync");

    var ex = Assert.ThrowsAsync<TapoErrorResponseException>(
      async () => await device.GetDeviceInfoAsync()
    );
    Assert.That(ex!.RequestMethod, Is.EqualTo("get_device_info"));
    Assert.That(ex.RawErrorCode, Is.EqualTo(GetDeviceInfoErrorCode));
    Assert.That(device.Session, Is.Null, $"{nameof(device.Session)} after GetDeviceInfoAsync");
  }

  [Test]
  public async Task SetDeviceInfoAsync()
  {
    var requestSequenceNumber = 0;

    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token",
      funcGeneratePassThroughResponse: (_, method, requestParams) => {
        Assert.That(method, Is.EqualTo("set_device_info"), "received request method");

        switch (requestSequenceNumber++) {
          case 0:
            Assert.That(requestParams.GetProperty("device_on")!.GetBoolean(), Is.True);
            break;

          case 1:
            Assert.That(requestParams.GetProperty("device_on")!.GetBoolean(), Is.False);
            break;

          default:
            throw new InvalidOperationException("unexpected request");
        }

        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<None>() {
            ErrorCode = KnownErrorCodes.Success,
            Result = default,
          }
        );
      }
    );

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.That(device.Session, Is.Null, $"{nameof(device.Session)} before SetDeviceInfoAsync");

    // call SetDeviceInfoAsync in the state that the session has not been established
    await device.SetDeviceInfoAsync(new { device_on = true });

    Assert.That(device.Session, Is.Not.Null, $"{nameof(device.Session)} after SetDeviceInfoAsync #1");

    // call SetDeviceInfoAsync in the state that the session has been established
    await device.SetDeviceInfoAsync(new { device_on = false });

    Assert.That(device.Session, Is.Not.Null, $"{nameof(device.Session)} after SetDeviceInfoAsync #2");

    Assert.That(requestSequenceNumber, Is.EqualTo(2), nameof(requestSequenceNumber));
  }

  [Test]
  public async Task SetDeviceInfoAsync_ErrorResponse()
  {
    const int SetDeviceInfoErrorCode = 1234;

    var pseudoDevice = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token",
      funcGeneratePassThroughResponse: (_, method, _) => {
        Assert.That(method, Is.EqualTo("set_device_info"), "received request method");
        return (
          KnownErrorCodes.Success,
          new PassThroughResponse<None>() {
            ErrorCode = SetDeviceInfoErrorCode,
            Result = default,
          }
        );
      }
    );

    using var device = TapoDevice.Create(
      deviceEndPoint: pseudoDevice.GetEndPoint(),
      serviceProvider: services!.BuildServiceProvider()
    );

    Assert.That(device.Session, Is.Null, $"{nameof(device.Session)} before SetDeviceInfoAsync");

    var ex = Assert.ThrowsAsync<TapoErrorResponseException>(
      async () => await device.SetDeviceInfoAsync(new { device_on = true })
    );
    Assert.That(ex!.RequestMethod, Is.EqualTo("set_device_info"));
    Assert.That(ex.RawErrorCode, Is.EqualTo(SetDeviceInfoErrorCode));

    Assert.That(device.Session, Is.Null, $"{nameof(device.Session)} after SetDeviceInfoAsync");
  }
}
