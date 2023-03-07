// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.IO;
using System.Text;

namespace System.Security.Cryptography;

internal static class AsymmetricAlgorithmShim {
#if !SYSTEM_SECURITY_CRYPTOGRAPHY_ASYMMETRICALGORITHM_EXPORTSUBJECTPUBLICKEYINFOPEM
  public static string ExportSubjectPublicKeyInfoPem(AsymmetricAlgorithm algorithm)
    => ExportPem(
      header: "-----BEGIN PUBLIC KEY-----",
      footer: "-----END PUBLIC KEY-----",
      key: (algorithm ?? throw new ArgumentNullException(nameof(algorithm))).ExportSubjectPublicKeyInfo()
    );

  private static string ExportPem(string header, string footer, byte[] key)
  {
    using var stream = new MemoryStream(capacity: header.Length + footer.Length + (key.Length * 2));
    var writer = new StreamWriter(stream: stream, encoding: Encoding.ASCII, leaveOpen: true, bufferSize: 1024) {
      NewLine = "\n",
    };

    writer.WriteLine(header);
    writer.Flush();

    using var base64Stream = new CryptoStream(
      stream,
      new ToBase64Transform(),
      CryptoStreamMode.Write,
      leaveOpen: true
    );

    // TODO: write newline per 48 bytes
    base64Stream.Write(key, 0, key.Length);
    base64Stream.Close();

    writer.WriteLine();

    writer.WriteLine(footer);
    writer.Close();

    stream.Position = 0L;

    using var reader = new StreamReader(stream, Encoding.ASCII);

    return reader.ReadToEnd();
  }
#endif
}
