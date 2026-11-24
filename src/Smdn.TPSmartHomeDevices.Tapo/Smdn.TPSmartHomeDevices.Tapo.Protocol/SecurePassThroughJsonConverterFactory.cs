// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE || SYSTEM_DIAGNOSTICS_CODEANALYSIS_UNCONDITIONALSUPPRESSMESSAGEATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;

using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// Provides a JsonConverter implementation to (de)serialize the 'request' and 'response' properties of the
/// 'securePassthrough' method, according to the crypto method used in Tapo's communication protocol.
/// </summary>
/// <remarks>
/// This implementation is based on and ported from the following
/// Python implementation by <see href="https://github.com/fishbigger">Toby Johnson</see>:
/// <see href="https://github.com/fishbigger/TapoP100">fishbigger/TapoP100</see>, published under the MIT License.
/// </remarks>
/// <seealso cref="PassThroughMessageTransform"/>
/// <seealso cref="SecurePassThroughInvalidPaddingException"/>
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE
[RequiresDynamicCode(FeatureRequirementMessages.RequiresDynamicCodeJsonConverterFactoryInType)]
[RequiresUnreferencedCode(FeatureRequirementMessages.RequiresUnreferencedCodeJsonConverterFactoryInType)]
#endif
#pragma warning disable IDE0055
public sealed class SecurePassThroughJsonConverterFactory :
  JsonConverterFactory,
  IDisposable,
  SecurePassThroughJsonConverterFactory.IPassThroughObjectJsonConverter
