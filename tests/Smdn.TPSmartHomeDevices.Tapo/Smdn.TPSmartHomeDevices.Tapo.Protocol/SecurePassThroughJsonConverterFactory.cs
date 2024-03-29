// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

using Smdn.IO.Streams;
using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

[TestFixture]
public class SecurePassThroughJsonConverterFactoryTests {
  private class NullTransform : ICryptoTransform {
    public bool CanReuseTransform => true;
    public bool CanTransformMultipleBlocks => true;
    public int InputBlockSize => 1;
    public int OutputBlockSize => 1;
    private bool disposed = false;

    public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
    {
#pragma warning disable CA1513
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
#pragma warning restore CA1513

      Array.Copy(
        sourceArray: inputBuffer,
        sourceIndex: inputOffset,
        destinationArray: outputBuffer,
        destinationIndex: outputOffset,
        length: inputCount
      );

      return inputCount;
    }

    public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
    {
#pragma warning disable CA1513
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
#pragma warning restore CA1513

      var outputBuffer = new byte[inputCount];

      Array.Copy(
        sourceArray: inputBuffer,
        sourceIndex: inputOffset,
        destinationArray: outputBuffer,
        destinationIndex: 0,
        length: inputCount
      );

      return outputBuffer;
    }

    public void Dispose()
    {
      disposed = true;
    }
  }

  private class ThrowExceptionTransform : ICryptoTransform {
    public bool CanReuseTransform => throw new InvalidOperationException();
    public bool CanTransformMultipleBlocks => throw new InvalidOperationException();
    public int InputBlockSize => throw new InvalidOperationException();
    public int OutputBlockSize => throw new InvalidOperationException();

    public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
      => throw new InvalidOperationException();

    public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
      => throw new InvalidOperationException();

    public void Dispose()
      => throw new InvalidOperationException();
  }

  private class ThrowExceptionJsonConverterException : Exception {
  }

  private class ThrowExceptionJsonConverter : JsonConverter<object> {
    public override bool CanConvert(Type typeToConvert) => true; // attemt to convert all object types

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
      => throw new ThrowExceptionJsonConverterException();

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      => throw new ThrowExceptionJsonConverterException();
  }

  private static SecurePassThroughJsonConverterFactory CreateFactory(
    ICryptoTransform? encryptorForPassThroughRequest = null,
    ICryptoTransform? decryptorForPassThroughResponse = null,
    JsonSerializerOptions? baseJsonSerializerOptions = null,
    ILogger? logger = null
  )
    => new(
      identity: null,
      encryptorForPassThroughRequest: encryptorForPassThroughRequest ?? new NullTransform(),
      decryptorForPassThroughResponse: decryptorForPassThroughResponse ?? new NullTransform(),
      baseJsonSerializerOptionsForPassThroughMessage: baseJsonSerializerOptions,
      logger: logger
    );

  private readonly struct NullResult { }

  [TestCase(typeof(HandshakeRequest), false)]
  [TestCase(typeof(HandshakeResponse), false)]
  [TestCase(typeof(LoginDeviceRequest), true)]
  [TestCase(typeof(LoginDeviceResponse), true)]
  [TestCase(typeof(GetDeviceInfoRequest), true)]
  [TestCase(typeof(PassThroughResponse<NullResult>), true)]
  [TestCase(typeof(SecurePassThroughRequest<LoginDeviceRequest>), false)]
  [TestCase(typeof(SecurePassThroughResponse<LoginDeviceResponse>), false)]
  public void CanConvert(Type typeToConvert, bool expected)
    => Assert.That(CreateFactory().CanConvert(typeToConvert), Is.EqualTo(expected));

  [TestCase(typeof(HandshakeRequest))]
  [TestCase(typeof(HandshakeResponse))]
  [TestCase(typeof(SecurePassThroughResponse<LoginDeviceResponse>))]
  [TestCase(typeof(SecurePassThroughRequest<LoginDeviceRequest>))]
  public void CreateConverter_CanNotConvert(Type typeToConvert)
  {
    var factory = CreateFactory();

    Assert.That(factory.CanConvert(typeToConvert), Is.False);

    Assert.That(factory.CreateConverter(typeToConvert, new()), Is.Null);
  }

