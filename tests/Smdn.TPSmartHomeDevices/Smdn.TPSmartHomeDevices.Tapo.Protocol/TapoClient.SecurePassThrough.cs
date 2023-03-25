// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

partial class TapoClientTests {
  [Test]
  public async Task SendRequestAsync()
  {
    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = static (_, _, _) => (
        ErrorCode.Success,
        new GetDeviceInfoResponse() {
          ErrorCode = ErrorCode.Success,
          Result = new(),
        }
      )
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider
      )
    );

    GetDeviceInfoResponse? nullableResponse = null;

    Assert.DoesNotThrowAsync(
      async () => nullableResponse = await client.SendRequestAsync<GetDeviceInfoRequest, GetDeviceInfoResponse>()
    );

    Assert.IsNotNull(nullableResponse);

    var response = nullableResponse!.Value;

    Assert.AreEqual(ErrorCode.Success, response.ErrorCode, nameof(response.ErrorCode));
    Assert.IsNotNull(response.Result, nameof(response.Result));
  }

  [Test]
  public void SendRequestAsync_SessionNotEstablished()
  {
    using var client = new TapoClient(
      endPoint: new DnsEndPoint("localhost", 0)
    );

    Assert.ThrowsAsync<InvalidOperationException>(
      async () => await client.SendRequestAsync<GetDeviceInfoRequest, GetDeviceInfoResponse>()
    );
    Assert.Throws<InvalidOperationException>(
      () => client.SendRequestAsync<GetDeviceInfoRequest, GetDeviceInfoResponse>()
    );
  }

  [Test]
  public async Task SendRequestAsync_ErrorResponse_PassThroughResponse()
  {
    const ErrorCode errorCode = (ErrorCode)9999;

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = static (_, _, _) => (
        ErrorCode.Success,
        new GetDeviceInfoResponse() {
          ErrorCode = errorCode,
          Result = new(),
        }
      )
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider
      )
    );

    var ex = Assert.ThrowsAsync<TapoErrorResponseException>(
      async () => await client.SendRequestAsync<GetDeviceInfoRequest, GetDeviceInfoResponse>()
    );

    Assert.AreEqual(errorCode, ex!.ErrorCode, nameof(ex.ErrorCode));
    Assert.AreEqual(new GetDeviceInfoRequest().Method, ex.RequestMethod, nameof(ex.RequestMethod));
    Assert.AreEqual(new Uri(device.EndPointUri!, client.Session.RequestPathAndQuery), ex.EndPoint, nameof(ex.EndPoint));
  }

  [Test]
  public async Task SendRequestAsync_ErrorResponse_SecurePassThroughResponse()
  {
    const ErrorCode errorCode = (ErrorCode)9999;

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = static (_, _, _) => (
        errorCode,
        new GetDeviceInfoResponse() {
          ErrorCode = ErrorCode.Success,
          Result = new(),
        }
      )
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider
      )
    );

    var ex = Assert.ThrowsAsync<TapoErrorResponseException>(
      async () => await client.SendRequestAsync<GetDeviceInfoRequest, GetDeviceInfoResponse>()
    );

    Assert.AreEqual(errorCode, ex!.ErrorCode, nameof(ex.ErrorCode));
    Assert.AreEqual("securePassthrough", ex.RequestMethod, nameof(ex.RequestMethod));
    Assert.AreEqual(new Uri(device.EndPointUri!, client.Session.RequestPathAndQuery), ex.EndPoint, nameof(ex.EndPoint));
  }
}
