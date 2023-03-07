// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Security.Cryptography;
using NUnit.Framework;

namespace System.Security.Cryptography;

[TestFixture]

public class AsymmetricAlgorithmShimTests {
#if !SYSTEM_SECURITY_CRYPTOGRAPHY_ASYMMETRICALGORITHM_EXPORTSUBJECTPUBLICKEYINFOPEM
  [Test]
  public void ExportSubjectPublicKeyInfoPem_ArgumentNull()
    => Assert.Throws<ArgumentNullException>(() => AsymmetricAlgorithmShim.ExportSubjectPublicKeyInfoPem(algorithm: null!));
#endif

  private static System.Collections.IEnumerable YeildTestCase_ExportSubjectPublicKeyInfoPem()
  {
    foreach (var (algorithmGenerator, label) in new (Func<AsymmetricAlgorithm>, string) [] {
      (static () => RSA.Create(keySizeInBits: 512), "RSA 512 bits" ),
      (static () => RSA.Create(keySizeInBits: 1024), "RSA 1024 bits" ),
      (static () => RSA.Create(keySizeInBits: 2048), "RSA 2048 bits" ),

      (static () => DSA.Create(keySizeInBits: 1024), "DSA 1024 bits" ),
      (static () => DSA.Create(keySizeInBits: 2048), "DSA 2048 bits" ),

      (static () => ECDsa.Create(), "ECDsa" ),
    }) {
      AsymmetricAlgorithm? algorithm = null;

      try {
        algorithm = algorithmGenerator();
      }
      catch (CryptographicException) {
        // ignore
      }

      if (algorithm is not null)
        yield return new object[] { algorithm, label };
    }
  }

  [TestCaseSource(nameof(YeildTestCase_ExportSubjectPublicKeyInfoPem))]
  public void ExportSubjectPublicKeyInfoPem(AsymmetricAlgorithm algorithm, string label)
  {
    var expected = string.Concat(
      "-----BEGIN PUBLIC KEY-----\n",
      Convert.ToBase64String(algorithm.ExportSubjectPublicKeyInfo()), "\n",
      "-----END PUBLIC KEY-----\n"
    );

#if !SYSTEM_SECURITY_CRYPTOGRAPHY_ASYMMETRICALGORITHM_EXPORTSUBJECTPUBLICKEYINFOPEM
    Assert.AreEqual(
      expected,
      AsymmetricAlgorithmShim.ExportSubjectPublicKeyInfoPem(algorithm),
      $"{nameof(AsymmetricAlgorithmShim.ExportSubjectPublicKeyInfoPem)} ({label})"
    );
#endif

#if SYSTEM_SECURITY_CRYPTOGRAPHY_ASYMMETRICALGORITHM_EXPORTSUBJECTPUBLICKEYINFOPEM
    Assert.AreEqual(
      RemoveLF(expected),
      RemoveLF(algorithm.ExportSubjectPublicKeyInfoPem()),
      $"compare with runtime library implementation's output ({label})"
    );

    static string RemoveLF(string input)
      => input.Replace("\n", string.Empty);
#endif

    algorithm.Dispose();
  }
}
