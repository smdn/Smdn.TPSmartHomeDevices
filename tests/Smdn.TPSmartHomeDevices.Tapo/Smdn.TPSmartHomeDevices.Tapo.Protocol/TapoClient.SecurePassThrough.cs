// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

partial class TapoClientTests {
  [Test]
  public void SendRequestAsync()
  {
    var device = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token",
      funcGeneratePassThroughResponse: static (_, _, _) => (
        KnownErrorCodes.Success,
        new PassThroughResponse<NullResult>() {
          ErrorCode = KnownErrorCodes.Success,
          Result = new(),
        }
      )
    );

    using var client = new TapoClient(
      endPoint: device.GetListenerEndPoint(),
      httpClientFactory: defaultHttpClientFactory
    );

    Assert.That(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      ),
      Throws.Nothing
    );

    PassThroughResponse<NullResult>? nullableResponse = null;

    Assert.That(
      async () => nullableResponse = await client.SendRequestAsync<GetDeviceInfoRequest, PassThroughResponse<NullResult>>(),
      Throws.Nothing
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

    Assert.That(
      async () => await client.SendRequestAsync<GetDeviceInfoRequest, PassThroughResponse<NullResult>>(),
      Throws.InvalidOperationException
    );
    Assert.That(
      () => client.SendRequestAsync<GetDeviceInfoRequest, PassThroughResponse<NullResult>>(),
      Throws.InvalidOperationException
    );
  }

  [Test]
  public void SendRequestAsync_ErrorResponse_PassThroughResponse()
  {
    const int ErrorCode = 9999;

    var device = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token",
      funcGeneratePassThroughResponse: static (_, _, _) => (
        KnownErrorCodes.Success,
        new PassThroughResponse<NullResult>() {
          ErrorCode = ErrorCode,
          Result = new(),
        }
      )
    );

    using var client = new TapoClient(
      endPoint: device.GetListenerEndPoint(),
      httpClientFactory: defaultHttpClientFactory
    );

    Assert.That(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      ),
      Throws.Nothing
    );

    Assert.That(
      async () => await client.SendRequestAsync<GetDeviceInfoRequest, PassThroughResponse<NullResult>>(),
      Throws
        .TypeOf<TapoErrorResponseException>()
        .With
        .Property(nameof(TapoErrorResponseException.RawErrorCode))
        .EqualTo(ErrorCode)
        .And
        .Property(nameof(TapoErrorResponseException.RequestMethod))
        .EqualTo(new GetDeviceInfoRequest().Method)
        .And
        .Property(nameof(TapoErrorResponseException.EndPoint))
        .EqualTo(new Uri(device.EndPointUri!, $"/app?token={client.Session!.Token}"))
    );
  }

  [Test]
  public void SendRequestAsync_ErrorResponse_SecurePassThroughResponse()
  {
    const int ErrorCode = 9999;

    var device = CommonPseudoTapoDevice.Configure(
      funcGenerateToken: static _ => "token",
      funcGeneratePassThroughResponse: static (_, _, _) => (
        ErrorCode,
        new PassThroughResponse<NullResult>() {
          ErrorCode = KnownErrorCodes.Success,
          Result = new(),
        }
      )
    );

    using var client = new TapoClient(
      endPoint: device.GetListenerEndPoint(),
      httpClientFactory: defaultHttpClientFactory
    );

    Assert.That(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      ),
      Throws.Nothing
    );

    Assert.That(
      async () => await client.SendRequestAsync<GetDeviceInfoRequest, PassThroughResponse<NullResult>>(),
      Throws
        .TypeOf<TapoErrorResponseException>()
        .With
        .Property(nameof(TapoErrorResponseException.RawErrorCode))
        .EqualTo(ErrorCode)
        .And
        .Property(nameof(TapoErrorResponseException.RequestMethod))
        .EqualTo("securePassthrough")
        .And
        .Property(nameof(TapoErrorResponseException.EndPoint))
        .EqualTo(new Uri(device.EndPointUri!, $"/app?token={client.Session!.Token}"))
    );
  }
}
