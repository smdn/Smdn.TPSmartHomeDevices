// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Smdn.TPSmartHomeDevices.Tapo;

[TestFixture]
public class TapoCredentailProviderServiceCollectionExtensionsTests {
  private const string EMail = "user@mail.test";
  private const string Password = "password";

  private const string Base64UserNameSHA1Digest = "YjhlY2VjNWIzNjk0ZTVlNzE0YTYxMmNhZTZlZTJiNmExMjQ5ZmZmZQ==";
  private const string Base64Password = "cGFzc3dvcmQ=";

  [Test]
  public void AddTapoCredential()
  {
    var services = new ServiceCollection();

    services.AddTapoCredential(
      email: EMail,
      password: Password
    );

    var cred = services.BuildServiceProvider().GetRequiredService<ITapoCredentialProvider>();

    Assert.IsNotNull(cred, nameof(cred));
    Assert.AreEqual(Base64UserNameSHA1Digest, cred.GetBase64EncodedUserNameSHA1Digest(host: string.Empty));
    Assert.AreEqual(Base64Password, cred.GetBase64EncodedPassword(host: string.Empty));
  }

  [Test]
  public void AddTapoCredential_TryAddMultiple()
  {
    var services = new ServiceCollection();

    services.AddTapoCredential(
      email: EMail,
      password: Password
    );
    services.AddTapoCredential(
      email: "this must not be selected",
      password: "this must not be selected"
    );

    var cred = services.BuildServiceProvider().GetRequiredService<ITapoCredentialProvider>();

    Assert.IsNotNull(cred, nameof(cred));
    Assert.AreEqual(Base64UserNameSHA1Digest, cred.GetBase64EncodedUserNameSHA1Digest(host: string.Empty));
    Assert.AreEqual(Base64Password, cred.GetBase64EncodedPassword(host: string.Empty));
  }

  [TestCase(EMail, null)]
  [TestCase(null, Password)]
  public void AddTapoCredential_ArgumentNull(string email, string password)
  {
    var services = new ServiceCollection();

    Assert.Throws<ArgumentNullException>(
      () => services.AddTapoCredential(
        email: email,
        password: password
      )
    );
  }

  [Test]
  public void AddTapoBase64EncodedCredential()
  {
    var services = new ServiceCollection();

    services.AddTapoBase64EncodedCredential(
      base64UserNameSHA1Digest: Base64UserNameSHA1Digest,
      base64Password: Base64Password
    );

    var cred = services.BuildServiceProvider().GetRequiredService<ITapoCredentialProvider>();

    Assert.IsNotNull(cred, nameof(cred));
    Assert.AreEqual(Base64UserNameSHA1Digest, cred.GetBase64EncodedUserNameSHA1Digest(host: string.Empty));
    Assert.AreEqual(Base64Password, cred.GetBase64EncodedPassword(host: string.Empty));
  }

  [Test]
  public void AddTapoBase64EncodedCredential_TryAddMultiple()
  {
    var services = new ServiceCollection();

    services.AddTapoBase64EncodedCredential(
      base64UserNameSHA1Digest: Base64UserNameSHA1Digest,
      base64Password: Base64Password
    );
    services.AddTapoBase64EncodedCredential(
      base64UserNameSHA1Digest: "this must not be selected",
      base64Password: "this must not be selected"
    );

    var cred = services.BuildServiceProvider().GetRequiredService<ITapoCredentialProvider>();

    Assert.IsNotNull(cred, nameof(cred));
    Assert.AreEqual(Base64UserNameSHA1Digest, cred.GetBase64EncodedUserNameSHA1Digest(host: string.Empty));
    Assert.AreEqual(Base64Password, cred.GetBase64EncodedPassword(host: string.Empty));
  }

  [TestCase(Base64UserNameSHA1Digest, null)]
  [TestCase(null, Base64Password)]
  public void AddTapoBase64EncodedCredential_ArgumentNull(string base64UserNameSHA1Digest, string base64Password)
  {
    var services = new ServiceCollection();

    Assert.Throws<ArgumentNullException>(
      () => services.AddTapoBase64EncodedCredential(
        base64UserNameSHA1Digest: base64UserNameSHA1Digest,
        base64Password: base64Password
      )
    );
  }
}
