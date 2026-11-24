// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

partial class TapoClientTests {
  public delegate ValueTask<PassThroughResponse<None>> SendRequestKlapSessionAsyncFunc(TapoClient client);

  private static System.Collections.IEnumerable YieldTestCases_SendRequestAsync_KLAP()
  {
    yield return new object[] {
      new SendRequestKlapSessionAsyncFunc(
#pragma warning disable CS0618 // CS0618: SendRequestAsync without JsonTypeInfo<T> is obsolete.
        static client => client.SendRequestAsync<GetDeviceInfoRequest, PassThroughResponse<None>>()
#pragma warning restore CS0618
      )
    };

    yield return new object[] {
      new SendRequestKlapSessionAsyncFunc(
        static client => client.SendRequestAsync(
          TapoPassThroughJsonSerializerContext.Default.GetDeviceInfoRequest,
          TapoClientTestsJsonSerializerContext.Default.PassThroughResponseOfNone
        )
      )
    };
  }

  [TestCaseSource(nameof(YieldTestCases_SendRequestAsync_KLAP))]
  public void SendRequestAsync_KLAP(SendRequestKlapSessionAsyncFunc sendRequestAsyncFunc)
  {
    var device = CommonPseudoTapoDevice.Configure(
      funcGenerateKlapAuthHash: (_, _, authHash) => defaultKlapCredentialProvider!.GetKlapCredential(null).WriteLocalAuthHash(authHash.Span),
      funcGenerateKlapRequestResponse: (_, _, _) => new PassThroughResponse<None>() {
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

    PassThroughResponse<None>? nullableResponse = null;

    for (var i = 0; i < 3; i++) {
      Assert.That(
        async () => nullableResponse = await sendRequestAsyncFunc(client),
        Throws.Nothing,
        $"attempt #{i}"
      );

      Assert.That(nullableResponse, Is.Not.Null);

      var response = nullableResponse!.Value;

      Assert.That(response.ErrorCode, Is.EqualTo(KnownErrorCodes.Success), nameof(response.ErrorCode));
    }
  }

  [TestCaseSource(nameof(YieldTestCases_SendRequestAsync_KLAP))]
  public void SendRequestAsync_KLAP_ErrorResponse(SendRequestKlapSessionAsyncFunc sendRequestAsyncFunc)
  {
    const int ErrorCode = 9999;
    int? sequenceNumber = null;

    var device = CommonPseudoTapoDevice.Configure(
      funcGenerateKlapAuthHash: (_, _, authHash) => defaultKlapCredentialProvider!.GetKlapCredential(null).WriteLocalAuthHash(authHash.Span),
      funcGenerateKlapRequestResponse: (_, _, seq) => {
        sequenceNumber = seq;

        return new PassThroughResponse<None>() {
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
            await sendRequestAsyncFunc(client);
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
