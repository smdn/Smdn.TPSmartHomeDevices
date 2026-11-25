// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE || SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using Microsoft.Extensions.Logging;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// Maintains authenticated Tapo session information, including access token, exchanged key and session ID.
/// </summary>
/// <remarks>
/// This implementation is based on the following
/// C# implementation by <see href="https://github.com/europowergenerators">E-Power International</see>:
/// <see href="https://github.com/europowergenerators/Tapo-plug-controller">europowergenerators/Tapo-plug-controller</see>, published under the MIT License.
/// </remarks>
internal sealed class TapoSecurePassThroughSession : TapoSession {
  internal static readonly Uri RequestPath = new("/app", UriKind.Relative);

  public Uri RequestPathAndQuery { get; private set; } = RequestPath;
  public override string? Token => token;
  public override TapoSessionProtocol Protocol => TapoSessionProtocol.SecurePassThrough;

  private string? token;
  private Aes? aes;
  private PassThroughMessageTransform passThroughMessageTransform;
#pragma warning disable CS0618 // CS0618: SecurePassThroughJsonConverterFactory is obsolete.
  private SecurePassThroughJsonConverterFactory securePassThroughJsonConverterFactory;
#pragma warning restore CS0618

  internal JsonSerializerOptions SecurePassThroughJsonSerializerOptions {
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE
    [RequiresDynamicCode(FeatureRequirementMessages.RequiresDynamicCodeJsonConverterFactoryInMethod)]
    [RequiresUnreferencedCode(FeatureRequirementMessages.RequiresUnreferencedCodeJsonConverterFactoryInMethod)]
#endif
    get;
  }

  internal LoginDevicePlainTextJsonSerializerContext LoginDeviceJsonContext { get; }

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_UNCONDITIONALSUPPRESSMESSAGEATTRIBUTE
  [UnconditionalSuppressMessage(
    category: "AOT",
    checkId: "IL3050",
    Justification = $"This method operates via the AOT-compatible code path unless `{nameof(SecurePassThroughJsonSerializerOptions)}` is used."
  )]
  [UnconditionalSuppressMessage(
    category: "Trimming",
    checkId: "IL2026",
    Justification = $"This method operates via the code path that does not use reflection, unless `{nameof(SecurePassThroughJsonSerializerOptions)}` is used."
  )]
#endif
  internal TapoSecurePassThroughSession(
    ITapoCredentialIdentity? identity,
    string? sessionId,
    DateTime expiresOn,
    ReadOnlySpan<byte> key,
    ReadOnlySpan<byte> iv,
    JsonSerializerOptions baseJsonSerializerOptions,
    ILogger? logger
  )
    : base(
      sessionId,
      expiresOn
    )
  {
    aes = Aes.Create();
    aes.Mode = CipherMode.CBC;
    aes.Padding = PaddingMode.PKCS7;
    aes.Key = key.ToArray();
    aes.IV = iv.ToArray();

    passThroughMessageTransform = new(
      identity: identity,
#pragma warning disable CA5401
      encryptorForPassThroughRequest: aes.CreateEncryptor(),
      decryptorForPassThroughResponse: aes.CreateDecryptor(),
#pragma warning restore CA5401
      baseJsonSerializerOptionsForPassThroughMessage: baseJsonSerializerOptions,
      logger: logger
    );
    securePassThroughJsonConverterFactory = new(
      passThroughMessageTransform: passThroughMessageTransform,
      shouldDisposePassThroughMessageTransform: false
    );

    SecurePassThroughJsonSerializerOptions = new(baseJsonSerializerOptions);
    SecurePassThroughJsonSerializerOptions.Converters.Add(securePassThroughJsonConverterFactory);

    LoginDeviceJsonContext = new LoginDevicePlainTextJsonSerializerContext(
      options: new() {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new LoginDeviceRequest.TapoCredentialJsonConverter(identity) },
      }
    );
  }

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
  [MemberNotNull(nameof(passThroughMessageTransform))]
  [MemberNotNull(nameof(securePassThroughJsonConverterFactory))]
#endif
  private protected override void ThrowIfDisposed()
  {
    if (passThroughMessageTransform is null)
      throw new ObjectDisposedException(GetType().FullName);
    if (securePassThroughJsonConverterFactory is null)
      throw new ObjectDisposedException(GetType().FullName);

    base.ThrowIfDisposed();
  }

  protected override void Dispose(bool disposing)
  {
    if (!disposing) {
      securePassThroughJsonConverterFactory?.Dispose();
      securePassThroughJsonConverterFactory = null!;

      passThroughMessageTransform?.Dispose();
      passThroughMessageTransform = null!;

      aes?.Dispose();
      aes = null;
    }

    base.Dispose(disposing);
  }

  internal void SetToken(string token)
  {
    this.token = token;

    // append issued token to the request path query
    RequestPathAndQuery = new Uri(
      string.Concat(
        RequestPath.ToString(), // only path
        "?token=",
        token
      ),
      UriKind.Relative
    );
  }

  internal PassThroughRequest EncryptPassThroughRequest<TPassThroughRequest>(
    TPassThroughRequest passThroughRequest,
    JsonTypeInfo<TPassThroughRequest> passThroughRequestJsonTypeInfo,
    JsonTypeInfo<TPassThroughRequest> passThroughRequestJsonTypeInfoForLogging
  )
    where TPassThroughRequest : notnull, ITapoPassThroughRequest
  {
    ThrowIfDisposed();

    // In this implementation, instead of serializing the `TPassThroughRequest`, the `PassThroughRequest`
    // holds a callback delegate that writes directly to the `Utf8JsonWriter`. This enables the
    // PassThroughRequest` to handle any type by performing strictly typed serialization on the
    // callback side, rather than making the `PassThroughRequest` itself a generic type, thereby avoiding
    // the use of reflection.
    return new(
      writeAction: writer => {
        passThroughMessageTransform.ThrowIfDisposed();

        passThroughMessageTransform.WriteEncryptedValue(
          writer: writer,
          value: passThroughRequest ?? throw new ArgumentNullException(nameof(passThroughRequest)),
          jsonTypeInfo: passThroughRequestJsonTypeInfo ?? throw new ArgumentNullException(nameof(passThroughRequestJsonTypeInfo)),
          jsonTypeInfoForLogging: passThroughRequestJsonTypeInfoForLogging ?? throw new ArgumentNullException(nameof(passThroughRequestJsonTypeInfoForLogging))
        );
      }
    );
  }

  internal TPassThroughResponse DecryptPassThroughResponse<TPassThroughResponse>(
    SecurePassThroughResponse securePassThroughResponse,
    JsonTypeInfo<TPassThroughResponse> passThroughResponseJsonTypeInfo)
    where TPassThroughResponse : ITapoPassThroughResponse
  {
    ThrowIfDisposed();

    return passThroughMessageTransform.ReadDecryptedValue(
      encryptedValue: securePassThroughResponse.Result.EncryptedPayload,
      jsonTypeInfo: passThroughResponseJsonTypeInfo ?? throw new ArgumentNullException(nameof(passThroughResponseJsonTypeInfo))
    );
  }
}