  [TestCase(typeof(LoginDeviceRequest))]
  [TestCase(typeof(GetDeviceInfoRequest))]
  public void CreateConverter_ITapoPassThroughRequest(Type typeToConvert)
  {
    var factory = CreateFactory();

    Assert.That(factory.CanConvert(typeToConvert), Is.True);

    var converter = factory.CreateConverter(typeToConvert, new());

    Assert.That(converter, Is.Not.Null);
  }

  [TestCase(typeof(LoginDeviceResponse))]
  [TestCase(typeof(PassThroughResponse<NullResult>))]
  public void CreateConverter_ITapoPassThroughResponse(Type typeToConvert)
  {
    var factory = CreateFactory();

    Assert.That(factory.CanConvert(typeToConvert), Is.True);

    var converter = factory.CreateConverter(typeToConvert, new());

    Assert.That(converter, Is.Not.Null);
  }

  [Test]
  public void Dispose_EncryptorForPassThroughRequest()
  {
    var encryptor = new NullTransform();
    var factory = CreateFactory(
      encryptorForPassThroughRequest: encryptor
    );

    var options = new JsonSerializerOptions();

    options.Converters.Add(factory);

    factory.Dispose();

    Assert.Throws<ObjectDisposedException>(() => encryptor.TransformBlock(Array.Empty<byte>(), 0, 0, Array.Empty<byte>(), 0), nameof(encryptor.TransformBlock));

    Assert.Throws<ObjectDisposedException>(() => JsonSerializer.Serialize(new LoginDeviceRequest(), options));
  }

  [Test]
  public void Dispose_DecryptorForPassThroughResponse()
  {
    var decryptor = new NullTransform();
    var factory = CreateFactory(
      decryptorForPassThroughResponse: decryptor
    );

    var options = new JsonSerializerOptions();

    options.Converters.Add(factory);

    factory.Dispose();

    Assert.Throws<ObjectDisposedException>(() => decryptor.TransformBlock(Array.Empty<byte>(), 0, 0, Array.Empty<byte>(), 0), nameof(decryptor.TransformBlock));

    Assert.Throws<ObjectDisposedException>(() => JsonSerializer.Deserialize<LoginDeviceResponse>("\"\"", options));
  }

#pragma warning disable IDE1006
  private readonly record struct SetDeviceInfoRequestPseudoParams(
    bool device_on,
    int brightness
  );
#pragma warning restore IDE1006

  private static System.Collections.IEnumerable YieldTestCases_ConverterForITapoPassThroughRequest()
  {
    var services = new ServiceCollection();

    services.AddTapoCredential(
      email: "user",
      password: "pass"
    );

    var credentialProvider = services.BuildServiceProvider()!.GetRequiredService<ITapoCredentialProvider>();

    yield return new object[] {
      new LoginDeviceRequest(credentialProvider),
      @"{""method"":""login_device"",""params"":{""password"":""cGFzcw=="",""username"":""MTJkZWE5NmZlYzIwNTkzNTY2YWI3NTY5MmM5OTQ5NTk2ODMzYWRjOQ==""},""requestTimeMils"":0}"
    };
    yield return new object[] {
      new GetDeviceInfoRequest(),
      @"{""method"":""get_device_info"",""requestTimeMils"":0}"
    };
    yield return new object[] {
      new SetDeviceInfoRequest<SetDeviceInfoRequestPseudoParams>("uuid", new(true, 100)),
      @"{""method"":""set_device_info"",""requestTimeMils"":0,""terminalUUID"":""uuid"",""params"":{""device_on"":true,""brightness"":100}}"
    };
  }

