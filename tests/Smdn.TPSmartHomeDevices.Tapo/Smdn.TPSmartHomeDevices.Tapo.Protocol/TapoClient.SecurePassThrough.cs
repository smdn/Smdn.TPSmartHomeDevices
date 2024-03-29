// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Threading.Tasks;

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
        new PassThroughResponse<NullResult>() {
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

    PassThroughResponse<NullResult>? nullableResponse = null;

    Assert.DoesNotThrowAsync(
      async () => nullableResponse = await client.SendRequestAsync<GetDeviceInfoRequest, PassThroughResponse<NullResult>>()
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
      async () => await client.SendRequestAsync<GetDeviceInfoRequest, PassThroughResponse<NullResult>>()
    );
#pragma warning disable CA2012
    Assert.Throws<InvalidOperationException>(
      () => client.SendRequestAsync<GetDeviceInfoRequest, PassThroughResponse<NullResult>>()
    );
#pragma warning restore CA2012
  }

  [Test]
  public async Task SendRequestAsync_ErrorResponse_PassThroughResponse()
  {
    const int ErrorCode = 9999;

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = static (_, _, _) => (
        KnownErrorCodes.Success,
        new PassThroughResponse<NullResult>() {
          ErrorCode = ErrorCode,
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
      async () => await client.SendRequestAsync<GetDeviceInfoRequest, PassThroughResponse<NullResult>>()
    );

    Assert.That(ex!.RawErrorCode, Is.EqualTo(ErrorCode), nameof(ex.RawErrorCode));
    Assert.That(ex.RequestMethod, Is.EqualTo(new GetDeviceInfoRequest().Method), nameof(ex.RequestMethod));
    Assert.That(ex.EndPoint, Is.EqualTo(new Uri(device.EndPointUri!, $"/app?token={client.Session!.Token}")), nameof(ex.EndPoint));
  }

  [Test]
  public async Task SendRequestAsync_ErrorResponse_SecurePassThroughResponse()
  {
    const int ErrorCode = 9999;

    await using var device = new PseudoTapoDevice() {
      FuncGenerateToken = static _ => "token",
      FuncGeneratePassThroughResponse = static (_, _, _) => (
        ErrorCode,
        new PassThroughResponse<NullResult>() {
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
      async () => await client.SendRequestAsync<GetDeviceInfoRequest, PassThroughResponse<NullResult>>()
    );

    Assert.That(ex!.RawErrorCode, Is.EqualTo(ErrorCode), nameof(ex.RawErrorCode));
    Assert.That(ex.RequestMethod, Is.EqualTo("securePassthrough"), nameof(ex.RequestMethod));
    Assert.That(ex.EndPoint, Is.EqualTo(new Uri(device.EndPointUri!, $"/app?token={client.Session!.Token}")), nameof(ex.EndPoint));
  }
}
