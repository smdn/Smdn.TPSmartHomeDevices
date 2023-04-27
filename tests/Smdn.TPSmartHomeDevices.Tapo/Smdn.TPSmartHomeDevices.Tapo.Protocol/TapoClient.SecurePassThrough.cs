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
        KnownErrorCodes.Success,
        new GetDeviceInfoResponse<NullResult>() {
          ErrorCode = KnownErrorCodes.Success,
          Result = new(),
        }
      )
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      httpClientFactory: defaultHttpClientFactory
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
    );

    GetDeviceInfoResponse<NullResult>? nullableResponse = null;

    Assert.DoesNotThrowAsync(
      async () => nullableResponse = await client.SendRequestAsync<GetDeviceInfoRequest, GetDeviceInfoResponse<NullResult>>()
    );

    Assert.IsNotNull(nullableResponse);

    var response = nullableResponse!.Value;

    Assert.AreEqual(KnownErrorCodes.Success, response.ErrorCode, nameof(response.ErrorCode));
    Assert.IsNotNull(response.Result, nameof(response.Result));
  }

  [Test]
  public void SendRequestAsync_SessionNotEstablished()
  {
    using var client = new TapoClient(
      endPoint: new DnsEndPoint("localhost", 0),
      httpClientFactory: defaultHttpClientFactory
    );

    Assert.ThrowsAsync<InvalidOperationException>(
      async () => await client.SendRequestAsync<GetDeviceInfoRequest, GetDeviceInfoResponse<NullResult>>()
    );
#pragma warning disable CA2012
    Assert.Throws<InvalidOperationException>(
      () => client.SendRequestAsync<GetDeviceInfoRequest, GetDeviceInfoResponse<NullResult>>()
    );
#pragma warning restore CA2012
  }

  [Test]
  public async Task SendRequestAsync_ErrorResponse_PassThroughResponse()
  {
    const int errorCode = 9999;

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = static (_, _, _) => (
        KnownErrorCodes.Success,
        new GetDeviceInfoResponse<NullResult>() {
          ErrorCode = errorCode,
          Result = new(),
        }
      )
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      httpClientFactory: defaultHttpClientFactory
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
    );

    var ex = Assert.ThrowsAsync<TapoErrorResponseException>(
      async () => await client.SendRequestAsync<GetDeviceInfoRequest, GetDeviceInfoResponse<NullResult>>()
    );

    Assert.AreEqual(errorCode, ex!.RawErrorCode, nameof(ex.RawErrorCode));
    Assert.AreEqual(new GetDeviceInfoRequest().Method, ex.RequestMethod, nameof(ex.RequestMethod));
    Assert.AreEqual(new Uri(device.EndPointUri!, client.Session!.RequestPathAndQuery), ex.EndPoint, nameof(ex.EndPoint));
  }

  [Test]
  public async Task SendRequestAsync_ErrorResponse_SecurePassThroughResponse()
  {
    const int errorCode = 9999;

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = static (_, _, _) => (
        errorCode,
        new GetDeviceInfoResponse<NullResult>() {
          ErrorCode = KnownErrorCodes.Success,
          Result = new(),
        }
      )
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      httpClientFactory: defaultHttpClientFactory
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
    );

    var ex = Assert.ThrowsAsync<TapoErrorResponseException>(
      async () => await client.SendRequestAsync<GetDeviceInfoRequest, GetDeviceInfoResponse<NullResult>>()
    );

    Assert.AreEqual(errorCode, ex!.RawErrorCode, nameof(ex.RawErrorCode));
    Assert.AreEqual("securePassthrough", ex.RequestMethod, nameof(ex.RequestMethod));
    Assert.AreEqual(new Uri(device.EndPointUri!, client.Session!.RequestPathAndQuery), ex.EndPoint, nameof(ex.EndPoint));
  }
}