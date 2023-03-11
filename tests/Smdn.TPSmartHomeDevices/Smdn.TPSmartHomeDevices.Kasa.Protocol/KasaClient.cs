// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    Assert.IsFalse(client.IsConnected, nameof(client.IsConnected));
    Assert.AreEqual(client.EndPoint, endPoint, nameof(client.EndPoint));

    Assert.DoesNotThrow(client.Dispose, "Dispose not-disposed");
    Assert.DoesNotThrow(client.Dispose, "Dispose already-disposed");

    Assert.Throws<ObjectDisposedException>(() => Assert.IsFalse(client.IsConnected), nameof(client.IsConnected));
    Assert.Throws<ObjectDisposedException>(() => Assert.AreEqual(client.EndPoint, endPoint, nameof(client.EndPoint)));
    Assert.Throws<ObjectDisposedException>(
      () => client.SendAsync(
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

    Assert.IsTrue(client.IsConnected, nameof(client.IsConnected));
    Assert.AreEqual(client.EndPoint, device.EndPoint, nameof(client.EndPoint));

    Assert.DoesNotThrow(client.Dispose, "Dispose not-disposed");
    Assert.DoesNotThrow(client.Dispose, "Dispose already-disposed");

    Assert.Throws<ObjectDisposedException>(() => Assert.IsFalse(client.IsConnected), nameof(client.IsConnected));
    Assert.Throws<ObjectDisposedException>(() => Assert.AreEqual(client.EndPoint, device.EndPoint, nameof(client.EndPoint)));
    Assert.Throws<ObjectDisposedException>(
      () => client.SendAsync(
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

    Assert.IsFalse(client.IsConnected, nameof(client.IsConnected));
    Assert.AreEqual(client.EndPoint, device.EndPoint, nameof(client.EndPoint));

    var result = await client.SendAsync(
      module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
      method: JsonEncodedText.Encode("get_light_state"),
      parameter: new { },
      composeResult: static json => JsonDocument.Parse(json.ToString())
    );

    Assert.IsTrue(client.IsConnected, nameof(client.IsConnected));

    Assert.IsNotNull(result);
    Assert.AreEqual(JsonValueKind.Object, result.RootElement.ValueKind, nameof(result.RootElement.ValueKind));
    Assert.AreEqual(1, result.RootElement.GetProperty("on_off").GetInt32(), "on_off");
    Assert.AreEqual(50, result.RootElement.GetProperty("brightness").GetInt32(), "on_off");
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

    Assert.IsFalse(client.IsConnected, nameof(client.IsConnected));
    Assert.AreEqual(client.EndPoint, device.EndPoint, nameof(client.EndPoint));

    await client.SendAsync(
      module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
      method: JsonEncodedText.Encode("get_light_state"),
      parameter: new { },
      composeResult: static json => JsonDocument.Parse(json.ToString())
    );

    Assert.IsTrue(client.IsConnected, nameof(client.IsConnected));
    Assert.AreEqual(client.EndPoint, device.EndPoint, nameof(client.EndPoint));

    var result = await client.SendAsync(
      module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
      method: JsonEncodedText.Encode("get_light_state"),
      parameter: new { },
      composeResult: static json => JsonDocument.Parse(json.ToString())
    );

    Assert.IsNotNull(result);
    Assert.AreEqual(JsonValueKind.Object, result.RootElement.ValueKind, nameof(result.RootElement.ValueKind));
    Assert.AreEqual(1, result.RootElement.GetProperty("on_off").GetInt32(), "on_off");
    Assert.AreEqual(50, result.RootElement.GetProperty("brightness").GetInt32(), "on_off");
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

    Assert.IsFalse(client.IsConnected, nameof(client.IsConnected));
    Assert.AreEqual(client.EndPoint, device.EndPoint, nameof(client.EndPoint));

    static bool ThrowExceptionComposeResult(JsonElement _) => throw new NotImplementedException();

    var ex = Assert.ThrowsAsync<KasaUnexpectedResponseException>(
      async () => await client.SendAsync(
        module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
        method: JsonEncodedText.Encode("get_light_state"),
        parameter: new { },
        composeResult: ThrowExceptionComposeResult
      )
    );

    Assert.IsTrue(client.IsConnected, nameof(client.IsConnected));
    Assert.AreEqual(client.EndPoint, device.EndPoint, nameof(client.EndPoint));
    Assert.IsInstanceOf<NotImplementedException>(ex!.InnerException);
  }

  [Test]
  public async Task SendAsync_DisconnectedException_DeviceNotRespond()
  {
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

    Assert.IsTrue(client.IsConnected, nameof(client.IsConnected));
    Assert.AreEqual(client.EndPoint, device.EndPoint, nameof(client.EndPoint));

    // disconnect from device
    await device.DisposeAsync();

    var ex = Assert.ThrowsAsync<KasaDisconnectedException>(
      async () => await client.SendAsync(
        module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
        method: JsonEncodedText.Encode("get_light_state"),
        parameter: new { },
        composeResult: static json => JsonDocument.Parse(json.ToString())
      )
    );

    Assert.AreEqual(ex.DeviceEndPoint, device.EndPoint, nameof(ex.DeviceEndPoint));
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

    Assert.IsTrue(client.IsConnected, nameof(client.IsConnected));
    Assert.AreEqual(client.EndPoint, device.EndPoint, nameof(client.EndPoint));

    // device will close connection
    var ex = Assert.ThrowsAsync<KasaDisconnectedException>(
      async () => await client.SendAsync(
        module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
        method: JsonEncodedText.Encode("get_light_state"),
        parameter: new { },
        composeResult: static json => JsonDocument.Parse(json.ToString())
      )
    );

    Assert.AreEqual(ex.DeviceEndPoint, device.EndPoint, nameof(ex.DeviceEndPoint));
  }
}
