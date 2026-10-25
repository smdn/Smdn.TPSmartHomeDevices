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
    Assert.That(
      () => {
        using var client = new KasaClient(
          endPoint: null!
        );
      },
      Throws.ArgumentNullException
    );
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

    Assert.That(() => _ = client.IsConnected, Throws.TypeOf<ObjectDisposedException>(), nameof(client.IsConnected));
    Assert.That(() => _ = client.EndPoint, Throws.TypeOf<ObjectDisposedException>(), nameof(client.EndPoint));

    Assert.That(
      () => client.SendAsync(
        JsonEncodedText.Encode("module"),
        JsonEncodedText.Encode("method"),
        parameter: new { },
        composeResult: static _ => _
      ),
      Throws.TypeOf<ObjectDisposedException>(),
      nameof(client.SendAsync)
    );

    Assert.That(
      async () => await client.SendAsync(
        JsonEncodedText.Encode("module"),
        JsonEncodedText.Encode("method"),
        parameter: new { },
        composeResult: static _ => _
      ),
      Throws.TypeOf<ObjectDisposedException>(),
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

    Assert.That(() => _ = client.IsConnected, Throws.TypeOf<ObjectDisposedException>(), nameof(client.IsConnected));
    Assert.That(() => _ = client.EndPoint, Throws.TypeOf<ObjectDisposedException>(), nameof(client.EndPoint));

    Assert.That(
      () => client.SendAsync(
        JsonEncodedText.Encode("module"),
        JsonEncodedText.Encode("method"),
        parameter: new { },
        composeResult: static _ => _
      ),
      Throws.TypeOf<ObjectDisposedException>(),
      nameof(client.SendAsync)
    );

    Assert.That(
      async () => await client.SendAsync(
        JsonEncodedText.Encode("module"),
        JsonEncodedText.Encode("method"),
        parameter: new { },
        composeResult: static _ => _
      ),
      Throws.TypeOf<ObjectDisposedException>(),
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

    Assert.That(
      async () => await client.SendAsync(
        module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
        method: JsonEncodedText.Encode("get_light_state"),
        parameter: new { },
        composeResult: static _ => true
      ),
      Throws.Nothing
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

    Assert.That(
      async () => await client.SendAsync(
        module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
        method: JsonEncodedText.Encode("get_light_state"),
        parameter: new { },
        composeResult: ThrowExceptionComposeResult
      ),
      Throws
        .TypeOf<KasaUnexpectedResponseException>()
        .With
        .InnerException.TypeOf<NotImplementedException>()
    );
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

    Assert.That(
      async () => {
        try {
          await client.SendAsync(
            module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
            method: JsonEncodedText.Encode("get_light_state"),
            parameter: new { },
            composeResult: static json => JsonDocument.Parse(json.ToString()),
            cancellationToken: cts.Token
          );
        }
        catch (OperationCanceledException) {
          Assert.Inconclusive("test timed out");
        }
      },
      Throws
        .InstanceOf<KasaDisconnectedException>()
        .With
        .Property(nameof(KasaDisconnectedException.DeviceEndPoint))
        .EqualTo(device.EndPoint)
    );
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
    Assert.That(
      async () => {
        try {
          await client.SendAsync(
            module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
            method: JsonEncodedText.Encode("get_light_state"),
            parameter: new { },
            composeResult: static json => JsonDocument.Parse(json.ToString())
          );
        }
        catch (OperationCanceledException) {
          Assert.Inconclusive("test timed out");
        }
      },
      Throws
        .InstanceOf<KasaDisconnectedException>()
        .With
        .Property(nameof(KasaDisconnectedException.DeviceEndPoint))
        .EqualTo(device.EndPoint)
    );
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

    Assert.That(
      async () => await client.SendAsync(
        module: JsonEncodedText.Encode("smartlife.iot.smartbulb.lightingservice"),
        method: JsonEncodedText.Encode("get_light_state"),
        parameter: new { },
        composeResult: static _ => 0
      ),
      Throws
        .TypeOf<KasaIncompleteResponseException>()
        .With.InnerException.TypeOf<KasaMessageBodyTooShortException>()
        .With.InnerException.Property(nameof(KasaMessageBodyTooShortException.ActualLength)).EqualTo(actualBodyLength)
        .With.InnerException.Property(nameof(KasaMessageBodyTooShortException.IndicatedLength)).EqualTo(bodyLengthIndicatedInHeader)
    );

    // Assert.IsTrue(client.IsConnected, nameof(client.IsConnected));
    Assert.That(device.EndPoint, Is.EqualTo(client.EndPoint), nameof(client.EndPoint));
  }
}
