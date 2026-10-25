// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

partial class TapoClientTests {
  [Test]
  public void SendRequestAsync_KLAP()
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

    Assert.That(
      async () => await client.AuthenticateAsync(
        protocol: TapoSessionProtocol.Klap,
        identity: null,
        credential: defaultKlapCredentialProvider!
      ),
      Throws.Nothing
    );

    PassThroughResponse<NullResult>? nullableResponse = null;

    for (var i = 0; i < 3; i++) {
      Assert.That(
        async () => nullableResponse = await client.SendRequestAsync<GetDeviceInfoRequest, PassThroughResponse<NullResult>>(),
        Throws.Nothing,
        $"attempt #{i}"
      );

      Assert.That(nullableResponse, Is.Not.Null);

      var response = nullableResponse!.Value;

      Assert.That(response.ErrorCode, Is.EqualTo(KnownErrorCodes.Success), nameof(response.ErrorCode));
    }
  }

  [Test]
  public void SendRequestAsync_KLAP_ErrorResponse()
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

    Assert.That(
      async () => await client.AuthenticateAsync(
        protocol: TapoSessionProtocol.Klap,
        identity: null,
        credential: defaultKlapCredentialProvider!
      ),
      Throws.Nothing
    );

    int? prevSequenceNumber = null;

    for (var i = 0; i < 3; i++) {
      Uri? endPointInThrownException = null;

      Assert.That(
        async () => {
          try {
            await client.SendRequestAsync<GetDeviceInfoRequest, PassThroughResponse<NullResult>>();
          }
          catch (TapoErrorResponseException ex) {
            endPointInThrownException = ex.EndPoint;
            throw;
          }
        },
        Throws
          .TypeOf<TapoErrorResponseException>()
          .With
          .Property(nameof(TapoErrorResponseException.RequestMethod))
          .EqualTo(new GetDeviceInfoRequest().Method)
          .And
          .Property(nameof(TapoErrorResponseException.RawErrorCode))
          .EqualTo(ErrorCode),
        $"attempt #{i}"
      );

      Assert.That(
        endPointInThrownException,
        Is.EqualTo(new Uri(device.EndPointUri!, $"/app/request?seq={sequenceNumber}"))
      );

      if (prevSequenceNumber.HasValue)
        Assert.That(prevSequenceNumber.Value + 1, Is.EqualTo(sequenceNumber), nameof(sequenceNumber));

      prevSequenceNumber = sequenceNumber;
    }

    if (prevSequenceNumber.HasValue)
      Assert.That(prevSequenceNumber.Value, Is.EqualTo(sequenceNumber), nameof(sequenceNumber));
  }
}