#pragma warning restore IDE0055
{
  private interface IPassThroughObjectJsonConverter {
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE
    [RequiresDynamicCode(FeatureRequirementMessages.RequiresDynamicCodeJsonConverterFactoryInMethod)]
    [RequiresUnreferencedCode(FeatureRequirementMessages.RequiresUnreferencedCodeJsonConverterFactoryInMethod)]
#endif
    void WriteEncryptedValue<TValue>(
      Utf8JsonWriter writer,
      TValue value
    );

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE
    [RequiresDynamicCode(FeatureRequirementMessages.RequiresDynamicCodeJsonConverterFactoryInMethod)]
    [RequiresUnreferencedCode(FeatureRequirementMessages.RequiresUnreferencedCodeJsonConverterFactoryInMethod)]
#endif
    TValue ReadDecryptedValue<TValue>(
      ref Utf8JsonReader reader,
      Type typeToConvert
    ) where TValue : notnull;
  }

  private PassThroughMessageTransform PassThroughMessageTransform {
    get {
      if (field is null)
        throw new ObjectDisposedException(GetType().FullName);

      return field;
    }
    set;
  }

  private readonly bool shouldDisposePassThroughMessageTransform;

  [Obsolete("This constructor overload is exposed for testing purposes only. Avoid using it directly.")]
  public SecurePassThroughJsonConverterFactory(
    ITapoCredentialIdentity? identity,
    ICryptoTransform? encryptorForPassThroughRequest,
    ICryptoTransform? decryptorForPassThroughResponse,
    JsonSerializerOptions? baseJsonSerializerOptionsForPassThroughMessage,
    ILogger? logger = null
  )
    : this(
      passThroughMessageTransform: new(
        identity: identity,
        encryptorForPassThroughRequest: encryptorForPassThroughRequest!,
        decryptorForPassThroughResponse: decryptorForPassThroughResponse!,
        baseJsonSerializerOptionsForPassThroughMessage: baseJsonSerializerOptionsForPassThroughMessage,
        logger: logger
      ),
      shouldDisposePassThroughMessageTransform: true
    )
  {
  }

  internal SecurePassThroughJsonConverterFactory(
    PassThroughMessageTransform passThroughMessageTransform,
    bool shouldDisposePassThroughMessageTransform
  )
  {
    PassThroughMessageTransform = passThroughMessageTransform ?? throw new ArgumentNullException(nameof(passThroughMessageTransform));
    this.shouldDisposePassThroughMessageTransform = shouldDisposePassThroughMessageTransform;
  }

  public void Dispose()
  {
    if (shouldDisposePassThroughMessageTransform)
      PassThroughMessageTransform?.Dispose();

    PassThroughMessageTransform = null!;
  }

  public override bool CanConvert(Type typeToConvert)
    =>
      typeof(ITapoPassThroughRequest).IsAssignableFrom(typeToConvert) ||
      typeof(ITapoPassThroughResponse).IsAssignableFrom(typeToConvert);

  public override JsonConverter? CreateConverter(
    Type typeToConvert,
    JsonSerializerOptions options
  )
    => CanConvert(typeToConvert)
      ? (JsonConverter)Activator.CreateInstance(
          type: typeof(PassThroughObjectJsonConverter<>).MakeGenericType(typeToConvert),
          bindingAttr: BindingFlags.Instance | BindingFlags.Public,
          binder: null,
          args: [this],
          culture: null
        )!
      // should throw exception?
      //   throw new NotSupportedException($"unexpected type to convert: {typeToConvert.FullName}");
      : null;

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE
  [RequiresDynamicCode(FeatureRequirementMessages.RequiresDynamicCodeJsonConverterFactoryInMethod)]
  [RequiresUnreferencedCode(FeatureRequirementMessages.RequiresUnreferencedCodeJsonConverterFactoryInMethod)]
#endif
  void IPassThroughObjectJsonConverter.WriteEncryptedValue<TValue>(
    Utf8JsonWriter writer,
    TValue value
  )
    => PassThroughMessageTransform.WriteEncryptedValueRequiresDynamicCode(
      writer: writer,
      value: value
    );

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE
  [RequiresDynamicCode(FeatureRequirementMessages.RequiresDynamicCodeJsonConverterFactoryInMethod)]
  [RequiresUnreferencedCode(FeatureRequirementMessages.RequiresUnreferencedCodeJsonConverterFactoryInMethod)]
#endif
  TValue IPassThroughObjectJsonConverter.ReadDecryptedValue<TValue>(
    ref Utf8JsonReader reader,
    Type typeToConvert
  )
  {
    if (!reader.TryGetBytesFromBase64(out var base64))
      throw new JsonException("could not read base64 string");

    using var encryptedValueStream = new MemoryStream(base64, writable: false);

    return PassThroughMessageTransform.ReadDecryptedValueRequiresDynamicCode<TValue>(
      encryptedValueStream: encryptedValueStream,
      typeToConvert: typeToConvert
    );
  }

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_REQUIRESUNREFERENCEDCODEATTRIBUTE
  [RequiresDynamicCode(FeatureRequirementMessages.RequiresDynamicCodeJsonConverterFactoryInType)]
  [RequiresUnreferencedCode(FeatureRequirementMessages.RequiresUnreferencedCodeJsonConverterFactoryInType)]
#endif
#pragma warning disable IDE0055, CA1812
  private sealed class PassThroughObjectJsonConverter<TPassThroughObject>
    : JsonConverter<TPassThroughObject>
    // where TPassThroughObject : ITapoPassThroughRequest or ITapoPassThroughResponse
    where TPassThroughObject : notnull
#pragma warning restore IDE0055, CA1812
  {
    private readonly IPassThroughObjectJsonConverter converter;

    public PassThroughObjectJsonConverter(IPassThroughObjectJsonConverter converter)
    {
      this.converter = converter;
    }

    public override void Write(
      Utf8JsonWriter writer,
      TPassThroughObject value,
      JsonSerializerOptions options
    )
      => converter.WriteEncryptedValue(writer, value);

    public override TPassThroughObject? Read(
      ref Utf8JsonReader reader,
      Type typeToConvert,
      JsonSerializerOptions options
    )
      => converter.ReadDecryptedValue<TPassThroughObject>(ref reader, typeToConvert);
  }
}
