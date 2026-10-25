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
  public void PostRequestAsync_NullJsonResponse()
  {
    var contentEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    var device = CommonPseudoTapoDevice.Configure(
      funcProcessRequest: context => {
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
    );

    using var client = new TapoClient(
      endPoint: device.GetListenerEndPoint()
    );

    // Confirm that at least an exception other than NullReferenceException is thrown,
    // since the case where JsonSerializer returns null cannot be reproduced.
    Assert.That(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      ),
      Throws.Exception.Not.InstanceOf<NullReferenceException>()
    );

    Assert.That(client.Session, Is.Null);
  }

  [Test]
  public void PostRequestAsync_Http3xxResponse()
    // default http handler must not allow auto redirect
    => PostRequestAsync_HttpErrorResponse(HttpStatusCode.MovedPermanently);

  [Test]
  public void PostRequestAsync_Http4xxResponse()
    => PostRequestAsync_HttpErrorResponse(HttpStatusCode.Forbidden);

  [Test]
  public void PostRequestAsync_Http5xxResponse()
    => PostRequestAsync_HttpErrorResponse(HttpStatusCode.InternalServerError);

  private void PostRequestAsync_HttpErrorResponse(HttpStatusCode statusCode)
  {
    var device = CommonPseudoTapoDevice.Configure(
      funcProcessRequest: context => {
        context.Response.StatusCode = (int)statusCode;
      }
    );

    using var client = new TapoClient(
      endPoint: device.GetListenerEndPoint()
    );

    Assert.That(
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!
      ),
      Throws
        .TypeOf<HttpRequestException>()
        .With
        .Property(nameof(HttpRequestException.StatusCode)).EqualTo(statusCode)
    );

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
  public void PostRequestAsync_CancellationRequested()
  {
    var device = CommonPseudoTapoDevice.Configure(
      funcProcessRequest: static context => throw new InvalidOperationException("request must not be performed")
    );

    using var cts = new CancellationTokenSource();
    using var client = new TapoClient(
      endPoint: device.GetListenerEndPoint(),
      httpClientFactory: new RequestCancellationHttpClientFactory(cts)
    );

    Assert.That(
      // RequestCancellationHttpClientFactory will make a cancel request immediately after creating the HttpClient.
      // This will cause a cancellation request to be made before the request is sent.
      async () => await client.AuthenticateAsync(
        identity: null,
        credential: defaultCredentialProvider!,
        cancellationToken: cts.Token
      ),
      Throws.InstanceOf<OperationCanceledException>()
    );

    Assert.That(client.Session, Is.Null);
  }
}
