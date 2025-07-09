// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
// cSpell:ignore smartlife,smartbulb,lightingservice

using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Kasa.Protocol;

[TestFixture]
public class KasaClientTests {
  [Test]
  public void Ctor_ArgumentNull_EndPoint()
  {
    Assert.Throws<ArgumentNullException>(() => {
      using var client = new KasaClient(
        endPoint: null!
      );
    });
  }

  [Test]
  public void Dispose()
  {
    var endPoint = new IPEndPoint(IPAddress.Loopback, 9999);

    using var client = new KasaClient(
      endPoint: endPoint
    );

    Assert.That(client.IsConnected, Is.False, nameof(client.IsConnected));
    Assert.That(endPoint, Is.EqualTo(client.EndPoint), nameof(client.EndPoint));

    Assert.DoesNotThrow(client.Dispose, "Dispose not-disposed");
    Assert.DoesNotThrow(client.Dispose, "Dispose already-disposed");

    Assert.Throws<ObjectDisposedException>(() => Assert.That(client.IsConnected, Is.False), nameof(client.IsConnected));
    Assert.Throws<ObjectDisposedException>(() => Assert.That(endPoint, Is.EqualTo(client.EndPoint), nameof(client.EndPoint)));
#pragma warning disable CA2012
    Assert.Throws<ObjectDisposedException>(
      () => client.SendAsync(
        JsonEncodedText.Encode("module"),
        JsonEncodedText.Encode("method"),
        parameter: new { },
        composeResult: static _ => _
      ),
      nameof(client.SendAsync)
    );
#pragma warning restore CA2012
    Assert.ThrowsAsync<ObjectDisposedException>(
      async () => await client.SendAsync(
        JsonEncodedText.Encode("module"),
        JsonEncodedText.Encode("method"),
        parameter: new { },
        composeResult: static _ => _
      ),
      nameof(client.SendAsync)
    );
  }

  [Test]
  public async Task Dispose_ConnectedState()
  {
    await using var device = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, _) => JsonDocument.Parse(@"{""module"":{""method"":{}}}"),
    };

    using var client = new KasaClient(
      endPoint: device.Start()
    );

    // connect
    await client.SendAsync(
      JsonEncodedText.Encode("module"),
      JsonEncodedText.Encode("method"),
      parameter: new { },
      composeResult: static _ => _
    );

    Assert.That(client.IsConnected, Is.True, nameof(client.IsConnected));
    Assert.That(device.EndPoint, Is.EqualTo(client.EndPoint), nameof(client.EndPoint));

    Assert.DoesNotThrow(client.Dispose, "Dispose not-disposed");
    Assert.DoesNotThrow(client.Dispose, "Dispose already-disposed");

    Assert.Throws<ObjectDisposedException>(() => Assert.That(client.IsConnected, Is.False), nameof(client.IsConnected));
    Assert.Throws<ObjectDisposedException>(() => Assert.That(device.EndPoint, Is.EqualTo(client.EndPoint), nameof(client.EndPoint)));
#pragma warning disable CA2012
    Assert.Throws<ObjectDisposedException>(
      () => client.SendAsync(
        JsonEncodedText.Encode("module"),
        JsonEncodedText.Encode("method"),
        parameter: new { },
        composeResult: static _ => _
      ),
      nameof(client.SendAsync)
    );
