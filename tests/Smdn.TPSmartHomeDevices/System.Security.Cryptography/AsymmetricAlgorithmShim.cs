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
    yield return new object[] { RSA.Create(keySizeInBits: 512) };
    yield return new object[] { RSA.Create(keySizeInBits: 1024) };
    yield return new object[] { RSA.Create(keySizeInBits: 2048) };

    yield return new object[] { DSA.Create(keySizeInBits: 1024) };
    yield return new object[] { DSA.Create(keySizeInBits: 2048) };

    yield return new object[] { ECDsa.Create() };
  }

  [TestCaseSource(nameof(YeildTestCase_ExportSubjectPublicKeyInfoPem))]
  public void ExportSubjectPublicKeyInfoPem(AsymmetricAlgorithm algorithm)
  {
    var expected = string.Concat(
      "-----BEGIN PUBLIC KEY-----\n",
      Convert.ToBase64String(algorithm.ExportSubjectPublicKeyInfo()), "\n",
      "-----END PUBLIC KEY-----\n"
    );

#if !SYSTEM_SECURITY_CRYPTOGRAPHY_ASYMMETRICALGORITHM_EXPORTSUBJECTPUBLICKEYINFOPEM
    Assert.AreEqual(
      expected,
      AsymmetricAlgorithmShim.ExportSubjectPublicKeyInfoPem(algorithm)
    );
#endif

#if SYSTEM_SECURITY_CRYPTOGRAPHY_ASYMMETRICALGORITHM_EXPORTSUBJECTPUBLICKEYINFOPEM
    Assert.AreEqual(
      RemoveLF(expected),
      RemoveLF(algorithm.ExportSubjectPublicKeyInfoPem()),
      "compare with runtime library implementation's output"
    );

    static string RemoveLF(string input)
      => input.Replace("\n", string.Empty);
#endif

    algorithm.Dispose();
  }
}
