// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Smdn.Formats;

namespace Smdn.TPSmartHomeDevices.Tapo.Credentials;

/// <summary>
/// Provides functionalities related to the credentials used for authentication in Tapo's communication protocol.
/// </summary>
/// <remarks>
/// This implementation is based on and ported from the following
/// Python implementation by <see href="https://github.com/fishbigger">Toby Johnson</see>:
/// <see href="https://github.com/fishbigger/TapoP100">fishbigger/TapoP100</see>, published under the MIT License.
/// </remarks>
public static partial class TapoCredentials {
#if SYSTEM_SECURITY_CRYPTOGRAPHY_SHA1_HASHSIZEINBYTES
  private const int SHA1HashSizeInBytes = SHA1.HashSizeInBytes;
#else
  private const int SHA1HashSizeInBytes = 160/*bits*/ / 8;
#endif
  public const int HexSHA1HashSizeInBytes = SHA1HashSizeInBytes * 2; // byte array -> hex byte array

  /// <summary>
  /// Hash the string passed by the <paramref name="str"/> with SHA-1 algorithm and converts to the base64 format used for authentication of Tapo devices.
  /// </summary>
  /// <param name="str">The string to convert.</param>
  /// <returns>The <see cref="string"/> containing the result of the conversion.</returns>
  public static string ToBase64EncodedSHA1DigestString(ReadOnlySpan<char> str)
  {
    byte[]? bytes = null;

    try {
      // string -> UTF-8 byte array
      var length = Encoding.UTF8.GetByteCount(str);

      bytes = ArrayPool<byte>.Shared.Rent(length);

      Encoding.UTF8.GetBytes(str, bytes);

      // UTF-8 byte array -> hex SHA-1 hash byte array
      Span<byte> hexSHA1Hash = stackalloc byte[HexSHA1HashSizeInBytes];

      if (!TryConvertToHexSHA1Hash(bytes.AsSpan(0, length), hexSHA1Hash, out _))
        throw new InvalidOperationException("failed to convert hex SHA-1 hash");

      // hex SHA-1 hash byte array -> base64 string
      return Convert.ToBase64String(hexSHA1Hash, Base64FormattingOptions.None);
    }
    finally {
      if (bytes is not null)
        ArrayPool<byte>.Shared.Return(bytes, clearArray: true);
    }
  }

  /// <summary>
  /// Attempts to convert the UTF-8 string passed by the <paramref name="input"/> to the SHA-1 hash represented in the hexadecimal format (base16).
  /// </summary>
  /// <param name="input">The UTF-8 string to convert.</param>
  /// <param name="destination">The buffer to receive the converted value.</param>
  /// <param name="bytesWritten">When this method returns, the total number of bytes written into destination.</param>
  /// <returns><see langword="false"/> if <paramref name="destination"/> is too small to hold the calculated hash, <see langword="true"/> otherwise.</returns>
  public static bool TryConvertToHexSHA1Hash(
    ReadOnlySpan<byte> input,
    Span<byte> destination,
    out int bytesWritten
  )
  {
    bytesWritten = 0;

    if (destination.Length < HexSHA1HashSizeInBytes)
      return false;

    Span<byte> sha1hash = stackalloc byte[SHA1HashSizeInBytes];

    try {
#pragma warning disable CA5350
#if SYSTEM_SECURITY_CRYPTOGRAPHY_SHA1_TRYHASHDATA
      if (!SHA1.TryHashData(input, sha1hash, out var bytesWrittenSHA1))
        return false; // destination too short
#else
      using var sha1 = SHA1.Create();

      if (!sha1.TryComputeHash(input, sha1hash, out var bytesWrittenSHA1))
        return false; // destination too short
#endif
#pragma warning restore CA5350

      if (bytesWrittenSHA1 != SHA1HashSizeInBytes)
        return false; // unexpected state

      /*
        * SHA-1 hash byte array -> hex byte array
        */
      if (!Hexadecimal.TryEncodeLowerCase(sha1hash, destination, out bytesWritten))
        return false; // destination too short

      return true;
    }
    finally {
      sha1hash.Clear();
    }
  }

  public static string ToBase64EncodedString(ReadOnlySpan<char> str)
  {
    byte[]? bytes = null;

    try {
      var length = Encoding.UTF8.GetByteCount(str);

      bytes = ArrayPool<byte>.Shared.Rent(length);

      var bytesWritten = Encoding.UTF8.GetBytes(str, bytes);

      return Convert.ToBase64String(bytes.AsSpan(0, bytesWritten), Base64FormattingOptions.None);
    }
    finally {
      if (bytes is not null)
        ArrayPool<byte>.Shared.Return(bytes, clearArray: true);
    }
  }

  internal static InvalidOperationException CreateExceptionNoCredentialForIdentity(ITapoCredentialIdentity? identity)
    => new($"Could not get a credential for an identity '{identity?.ToString() ?? "(null)"}'");

  internal static ITapoCredentialProvider CreateProviderFromPlainText(string email, string password)
    => new SingleIdentityStringCredentialProvider(
      username: email ?? throw new ArgumentNullException(nameof(email)),
      password: password ?? throw new ArgumentNullException(nameof(password)),
      isPlainText: true
    );

  internal static ITapoCredentialProvider CreateProviderFromBase64EncodedText(
    string base64UserNameSHA1Digest,
    string base64Password
  )
    => new SingleIdentityStringCredentialProvider(
      username: base64UserNameSHA1Digest ?? throw new ArgumentNullException(nameof(base64UserNameSHA1Digest)),
      password: base64Password ?? throw new ArgumentNullException(nameof(base64Password)),
      isPlainText: false
    );

