// SPDX-FileCopyrightText: 2025 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

using NUnit.Framework;

using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

[SetUpFixture]
internal class CommonPseudoTapoDevice {
  public static PseudoTapoDevice Instance {
    get => field ?? throw new InvalidCastException("CommonPseudoTapoDevice is not set up yet.");
    private set;
  }

  public static PseudoTapoDevice Configure(
    object? state = null,
    Action<HttpListenerContext>? funcProcessRequest = null,
    Func<PseudoTapoDevice.SessionBase, string?>? funcGenerateToken = null,
    Func<PseudoTapoDevice.SessionBase, RSA, HandshakeResponse>? funcGenerateHandshakeResponse = null,
    Func<PseudoTapoDevice.SessionBase, string?>? funcGenerateCookieValue = null,
    Func<PseudoTapoDevice.SessionBase, JsonElement, LoginDeviceResponse>? funcGenerateLoginDeviceResponse = null,
    Func<PseudoTapoDevice.SessionBase, string, JsonElement, (int, ITapoPassThroughResponse?)>? funcGeneratePassThroughResponse = null,
    Action<IPEndPoint, Memory<byte>, Memory<byte>>? funcGenerateKlapAuthHash = null,
    Func<PseudoTapoDevice.SessionBase, (HttpStatusCode, string)>? funcGenerateKlapHandshake2Response = null,
    Func<PseudoTapoDevice.SessionBase, JsonDocument, int, object?>? funcGenerateKlapRequestResponse = null
  )
  {
    Instance.ClearSessions();
    Instance.State = state;
    Instance.FuncProcessRequest = funcProcessRequest;
    Instance.FuncGenerateToken = funcGenerateToken;
    Instance.FuncGenerateHandshakeResponse = funcGenerateHandshakeResponse;
    Instance.FuncGenerateCookieValue = funcGenerateCookieValue;
    Instance.FuncGenerateLoginDeviceResponse = funcGenerateLoginDeviceResponse;
    Instance.FuncGeneratePassThroughResponse = funcGeneratePassThroughResponse;
    Instance.FuncGenerateKlapAuthHash = funcGenerateKlapAuthHash;
    Instance.FuncGenerateKlapHandshake2Response = funcGenerateKlapHandshake2Response;
    Instance.FuncGenerateKlapRequestResponse = funcGenerateKlapRequestResponse;

    return Instance;
  }

  [OneTimeSetUp]
  public void SetUp()
  {
    Instance = new PseudoTapoDevice();

    Instance.Start();
  }

  [OneTimeTearDown]
  public async Task TearDown()
  {
    await Instance.DisposeAsync().ConfigureAwait(false);

    Instance = null!;
  }
}