#pragma warning restore CA2012
    Assert.ThrowsAsync<ObjectDisposedException>(
      async () => await client.SendAsync(
        JsonEncodedText.Encode("module"),
        JsonEncodedText.Encode("method"),
        parameter: new { },
        composeResult: static _ => _
      ),
      nameof(client.SendAsync)
    );
  }

  private readonly struct PseudoRequest {
    [JsonPropertyName("smartlife.iot.smartbulb.lightingservice")]
    public GetLightStateMethod Method { get; init; }

    public readonly struct GetLightStateMethod {
      [JsonPropertyName("get_light_state")]
      public RequestParams Params { get; init; }

      public readonly struct RequestParams { }
    }
  }

  [Test]
  public async Task SendAsync()
  {
    await using var device = new PseudoKasaDevice() {
      FuncGenerateResponse = static (EndPoint _, JsonDocument requestJsonDocument) => {
        Assert.DoesNotThrow(() => JsonSerializer.Deserialize<PseudoRequest>(requestJsonDocument));

        return JsonDocument.Parse(json: @"{
  ""smartlife.iot.smartbulb.lightingservice"": {
    ""get_light_state"": {
      ""on_off"": 1,
      ""brightness"": 50
    }
  }
}")!;
      },
    };

    using var client = new KasaClient(
      endPoint: device.Start()
    );

    Assert.That(client.IsConnected, Is.False, nameof(client.IsConnected));
    Assert.That(device.EndPoint, Is.EqualTo(client.EndPoint), nameof(client.EndPoint));

    var result = await client.SendAsync(
      module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
      method: JsonEncodedText.Encode("get_light_state"),
      parameter: new { },
      composeResult: static json => JsonDocument.Parse(json.ToString())
    );

    Assert.That(client.IsConnected, Is.True, nameof(client.IsConnected));

    Assert.That(result, Is.Not.Null);
    Assert.That(result.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Object), nameof(result.RootElement.ValueKind));
    Assert.That(result.RootElement.GetProperty("on_off").GetInt32(), Is.EqualTo(1), "on_off");
    Assert.That(result.RootElement.GetProperty("brightness").GetInt32(), Is.EqualTo(50), "on_off");
  }

  [Test]
  public async Task SendAsync_ConnectedState()
  {
    await using var device = new PseudoKasaDevice() {
      FuncGenerateResponse = static (EndPoint _, JsonDocument requestJsonDocument) => {
        Assert.DoesNotThrow(() => JsonSerializer.Deserialize<PseudoRequest>(requestJsonDocument));

        return JsonDocument.Parse(json: @"{
  ""smartlife.iot.smartbulb.lightingservice"": {
    ""get_light_state"": {
      ""on_off"": 1,
      ""brightness"": 50
    }
  }
}")!;
      },
    };

    using var client = new KasaClient(
      endPoint: device.Start()
    );

    Assert.That(client.IsConnected, Is.False, nameof(client.IsConnected));
    Assert.That(device.EndPoint, Is.EqualTo(client.EndPoint), nameof(client.EndPoint));

    await client.SendAsync(
      module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
      method: JsonEncodedText.Encode("get_light_state"),
      parameter: new { },
      composeResult: static json => JsonDocument.Parse(json.ToString())
    );

    Assert.That(client.IsConnected, Is.True, nameof(client.IsConnected));
    Assert.That(device.EndPoint, Is.EqualTo(client.EndPoint), nameof(client.EndPoint));

    var result = await client.SendAsync(
      module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
      method: JsonEncodedText.Encode("get_light_state"),
      parameter: new { },
      composeResult: static json => JsonDocument.Parse(json.ToString())
    );

    Assert.That(result, Is.Not.Null);
    Assert.That(result.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Object), nameof(result.RootElement.ValueKind));
    Assert.That(result.RootElement.GetProperty("on_off").GetInt32(), Is.EqualTo(1), "on_off");
    Assert.That(result.RootElement.GetProperty("brightness").GetInt32(), Is.EqualTo(50), "on_off");
  }

  [Test]
  public async Task SendAsync_UseInterNetworkAddressTypeIfDnsEndPointWithUnspecifiedAddressType()
  {
    await using var device = new PseudoKasaDevice() {
      FuncGenerateResponse = static (EndPoint _, JsonDocument requestJsonDocument) => {
        Assert.DoesNotThrow(() => JsonSerializer.Deserialize<PseudoRequest>(requestJsonDocument));

        return JsonDocument.Parse(json: @"{
  ""smartlife.iot.smartbulb.lightingservice"": {
    ""get_light_state"": { }
  }
}")!;
      },
    };

    device.Start();

    using var client = new KasaClient(
      endPoint: new DnsEndPoint(device.EndPoint!.Address.ToString(), device.EndPoint!.Port, AddressFamily.Unspecified)
    );

    Assert.That(client.IsConnected, Is.False, nameof(client.IsConnected));
    Assert.That(client.EndPoint.AddressFamily, Is.EqualTo(AddressFamily.Unspecified), nameof(client.EndPoint.AddressFamily));

    Assert.DoesNotThrowAsync(
      async () => await client.SendAsync(
        module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
        method: JsonEncodedText.Encode("get_light_state"),
        parameter: new { },
        composeResult: static _ => true
      )
    );

    Assert.That(client.IsConnected, Is.True, nameof(client.IsConnected));
  }

  [Test]
  public async Task SendAsync_ExceptionThrownByResultConverter()
  {
    await using var device = new PseudoKasaDevice() {
      FuncGenerateResponse = static (EndPoint _, JsonDocument requestJsonDocument) => {
        Assert.DoesNotThrow(() => JsonSerializer.Deserialize<PseudoRequest>(requestJsonDocument));

        return JsonDocument.Parse(json: @"{
  ""smartlife.iot.smartbulb.lightingservice"": {
    ""get_light_state"": {}
  }
}")!;
      },
    };

    using var client = new KasaClient(
      endPoint: device.Start()
    );

    Assert.That(client.IsConnected, Is.False, nameof(client.IsConnected));
    Assert.That(device.EndPoint, Is.EqualTo(client.EndPoint), nameof(client.EndPoint));

    static bool ThrowExceptionComposeResult(JsonElement _) => throw new NotImplementedException();

    var ex = Assert.ThrowsAsync<KasaUnexpectedResponseException>(
      async () => await client.SendAsync(
        module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
        method: JsonEncodedText.Encode("get_light_state"),
        parameter: new { },
        composeResult: ThrowExceptionComposeResult
      )
    );

    Assert.That(client.IsConnected, Is.True, nameof(client.IsConnected));
    Assert.That(device.EndPoint, Is.EqualTo(client.EndPoint), nameof(client.EndPoint));
    Assert.That(ex!.InnerException, Is.InstanceOf<NotImplementedException>());
  }

  [Test]
  public async Task SendAsync_DisconnectedException_DeviceNotRespond()
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
      Assert.Ignore("disconnection of device causes test runner timeout");
      return;
    }

    await using var device = new PseudoKasaDevice() {
      FuncGenerateResponse = static (EndPoint _, JsonDocument requestJsonDocument) => JsonDocument.Parse(
json: @"{
  ""smartlife.iot.smartbulb.lightingservice"": {
    ""get_light_state"": {
      ""on_off"": 1,
      ""brightness"": 50
    }
  }
}")!,
    };

    using var client = new KasaClient(
      endPoint: device.Start()
    );

    // set connected state
    await client.SendAsync(
      module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
      method: JsonEncodedText.Encode("get_light_state"),
      parameter: new { },
      composeResult: static json => JsonDocument.Parse(json.ToString())
    );

    Assert.That(client.IsConnected, Is.True, nameof(client.IsConnected));
    Assert.That(device.EndPoint, Is.EqualTo(client.EndPoint), nameof(client.EndPoint));

    // disconnect from device
    await device.DisposeAsync();

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20.0));

    var ex = Assert.CatchAsync(
      async () => await client.SendAsync(
        module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
        method: JsonEncodedText.Encode("get_light_state"),
        parameter: new { },
        composeResult: static json => JsonDocument.Parse(json.ToString()),
        cancellationToken: cts.Token
      )
    );

    switch (ex) {
      case KasaDisconnectedException disconnectedException:
        Assert.That(device.EndPoint, Is.EqualTo(disconnectedException.DeviceEndPoint), nameof(disconnectedException.DeviceEndPoint));
        break;

      case OperationCanceledException:
        Assert.Inconclusive("test timed out");
        break;

      default: // unexpected exception
        Assert.That(ex, Is.InstanceOf<KasaDisconnectedException>());
        break;
    }
  }

  [Test]
  public async Task SendAsync_DisconnectedException_ConnectionClosed()
  {
    int request = 0;

    await using var device = new PseudoKasaDevice() {
      FuncGenerateResponse = (EndPoint _, JsonDocument requestJsonDocument) => {
        if (request == 0) {
          request++;

          return JsonDocument.Parse(json: @"{
  ""smartlife.iot.smartbulb.lightingservice"": {
    ""get_light_state"": {
      ""on_off"": 1,
      ""brightness"": 50
    }
  }
}")!;
        }
        else {
          throw new PseudoKasaDevice.AbortProcessException("close connection");
        }
      },
    };

    using var client = new KasaClient(
      endPoint: device.Start()
    );

    // set connected state
    await client.SendAsync(
      module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
      method: JsonEncodedText.Encode("get_light_state"),
      parameter: new { },
      composeResult: static json => JsonDocument.Parse(json.ToString())
    );

    Assert.That(client.IsConnected, Is.True, nameof(client.IsConnected));
    Assert.That(device.EndPoint, Is.EqualTo(client.EndPoint), nameof(client.EndPoint));

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20.0));

    // device will close connection
    var ex = Assert.CatchAsync(
      async () => await client.SendAsync(
        module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
        method: JsonEncodedText.Encode("get_light_state"),
        parameter: new { },
        composeResult: static json => JsonDocument.Parse(json.ToString())
      )
    );


    switch (ex) {
      case KasaDisconnectedException disconnectedException:
        Assert.That(device.EndPoint, Is.EqualTo(disconnectedException.DeviceEndPoint), nameof(disconnectedException.DeviceEndPoint));
        break;

      case OperationCanceledException:
        Assert.Inconclusive("test timed out");
        break;

      default: // unexpected exception
        Assert.That(ex, Is.InstanceOf<KasaDisconnectedException>());
        break;
    }
  }

  [TestCase(1, 0)]
  [TestCase(1033, 1024)]
  public async Task SendAsync_ReceivedIncompleteResponse(
    int bodyLengthIndicatedInHeader,
    int actualBodyLength
  )
  {
    await using var device = new PseudoKasaDevice() {
      FuncGenerateResponse = static (_, _) => JsonDocument.Parse(json: "{}")!,
      FuncEncryptResponse = _ => {
        const int HeaderLength = 4;
        var response = new byte[HeaderLength + actualBodyLength];

        BinaryPrimitives.WriteInt32BigEndian(response.AsSpan(0, 4), bodyLengthIndicatedInHeader);

        return response;
      }
    };

    using var client = new KasaClient(
      endPoint: device.Start()
    );

    Assert.That(client.IsConnected, Is.False, nameof(client.IsConnected));
    Assert.That(device.EndPoint, Is.EqualTo(client.EndPoint), nameof(client.EndPoint));

    var ex = Assert.ThrowsAsync<KasaIncompleteResponseException>(
      async () => await client.SendAsync(
        module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
        method: JsonEncodedText.Encode("get_light_state"),
        parameter: new { },
        composeResult: static _ => 0
      )
    );

    // Assert.IsTrue(client.IsConnected, nameof(client.IsConnected));
    Assert.That(device.EndPoint, Is.EqualTo(client.EndPoint), nameof(client.EndPoint));
    Assert.That(ex!.InnerException, Is.InstanceOf<KasaMessageBodyTooShortException>());

    var exBodyTooShortException = (ex.InnerException as KasaMessageBodyTooShortException)!;

    Assert.That(exBodyTooShortException.ActualLength, Is.EqualTo(actualBodyLength), nameof(exBodyTooShortException.ActualLength));
    Assert.That(exBodyTooShortException.IndicatedLength, Is.EqualTo(bodyLengthIndicatedInHeader), nameof(exBodyTooShortException.IndicatedLength));
  }
}