  [TestCaseSource(nameof(YieldTestCases_ConverterForITapoPassThroughRequest))]
  public void ConverterForITapoPassThroughRequest(
    ITapoPassThroughRequest request,
    string expectedJsonExpression
  )
  {
    var options = new JsonSerializerOptions();

    options.Converters.Add(CreateFactory());

    var expectedJsonExpressionBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedJsonExpression));

    Assert.That(
      JsonSerializer.Serialize(request, request.GetType(), options),
      Is.EqualTo("\"" + expectedJsonExpressionBase64 + "\"")
    );
  }

  [TestCaseSource(nameof(YieldTestCases_ConverterForITapoPassThroughRequest))]
  public void ConverterForITapoPassThroughRequest_Encrypting(
    ITapoPassThroughRequest request,
    string expectedJsonExpression
  )
  {
    var aes = Aes.Create();

    aes.Padding = PaddingMode.PKCS7;

    var rng = RandomNumberGenerator.Create();
    var key = new byte[16];
    var iv = new byte[16];

    rng.GetBytes(key);
    rng.GetBytes(iv);

    var options = new JsonSerializerOptions();

    options.Converters.Add(
      CreateFactory(
        encryptorForPassThroughRequest: aes.CreateEncryptor(key, iv),
        decryptorForPassThroughResponse: new ThrowExceptionTransform() // decryption must not be performed
      )
    );

    var serializedRequestStream = new MemoryStream();

    JsonSerializer.Serialize(serializedRequestStream, request, request.GetType(), options);

    serializedRequestStream.Position = 0L;

    Assert.That(
      serializedRequestStream.ReadByte(),
      Is.EqualTo((byte)'"'),
      "first byte of serialized request"
    );

    serializedRequestStream.Position = serializedRequestStream.Length - 1L;

    Assert.That(
      serializedRequestStream.ReadByte(),
      Is.EqualTo((byte)'"'),
      "last byte of serialized request"
    );

    serializedRequestStream.Position = 0L;

    var dequotingStream = new PartialStream(
      serializedRequestStream,
      1,
      serializedRequestStream.Length - 2
    );
    var fromBase64Stream = new CryptoStream(
      dequotingStream,
      new FromBase64Transform(),
      CryptoStreamMode.Read
    );
    var decryptingStream = new CryptoStream(
      fromBase64Stream,
      aes.CreateDecryptor(key, iv),
      CryptoStreamMode.Read
    );

    Assert.That(
      new StreamReader(decryptingStream).ReadToEnd(),
      Is.EqualTo(expectedJsonExpression)
    );
  }

  private readonly struct GetDeviceInfoResponseResult {
    [JsonPropertyName("device_id")]
    public string? Id { get; init; }

    [JsonPropertyName("device_on")]
    public bool IsOn { get; init; }

    [JsonPropertyName("nickname")]
    public string? NickName { get; init; }
  }

  [TestCaseSource(nameof(YieldTestCases_ConverterForITapoPassThroughRequest))]
  public void ConverterForITapoPassThroughRequest_OptionsSuppliedByFactoryMustBeUsed(
    ITapoPassThroughRequest request,
    string _
  )
  {
    var optionsForITapoPassThroughRequest = new JsonSerializerOptions();

    optionsForITapoPassThroughRequest.Converters.Add(new ThrowExceptionJsonConverter());

    var options = new JsonSerializerOptions();

    options.Converters.Add(
      CreateFactory(
        encryptorForPassThroughRequest: null,
        decryptorForPassThroughResponse: null,
        baseJsonSerializerOptions: optionsForITapoPassThroughRequest
      )
    );

    Assert.Throws<ThrowExceptionJsonConverterException>(() => JsonSerializer.Serialize(request, request.GetType(), options));
  }

  private readonly struct SetDeviceInfoResponseResult {
    [JsonPropertyName("foo")] public int Foo { get; init; }
    [JsonPropertyName("bar")] public string? Bar { get; init; }
  }

  private static System.Collections.IEnumerable YieldTestCases_ConverterForITapoPassThroughResponse()
  {
    yield return new object[] {
      typeof(LoginDeviceResponse),
      @"{""error_code"":0,""result"":{""token"":""TOKEN""}}",
      new Action<ITapoPassThroughResponse>(
        static (ITapoPassThroughResponse deserialized) => {
          Assert.That(deserialized, Is.InstanceOf<LoginDeviceResponse>());

          var resp = (LoginDeviceResponse)deserialized;
          Assert.That(resp.ErrorCode, Is.EqualTo(0), nameof(LoginDeviceResponse.ErrorCode));
          Assert.That(resp.Result.Token, Is.EqualTo("TOKEN"), nameof(LoginDeviceResponse.ResponseResult.Token));
        }
      )
    };

    yield return new object[] {
      typeof(PassThroughResponse<GetDeviceInfoResponseResult>),
      @"{""error_code"":-1,""result"":{""device_id"":""<device-id>"",""device_on"":true}}",
      new Action<ITapoPassThroughResponse>(
        static (ITapoPassThroughResponse deserialized) => {
          Assert.That(deserialized, Is.InstanceOf<PassThroughResponse<GetDeviceInfoResponseResult>>());

          var resp = (PassThroughResponse<GetDeviceInfoResponseResult>)deserialized;
          Assert.That(resp.ErrorCode, Is.EqualTo(-1), nameof(PassThroughResponse<GetDeviceInfoResponseResult>.ErrorCode));
          Assert.That(resp.Result.Id, Is.EqualTo("<device-id>"), nameof(PassThroughResponse<GetDeviceInfoResponseResult>.Result.Id));
          Assert.That(resp.Result.IsOn, Is.True, nameof(PassThroughResponse<GetDeviceInfoResponseResult>.Result.IsOn));
          Assert.That(resp.Result.NickName, Is.Null, nameof(PassThroughResponse<GetDeviceInfoResponseResult>.Result.NickName));
        }
      )
    };

    yield return new object[] {
      typeof(PassThroughResponse<SetDeviceInfoResponseResult>),
      @"{""error_code"":1,""result"":{""foo"":0,""bar"":""baz"",""qux"":""extra property""}}",
      new Action<ITapoPassThroughResponse>(
        static (ITapoPassThroughResponse deserialized) => {
          Assert.That(deserialized, Is.InstanceOf<PassThroughResponse<SetDeviceInfoResponseResult>>());

          var resp = (PassThroughResponse<SetDeviceInfoResponseResult>)deserialized;

          Assert.That(resp.ErrorCode, Is.EqualTo(1), nameof(PassThroughResponse<SetDeviceInfoResponseResult>.ErrorCode));
          Assert.That(resp.Result.Foo, Is.EqualTo(0), nameof(PassThroughResponse<SetDeviceInfoResponseResult>.Result.Foo));
          Assert.That(resp.Result.Bar, Is.EqualTo("baz"), nameof(PassThroughResponse<SetDeviceInfoResponseResult>.Result.Bar));
        }
      )
    };
  }

  [TestCaseSource(nameof(YieldTestCases_ConverterForITapoPassThroughResponse))]
  public void ConverterForITapoPassThroughResponse(
    Type returnType,
    string rawJsonExpression,
    Action<ITapoPassThroughResponse> assert
  )
  {
    var options = new JsonSerializerOptions();

    options.Converters.Add(CreateFactory());

    var inputJsonExpressionBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawJsonExpression));

    var deserialized = JsonSerializer.Deserialize(
      "\"" + inputJsonExpressionBase64 + "\"",
      returnType,
      options
    );

    Assert.That(deserialized, Is.Not.Null, nameof(deserialized));
    Assert.That(deserialized, Is.InstanceOf(returnType), nameof(deserialized));

    assert((ITapoPassThroughResponse)deserialized!);
  }

  [TestCaseSource(nameof(YieldTestCases_ConverterForITapoPassThroughResponse))]
  public void ConverterForITapoPassThroughResponse_Decrypting(
    Type returnType,
    string rawJsonExpression,
    Action<ITapoPassThroughResponse> assert
  )
  {
    var aes = Aes.Create();

    aes.Padding = PaddingMode.PKCS7;

    var rng = RandomNumberGenerator.Create();
    var key = new byte[16];
    var iv = new byte[16];

    rng.GetBytes(key);
    rng.GetBytes(iv);

    var options = new JsonSerializerOptions();

    options.Converters.Add(
      CreateFactory(
        encryptorForPassThroughRequest: new ThrowExceptionTransform(), // encryption must not be performed,
        decryptorForPassThroughResponse: aes.CreateDecryptor(key, iv)
      )
    );

    using var inputStream = new MemoryStream();

    inputStream.WriteByte((byte)'"');

    var toBase64Stream = new CryptoStream(
      inputStream,
      new ToBase64Transform(),
      CryptoStreamMode.Write,
      leaveOpen: true
    );
    var encryptingStream = new CryptoStream(
      toBase64Stream,
      aes.CreateEncryptor(key, iv),
      CryptoStreamMode.Write,
      leaveOpen: true
    );

    encryptingStream.Write(Encoding.UTF8.GetBytes(rawJsonExpression));
    encryptingStream.Close();
    toBase64Stream.Close();

    inputStream.WriteByte((byte)'"');
    inputStream.Position = 0L;

    var deserialized = JsonSerializer.Deserialize(
      inputStream,
      returnType,
      options
    );

    Assert.That(deserialized, Is.Not.Null, nameof(deserialized));
    Assert.That(deserialized, Is.InstanceOf(returnType), nameof(deserialized));

    assert((ITapoPassThroughResponse)deserialized!);
  }

  private class NullLogger : ILogger {
    public IDisposable BeginScope<TState>(TState state) => NullLoggerScope.Instance; // do nothing
    public bool IsEnabled(LogLevel logLevel) => true; // enable all log level
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { } // do nothing
  }

  private static System.Collections.IEnumerable YieldTestCases_ConverterForITapoPassThroughResponse_Decrypting_InvalidPadding()
  {
    yield return new object?[] { @"{""error_code"":0,""result"":{""token"":""TOKEN""}}", typeof(LoginDeviceResponse), null };
    yield return new object?[] { @"{""error_code"":0,""result"":{""token"":""TOKEN""}}", typeof(LoginDeviceResponse), new NullLogger() };
  }

  [TestCaseSource(nameof(YieldTestCases_ConverterForITapoPassThroughResponse_Decrypting_InvalidPadding))]
  public void ConverterForITapoPassThroughResponse_Decrypting_InvalidPadding(
    string rawJsonExpression,
    Type returnType,
    ILogger? logger
  )
  {
    var rng = RandomNumberGenerator.Create();
    var key = new byte[16];
    var iv = new byte[16];

    rng.GetBytes(key);
    rng.GetBytes(iv);

    var aesDecrypting = Aes.Create();
    var aesEncrypting = Aes.Create();

    // reproduce a case when the padding is invalid
    aesDecrypting.Padding = PaddingMode.PKCS7;
    aesEncrypting.Padding = PaddingMode.ISO10126;

    var options = new JsonSerializerOptions();

    options.Converters.Add(
      CreateFactory(
        encryptorForPassThroughRequest: new ThrowExceptionTransform(), // encryption must not be performed,
        decryptorForPassThroughResponse: aesDecrypting.CreateDecryptor(key, iv),
        logger: logger
      )
    );

    using var inputStream = new MemoryStream();

    inputStream.WriteByte((byte)'"');

    var toBase64Stream = new CryptoStream(
      inputStream,
      new ToBase64Transform(),
      CryptoStreamMode.Write,
      leaveOpen: true
    );
    var encryptingStream = new CryptoStream(
      toBase64Stream,
      aesEncrypting.CreateEncryptor(key, iv),
      CryptoStreamMode.Write,
      leaveOpen: true
    );

    encryptingStream.Write(Encoding.UTF8.GetBytes(rawJsonExpression));
    encryptingStream.Close();
    toBase64Stream.Close();

    inputStream.WriteByte((byte)'"');
    inputStream.Position = 0L;

    Assert.Throws<SecurePassThroughInvalidPaddingException>(
      () => JsonSerializer.Deserialize(
        inputStream,
        returnType,
        options
      )
    );
  }

  [TestCaseSource(nameof(YieldTestCases_ConverterForITapoPassThroughResponse))]
  public void ConverterForITapoPassThroughResponse_OptionsSuppliedByFactoryMustBeUsed(
    Type returnType,
    string rawJsonExpression,
    Action<ITapoPassThroughResponse> _
  )
  {
    var optionsForITapoPassThroughRequest = new JsonSerializerOptions();

    optionsForITapoPassThroughRequest.Converters.Add(new ThrowExceptionJsonConverter());

    var options = new JsonSerializerOptions();

    options.Converters.Add(
      CreateFactory(
        encryptorForPassThroughRequest: null,
        decryptorForPassThroughResponse: null,
        baseJsonSerializerOptions: optionsForITapoPassThroughRequest
      )
    );

    Assert.Throws<ThrowExceptionJsonConverterException>(() => {
      JsonSerializer.Deserialize(
        "\"" + Convert.ToBase64String(Encoding.UTF8.GetBytes(rawJsonExpression)) + "\"",
        returnType,
        options
      );
    });
  }

  private static System.Collections.IEnumerable YieldTestCases_ConverterForITapoPassThroughResponse_Decrypting_Null()
  {
    yield return new object?[] { @"null", typeof(LoginDeviceResponse), null };
    yield return new object?[] { @"null", typeof(LoginDeviceResponse), new NullLogger() };
    yield return new object?[] { @"{""error_code"":0,""result"":null}", typeof(LoginDeviceResponse), null };
    yield return new object?[] { @"{""error_code"":0,""result"":null}", typeof(LoginDeviceResponse), new NullLogger() };
  }

  [TestCaseSource(nameof(YieldTestCases_ConverterForITapoPassThroughResponse_Decrypting_Null))]
  public void ConverterForITapoPassThroughResponse_Decrypting_Null(
    string rawJsonExpression,
    Type returnType,
    ILogger? logger
  )
  {
    var rng = RandomNumberGenerator.Create();
    var key = new byte[16];
    var iv = new byte[16];

    rng.GetBytes(key);
    rng.GetBytes(iv);

    var aes = Aes.Create();

    aes.Padding = PaddingMode.PKCS7;

    var options = new JsonSerializerOptions();

    options.Converters.Add(
      CreateFactory(
        encryptorForPassThroughRequest: new ThrowExceptionTransform(), // encryption must not be performed,
        decryptorForPassThroughResponse: aes.CreateDecryptor(key, iv),
        logger: logger
      )
    );

    using var inputStream = new MemoryStream();

    inputStream.WriteByte((byte)'"');

    var toBase64Stream = new CryptoStream(
      inputStream,
      new ToBase64Transform(),
      CryptoStreamMode.Write,
      leaveOpen: true
    );
    var encryptingStream = new CryptoStream(
      toBase64Stream,
      aes.CreateEncryptor(key, iv),
      CryptoStreamMode.Write,
      leaveOpen: true
    );

    encryptingStream.Write(Encoding.UTF8.GetBytes(rawJsonExpression));
    encryptingStream.Close();
    toBase64Stream.Close();

    inputStream.WriteByte((byte)'"');
    inputStream.Position = 0L;

    // Confirm that at least an exception other than CryptographicException is thrown,
    // since the case where JsonSerializer returns null cannot be reproduced.
    var ex = Assert.Catch(
      () => JsonSerializer.Deserialize(
        inputStream,
        returnType,
        options
      )
    );

    Assert.That(ex, Is.Not.InstanceOf<CryptographicException>());
  }
}
