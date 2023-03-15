// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Smdn.IO.Streams;

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
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);

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
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);

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
      encryptorForPassThroughRequest: encryptorForPassThroughRequest ?? new NullTransform(),
      decryptorForPassThroughResponse: decryptorForPassThroughResponse ?? new NullTransform(),
      plainTextJsonSerializerOptions: baseJsonSerializerOptions,
      logger: logger
    );

  [TestCase(typeof(HandshakeRequest), false)]
  [TestCase(typeof(HandshakeResponse), false)]
  [TestCase(typeof(LoginDeviceRequest), true)]
  [TestCase(typeof(LoginDeviceResponse), true)]
  [TestCase(typeof(GetDeviceInfoRequest), true)]
  [TestCase(typeof(GetDeviceInfoResponse), true)]
  [TestCase(typeof(SecurePassThroughRequest<LoginDeviceRequest>), false)]
  [TestCase(typeof(SecurePassThroughResponse<LoginDeviceResponse>), false)]
  public void CanConvert(Type typeToConvert, bool expected)
    => Assert.AreEqual(expected, CreateFactory().CanConvert(typeToConvert));

  [TestCase(typeof(HandshakeRequest))]
  [TestCase(typeof(HandshakeResponse))]
  [TestCase(typeof(SecurePassThroughResponse<LoginDeviceResponse>))]
  [TestCase(typeof(SecurePassThroughRequest<LoginDeviceRequest>))]
  public void CreateConverter_CanNotConvert(Type typeToConvert)
  {
    var factory = CreateFactory();

    Assert.IsFalse(factory.CanConvert(typeToConvert));

    Assert.IsNull(factory.CreateConverter(typeToConvert, new()));
  }

  [TestCase(typeof(LoginDeviceRequest))]
  [TestCase(typeof(GetDeviceInfoRequest))]
  public void CreateConverter_ITapoPassThroughRequest(Type typeToConvert)
  {
    var factory = CreateFactory();

    Assert.IsTrue(factory.CanConvert(typeToConvert));

    var converter = factory.CreateConverter(typeToConvert, new());

    Assert.IsNotNull(converter);
  }

  [TestCase(typeof(LoginDeviceResponse))]
  [TestCase(typeof(GetDeviceInfoResponse))]
  public void CreateConverter_ITapoPassThroughResponse(Type typeToConvert)
  {
    var factory = CreateFactory();

    Assert.IsTrue(factory.CanConvert(typeToConvert));

    var converter = factory.CreateConverter(typeToConvert, new());

    Assert.IsNotNull(converter);
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

  private readonly record struct SetDeviceInfoRequestPseudoParams(
    bool device_on,
    int brightness
  );

  private static System.Collections.IEnumerable YieldTestCases_ConverterForITapoPassThroughRequest()
  {
    yield return new object[] {
      new LoginDeviceRequest("pass", "user"),
      @"{""method"":""login_device"",""params"":{""password"":""pass"",""username"":""user""},""requestTimeMils"":0}"
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

    Assert.AreEqual(
      "\"" + expectedJsonExpressionBase64 + "\"",
      JsonSerializer.Serialize(request, request.GetType(), options)
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

    Assert.AreEqual(
      (byte)'"',
      serializedRequestStream.ReadByte(),
      "first byte of serialized request"
    );

    serializedRequestStream.Position = serializedRequestStream.Length - 1L;

    Assert.AreEqual(
      (byte)'"',
      serializedRequestStream.ReadByte(),
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

    Assert.AreEqual(
      expectedJsonExpression,
      new StreamReader(decryptingStream).ReadToEnd()
    );
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

  private static System.Collections.IEnumerable YieldTestCases_ConverterForITapoPassThroughResponse()
  {
    yield return new object[] {
      typeof(LoginDeviceResponse),
      @"{""error_code"":0,""result"":{""token"":""TOKEN""}}",
      new Action<ITapoPassThroughResponse>(
        static (ITapoPassThroughResponse deserialized) => {
          Assert.IsInstanceOf<LoginDeviceResponse>(deserialized);

          var resp = (LoginDeviceResponse)deserialized;
          Assert.AreEqual(0, (int)resp.ErrorCode, nameof(LoginDeviceResponse.ErrorCode));
          Assert.AreEqual("TOKEN", resp.Result.Token, nameof(LoginDeviceResponse.ResponseResult.Token));
        }
      )
    };

    yield return new object[] {
      typeof(GetDeviceInfoResponse),
      @"{""error_code"":-1,""result"":{""device_id"":""<device-id>"",""device_on"":true}}",
      new Action<ITapoPassThroughResponse>(
        static (ITapoPassThroughResponse deserialized) => {
          Assert.IsInstanceOf<GetDeviceInfoResponse>(deserialized);

          var resp = (GetDeviceInfoResponse)deserialized;
          Assert.AreEqual(-1, (int)resp.ErrorCode, nameof(GetDeviceInfoResponse.ErrorCode));
          Assert.IsNotNull(resp.Result, nameof(GetDeviceInfoResponse.Result));
          Assert.AreEqual("<device-id>", resp.Result.Id, nameof(GetDeviceInfoResponse.Result.Id));
          Assert.IsTrue(resp.Result.IsOn, nameof(GetDeviceInfoResponse.Result.IsOn));
          Assert.IsNull(resp.Result.NickName, nameof(GetDeviceInfoResponse.Result.NickName));
        }
      )
    };

    yield return new object[] {
      typeof(SetDeviceInfoResponse),
      @"{""error_code"":1,""result"":{""foo"":0,""bar"":""baz""}}",
      new Action<ITapoPassThroughResponse>(
        static (ITapoPassThroughResponse deserialized) => {
          Assert.IsInstanceOf<SetDeviceInfoResponse>(deserialized);

          var resp = (SetDeviceInfoResponse)deserialized;
          Assert.AreEqual(1, (int)resp.ErrorCode, nameof(SetDeviceInfoResponse.ErrorCode));
          Assert.IsNotNull(resp.Result.ExtraData, nameof(SetDeviceInfoResponse.ResponseResult.ExtraData));
          Assert.AreEqual(2, resp.Result.ExtraData!.Count, nameof(SetDeviceInfoResponse.ResponseResult.ExtraData.Count));
          Assert.AreEqual(0, ((JsonElement)resp.Result.ExtraData["foo"]).GetInt32(), nameof(SetDeviceInfoResponse.ResponseResult.ExtraData));
          Assert.AreEqual("baz", ((JsonElement)resp.Result.ExtraData["bar"]).GetString(), nameof(SetDeviceInfoResponse.ResponseResult.ExtraData));
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

    Assert.IsNotNull(deserialized, nameof(deserialized));
    Assert.IsInstanceOf(returnType, deserialized, nameof(deserialized));

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

    Assert.IsNotNull(deserialized, nameof(deserialized));
    Assert.IsInstanceOf(returnType, deserialized, nameof(deserialized));

    assert((ITapoPassThroughResponse)deserialized!);
  }

  private class NullLogger : ILogger {
    public IDisposable? BeginScope<TState>(TState state) => null; // do nothing
    public bool IsEnabled(LogLevel logLevel) => true; // enable all log level
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { } // do nothing
  }

  private static System.Collections.IEnumerable YieldTestCases_ConverterForITapoPassThroughResponse_Decrypting_InvalidPadding()
  {
    yield return new object?[] { @"{""error_code"":0,""result"":{""token"":""TOKEN""}}", typeof(LoginDeviceResponse), (ILogger?)null };
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
}
