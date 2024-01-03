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
          ErrorCode = ErrorCode,
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
