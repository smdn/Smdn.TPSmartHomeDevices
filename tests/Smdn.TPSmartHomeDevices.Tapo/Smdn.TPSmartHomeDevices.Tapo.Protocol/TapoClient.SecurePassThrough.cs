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

    Assert.That(nullableResponse, Is.Not.Null);

    var response = nullableResponse!.Value;

    Assert.That(response.ErrorCode, Is.EqualTo(KnownErrorCodes.Success), nameof(response.ErrorCode));
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

    Assert.That(ex!.RawErrorCode, Is.EqualTo(errorCode), nameof(ex.RawErrorCode));
    Assert.That(ex.RequestMethod, Is.EqualTo(new GetDeviceInfoRequest().Method), nameof(ex.RequestMethod));
    Assert.That(ex.EndPoint, Is.EqualTo(new Uri(device.EndPointUri!, $"/app?token={client.Session!.Token}")), nameof(ex.EndPoint));
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

    Assert.That(ex!.RawErrorCode, Is.EqualTo(errorCode), nameof(ex.RawErrorCode));
    Assert.That(ex.RequestMethod, Is.EqualTo("securePassthrough"), nameof(ex.RequestMethod));
    Assert.That(ex.EndPoint, Is.EqualTo(new Uri(device.EndPointUri!, $"/app?token={client.Session!.Token}")), nameof(ex.EndPoint));
  }
}