  internal static ITapoCredentialProvider CreateProviderFromEnvironmentVariables(
    string envVarUsername,
    string envVarPassword
  )
  {
    if (string.IsNullOrEmpty(envVarUsername))
      throw new ArgumentException(message: "must be non-empty string", paramName: nameof(envVarUsername));

    if (string.IsNullOrEmpty(envVarPassword))
      throw new ArgumentException(message: "must be non-empty string", paramName: nameof(envVarPassword));

    return new SingleIdentityEnvVarCredentialProvider(
      envVarUsername: envVarUsername,
      envVarPassword: envVarPassword
    );
  }

  private sealed class SingleIdentityStringCredentialProvider : ITapoCredentialProvider, ITapoCredential {
    private readonly byte[] utf8Username;
    private readonly byte[] utf8Password;
    private readonly bool isPlainText;

    public SingleIdentityStringCredentialProvider(
      string username,
      string password,
      bool isPlainText
    )
    {
      utf8Username = Encoding.UTF8.GetBytes(username);
      utf8Password = Encoding.UTF8.GetBytes(password);
      this.isPlainText = isPlainText;
    }

    ITapoCredential ITapoCredentialProvider.GetCredential(ITapoCredentialIdentity? identity) => this;

    void IDisposable.Dispose() { /* nothing to do */ }

    void ITapoCredential.WritePasswordPropertyValue(Utf8JsonWriter writer)
    {
      if (isPlainText)
        writer.WriteBase64StringValue(utf8Password);
      else
        writer.WriteStringValue(utf8Password);
    }

    void ITapoCredential.WriteUsernamePropertyValue(Utf8JsonWriter writer)
    {
      if (isPlainText) {
        Span<byte> buffer = stackalloc byte[HexSHA1HashSizeInBytes];

        try {
          if (!TryConvertToHexSHA1Hash(utf8Username, buffer, out _))
            throw new InvalidOperationException("failed to encode username property");

          writer.WriteBase64StringValue(buffer);
        }
        finally {
          buffer.Clear();
        }
      }
      else {
        writer.WriteStringValue(utf8Username);
      }
    }

    int ITapoCredential.HashPassword(HashAlgorithm algorithm, Span<byte> destination)
      => algorithm.TryComputeHash(utf8Password, destination, out var bytesWritten)
        ? bytesWritten
        : 0;

    int ITapoCredential.HashUsername(HashAlgorithm algorithm, Span<byte> destination)
      => algorithm.TryComputeHash(utf8Username, destination, out var bytesWritten)
        ? bytesWritten
        : 0;
  }

  private sealed class SingleIdentityEnvVarCredentialProvider : ITapoCredentialProvider, ITapoCredential {
    private readonly string envVarUsername;
    private readonly string envVarPassword;

    public SingleIdentityEnvVarCredentialProvider(
      string envVarUsername,
      string envVarPassword
    )
    {
      this.envVarUsername = envVarUsername;
      this.envVarPassword = envVarPassword;
    }

    ITapoCredential ITapoCredentialProvider.GetCredential(ITapoCredentialIdentity? identity) => this;

    void IDisposable.Dispose() { /* nothing to do */ }

    void ITapoCredential.WritePasswordPropertyValue(Utf8JsonWriter writer)
      => ReadEnvVar(
        envVarPassword,
        output: Span<None>.Empty,
        (utf8Password, output) => {
          writer.WriteBase64StringValue(utf8Password);
          return default(None);
        }
      );

    void ITapoCredential.WriteUsernamePropertyValue(Utf8JsonWriter writer)
      => ReadEnvVar(
        envVarUsername,
        output: Span<None>.Empty,
        (utf8Username, output) => {
          Span<byte> buffer = stackalloc byte[HexSHA1HashSizeInBytes];

          try {
            if (!TryConvertToHexSHA1Hash(utf8Username, buffer, out _))
              throw new InvalidOperationException("failed to encode username property");

            writer.WriteBase64StringValue(buffer);

            return default(None);
          }
          finally {
            buffer.Clear();
          }
        }
      );

    int ITapoCredential.HashPassword(HashAlgorithm algorithm, Span<byte> destination)
      => ReadEnvVar(
        envVarPassword,
        destination,
        (utf8Password, dest) => algorithm.TryComputeHash(utf8Password, dest, out var bytesWritten)
          ? bytesWritten
          : 0
      );

    int ITapoCredential.HashUsername(HashAlgorithm algorithm, Span<byte> destination)
      => ReadEnvVar(
        envVarUsername,
        destination,
        (utf8Username, dest) => algorithm.TryComputeHash(utf8Username, dest, out var bytesWritten)
          ? bytesWritten
          : 0
      );

    private delegate TResult ReadEnvVarFunc<T, TOut, TResult>(ReadOnlySpan<T> span, Span<TOut> output);

    private static TResult ReadEnvVar<TOut, TResult>(
      string envVar,
      Span<TOut> output,
      ReadEnvVarFunc<byte, TOut, TResult> func
    )
    {
      byte[]? utf8EncodedEnvVarValue = null;

      try {
        var envVarValue = Environment.GetEnvironmentVariable(envVar);

        if (string.IsNullOrEmpty(envVarValue))
          throw new InvalidOperationException($"envvar '{envVar}' not set");

        utf8EncodedEnvVarValue = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(envVarValue));

        var len = Encoding.UTF8.GetBytes(envVarValue, utf8EncodedEnvVarValue);

        return func(utf8EncodedEnvVarValue.AsSpan(0, len), output);
      }
      finally {
        if (utf8EncodedEnvVarValue is not null)
          ArrayPool<byte>.Shared.Return(utf8EncodedEnvVarValue, clearArray: true);
      }
    }
  }
}
