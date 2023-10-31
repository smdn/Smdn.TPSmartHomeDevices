// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading.Tasks;
using NUnit.Framework;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

partial class TapoClientTests {
  [Test]
  public async Task SendRequestAsync_KLAP()
  {
    await using var device = new PseudoTapoDevice() {
      FuncGenerateKlapAuthHash = (_, _, authHash) =>
        _ = TapoCredentials.TryComputeKlapAuthHash(
          defaultCredentialProvider!.GetCredential(null),
          authHash.Span,
          out _
        ),
      FuncGenerateKlapRequestResponse = (_, _, _) => new GetDeviceInfoResponse<NullResult>() {
        ErrorCode = KnownErrorCodes.Success,
        Result = new(),
      },
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      httpClientFactory: defaultHttpClientFactory
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(
        protocol: TapoSessionProtocol.Klap,
        identity: null,
        credential: defaultCredentialProvider!
      )
    );

    GetDeviceInfoResponse<NullResult>? nullableResponse = null;

    for (var i = 0; i < 3; i++) {
      Assert.DoesNotThrowAsync(
        async () => nullableResponse = await client.SendRequestAsync<GetDeviceInfoRequest, GetDeviceInfoResponse<NullResult>>(),
        $"attempt #{i}"
      );

      Assert.IsNotNull(nullableResponse);

      var response = nullableResponse!.Value;

      Assert.AreEqual(KnownErrorCodes.Success, response.ErrorCode, nameof(response.ErrorCode));
      Assert.IsNotNull(response.Result, nameof(response.Result));
    }
  }

  [Test]
  public async Task SendRequestAsync_KLAP_ErrorResponse()
  {
    const int errorCode = 9999;
    int? sequenceNumber = null;

    await using var device = new PseudoTapoDevice() {
      FuncGenerateKlapAuthHash = (_, _, authHash) =>
        _ = TapoCredentials.TryComputeKlapAuthHash(
          defaultCredentialProvider!.GetCredential(null),
          authHash.Span,
          out _
        ),
      FuncGenerateKlapRequestResponse = (_, _, seq) => {
        sequenceNumber = seq;

        return new GetDeviceInfoResponse<NullResult>() {
          ErrorCode = errorCode,
          Result = new(),
        };
      },
    };
    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint,
      httpClientFactory: defaultHttpClientFactory
    );

    Assert.DoesNotThrowAsync(
      async () => await client.AuthenticateAsync(
        protocol: TapoSessionProtocol.Klap,
        identity: null,
        credential: defaultCredentialProvider!
      )
    );

    int? prevSequenceNumber = null;

    for (var i = 0; i < 3; i++) {
      var ex = Assert.ThrowsAsync<TapoErrorResponseException>(
        async () => await client.SendRequestAsync<GetDeviceInfoRequest, GetDeviceInfoResponse<NullResult>>(),
        $"attempt #{i}"
      );

      Assert.AreEqual(errorCode, ex!.RawErrorCode, nameof(ex.RawErrorCode));
      Assert.AreEqual(new GetDeviceInfoRequest().Method, ex.RequestMethod, nameof(ex.RequestMethod));
      Assert.AreEqual(new Uri(device.EndPointUri!, $"/app/request?seq={sequenceNumber}"), ex.EndPoint, nameof(ex.EndPoint));

      if (prevSequenceNumber.HasValue)
        Assert.AreEqual(sequenceNumber, prevSequenceNumber.Value + 1, nameof(sequenceNumber));

      prevSequenceNumber = sequenceNumber;
    }

    if (prevSequenceNumber.HasValue)
      Assert.AreEqual(sequenceNumber, prevSequenceNumber.Value, nameof(sequenceNumber));
  }
}
