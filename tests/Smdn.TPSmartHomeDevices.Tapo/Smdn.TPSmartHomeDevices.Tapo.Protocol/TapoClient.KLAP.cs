// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

partial class TapoClientTests {
  [Test]
  public async Task SendRequestAsync_KLAP()
  {
    var device = CommonPseudoTapoDevice.Configure(
      funcGenerateKlapAuthHash: (_, _, authHash) => defaultKlapCredentialProvider!.GetKlapCredential(null).WriteLocalAuthHash(authHash.Span),
      funcGenerateKlapRequestResponse: (_, _, _) => new PassThroughResponse<NullResult>() {
        ErrorCode = KnownErrorCodes.Success,
        Result = new(),
      }
    );

    using var client = new TapoClient(
      endPoint: device.GetListenerEndPoint(),
      httpClientFactory: defaultHttpClientFactory
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(
        protocol: TapoSessionProtocol.Klap,
        identity: null,
        credential: defaultKlapCredentialProvider!
      )
    );

    PassThroughResponse<NullResult>? nullableResponse = null;

    for (var i = 0; i < 3; i++) {
      Assert.DoesNotThrowAsync(
        async () => nullableResponse = await client.SendRequestAsync<GetDeviceInfoRequest, PassThroughResponse<NullResult>>(),
        $"attempt #{i}"
      );

      Assert.That(nullableResponse, Is.Not.Null);

      var response = nullableResponse!.Value;

      Assert.That(response.ErrorCode, Is.EqualTo(KnownErrorCodes.Success), nameof(response.ErrorCode));
    }
  }

  [Test]
  public async Task SendRequestAsync_KLAP_ErrorResponse()
  {
    const int ErrorCode = 9999;
    int? sequenceNumber = null;

    var device = CommonPseudoTapoDevice.Configure(
      funcGenerateKlapAuthHash: (_, _, authHash) => defaultKlapCredentialProvider!.GetKlapCredential(null).WriteLocalAuthHash(authHash.Span),
      funcGenerateKlapRequestResponse: (_, _, seq) => {
        sequenceNumber = seq;

        return new PassThroughResponse<NullResult>() {
          ErrorCode = ErrorCode,
          Result = new(),
        };
      }
    );

    using var client = new TapoClient(
      endPoint: device.GetListenerEndPoint(),
      httpClientFactory: defaultHttpClientFactory
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(
        protocol: TapoSessionProtocol.Klap,
        identity: null,
        credential: defaultKlapCredentialProvider!
      )
    );

    int? prevSequenceNumber = null;

    for (var i = 0; i < 3; i++) {
      var ex = Assert.ThrowsAsync<TapoErrorResponseException>(
        async () => await client.SendRequestAsync<GetDeviceInfoRequest, PassThroughResponse<NullResult>>(),
        $"attempt #{i}"
      );

      Assert.That(ex!.RawErrorCode, Is.EqualTo(ErrorCode), nameof(ex.RawErrorCode));
      Assert.That(ex.RequestMethod, Is.EqualTo(new GetDeviceInfoRequest().Method), nameof(ex.RequestMethod));
      Assert.That(ex.EndPoint, Is.EqualTo(new Uri(device.EndPointUri!, $"/app/request?seq={sequenceNumber}")), nameof(ex.EndPoint));

      if (prevSequenceNumber.HasValue)
        Assert.That(prevSequenceNumber.Value + 1, Is.EqualTo(sequenceNumber), nameof(sequenceNumber));

      prevSequenceNumber = sequenceNumber;
    }

    if (prevSequenceNumber.HasValue)
      Assert.That(prevSequenceNumber.Value, Is.EqualTo(sequenceNumber), nameof(sequenceNumber));
  }
}
