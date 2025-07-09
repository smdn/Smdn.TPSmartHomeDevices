// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

partial class TapoClientTests {
  [Test]
  public async Task PostRequestAsync_NullJsonResponse()
  {
    var contentEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    await using var device = new PseudoTapoDevice() {
      FuncProcessRequest = context => {
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentEncoding = contentEncoding;
        context.Response.ContentType = "application/json";

        using var buffer = new MemoryStream();
        using var writer = new StreamWriter(buffer, contentEncoding, 1024, leaveOpen: true);

        writer.Write("null");
        writer.Close();

        context.Response.ContentLength64 = buffer.Length;

        buffer.Position = 0L;

        buffer.CopyTo(context.Response.OutputStream);
      }
    };

    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    // Confirm that at least an exception other than NullReferenceException is thrown,
    // since the case where JsonSerializer returns null cannot be reproduced.
    var ex = Assert.CatchAsync(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
    );

    Assert.That(ex, Is.Not.InstanceOf<NullReferenceException>());

    Assert.That(client.Session, Is.Null);
  }

  [Test]
  public Task PostRequestAsync_Http3xxResponse()
    // default http handler must not allow auto redirect
    => PostRequestAsync_HttpErrorResponse(HttpStatusCode.MovedPermanently);

  [Test]
  public Task PostRequestAsync_Http4xxResponse()
    => PostRequestAsync_HttpErrorResponse(HttpStatusCode.Forbidden);

  [Test]
  public Task PostRequestAsync_Http5xxResponse()
    => PostRequestAsync_HttpErrorResponse(HttpStatusCode.InternalServerError);

  private async Task PostRequestAsync_HttpErrorResponse(HttpStatusCode statusCode)
  {
    await using var device = new PseudoTapoDevice() {
      FuncProcessRequest = context => {
        context.Response.StatusCode = (int)statusCode;
      }
    };

    var endPoint = device.Start();

    using var client = new TapoClient(
      endPoint: endPoint
    );

    var ex = Assert.ThrowsAsync<HttpRequestException>(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      )
    );

    Assert.That(statusCode, Is.EqualTo(ex!.StatusCode), nameof(ex.StatusCode));

    Assert.That(client.Session, Is.Null);
  }

  private class RequestCancellationHttpClientFactory : IHttpClientFactory {
    private readonly CancellationTokenSource cancellationTokenSource;

    public RequestCancellationHttpClientFactory(CancellationTokenSource cancellationTokenSource)
    {
      this.cancellationTokenSource = cancellationTokenSource;
    }

    public HttpClient CreateClient(string name)
    {
      var client = TapoClient.DefaultHttpClientFactory.CreateClient(name);

      cancellationTokenSource.Cancel();

      return client;
    }
  }

  [Test]
  public async Task PostRequestAsync_CancellationRequested()
  {
    await using var device = new PseudoTapoDevice() {
      FuncProcessRequest = static context => throw new InvalidOperationException("request must not be performed"),
    };

    var endPoint = device.Start();

    using var cts = new CancellationTokenSource();
    using var client = new TapoClient(
      endPoint: endPoint,
      httpClientFactory: new RequestCancellationHttpClientFactory(cts)
    );

    var ex = Assert.CatchAsync(
      // RequestCancellationHttpClientFactory will make a cancel request immediately after creating the HttpClient.
      // This will cause a cancellation request to be made before the request is sent.
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!,
        cancellationToken: cts.Token
      )
    );

    Assert.That(ex, Is.InstanceOf<OperationCanceledException>().Or.InstanceOf<TaskCanceledException>());

    Assert.That(client.Session, Is.Null);
  }
}
