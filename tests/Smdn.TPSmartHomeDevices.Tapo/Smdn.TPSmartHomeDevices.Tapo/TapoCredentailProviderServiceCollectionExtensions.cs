// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Smdn.TPSmartHomeDevices.Tapo.Credentials;

namespace Smdn.TPSmartHomeDevices.Tapo;

[TestFixture]
public class TapoCredentailProviderServiceCollectionExtensionsTests {
  private const string EMail = "user@mail.test";
  private const string Password = "password";

  private const string Base64UserNameSHA1Digest = "YjhlY2VjNWIzNjk0ZTVlNzE0YTYxMmNhZTZlZTJiNmExMjQ5ZmZmZQ==";
  private const string Base64Password = "cGFzc3dvcmQ=";

  private static (string Username, string Password) GetEncodedCredential(
    ITapoCredentialProvider provider,
    ITapoCredentialIdentity? identity
  )
  {
    using var credential = provider.GetCredential(identity: identity);

    using var usernameStream = new MemoryStream(capacity: 64);
    using var usernameWriter = new Utf8JsonWriter(usernameStream);

    credential.WriteUsernamePropertyValue(usernameWriter);

    usernameWriter.Flush();

    using var passwordStream = new MemoryStream(capacity: 32);
    using var passwordWriter = new Utf8JsonWriter(passwordStream);

    credential.WritePasswordPropertyValue(passwordWriter);

    passwordWriter.Flush();

    var quotedUsername = Encoding.UTF8.GetString(usernameStream.ToArray());
    var quotedPassword = Encoding.UTF8.GetString(passwordStream.ToArray());

    if (!(quotedUsername.StartsWith("\"", StringComparison.Ordinal) && quotedUsername.EndsWith("\"", StringComparison.Ordinal)))
      throw new InvalidOperationException($"unexpected username: {quotedUsername}");

    if (!(quotedPassword.StartsWith("\"", StringComparison.Ordinal) && quotedPassword.EndsWith("\"", StringComparison.Ordinal)))
      throw new InvalidOperationException($"unexpected password: {quotedPassword}");

    var username = quotedUsername.Substring(1, quotedUsername.Length - 2);
    var password = quotedPassword.Substring(1, quotedPassword.Length - 2);

    return (username, password);
  }

  [Test]
  public void AddTapoCredential()
  {
    var services = new ServiceCollection();

    services.AddTapoCredential(
      email: EMail,
      password: Password
    );

    var credentialProvider = services.BuildServiceProvider().GetRequiredService<ITapoCredentialProvider>();

    Assert.IsNotNull(credentialProvider, nameof(credentialProvider));

    var (username, password) = GetEncodedCredential(credentialProvider, identity: null);

    Assert.AreEqual(Base64UserNameSHA1Digest, username);
    Assert.AreEqual(Base64Password, password);
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

    var credentialProvider = services.BuildServiceProvider().GetRequiredService<ITapoCredentialProvider>();

    Assert.IsNotNull(credentialProvider, nameof(credentialProvider));

    var (username, password) = GetEncodedCredential(credentialProvider, identity: null);

    Assert.AreEqual(Base64UserNameSHA1Digest, username);
    Assert.AreEqual(Base64Password, password);
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

    var credentialProvider = services.BuildServiceProvider().GetRequiredService<ITapoCredentialProvider>();

    Assert.IsNotNull(credentialProvider, nameof(credentialProvider));

    var (username, password) = GetEncodedCredential(credentialProvider, identity: null);

    Assert.AreEqual(Base64UserNameSHA1Digest, username);
    Assert.AreEqual(Base64Password, password);
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

    var credentialProvider = services.BuildServiceProvider().GetRequiredService<ITapoCredentialProvider>();

    Assert.IsNotNull(credentialProvider, nameof(credentialProvider));

    var (username, password) = GetEncodedCredential(credentialProvider, identity: null);

    Assert.AreEqual(Base64UserNameSHA1Digest, username);
    Assert.AreEqual(Base64Password, password);
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

  private class ConcreteTapoCredentialProvider : ITapoCredentialProvider {
    public ITapoCredential GetCredential(ITapoCredentialIdentity? identity)
      => throw new NotSupportedException();
  }

  [Test]
  public void AddTapoCredentialProvider()
  {
    var services = new ServiceCollection();
    var credentialProvider = new ConcreteTapoCredentialProvider();

    services.AddTapoCredentialProvider(
      credentialProvider: credentialProvider
    );

    var registeredCredentialProvider = services.BuildServiceProvider().GetRequiredService<ITapoCredentialProvider>();

    Assert.IsNotNull(registeredCredentialProvider, nameof(registeredCredentialProvider));
    Assert.AreSame(credentialProvider, registeredCredentialProvider);
  }

  [Test]
  public void AddTapoCredentialProvider_TryAddMultiple()
  {
    var services = new ServiceCollection();
    var firstCredentialProvider = new ConcreteTapoCredentialProvider();
    var secondCredentialProvider = new ConcreteTapoCredentialProvider();

    services.AddTapoCredentialProvider(
      credentialProvider: firstCredentialProvider
    );
    services.AddTapoCredentialProvider(
      credentialProvider: secondCredentialProvider
    );

    var registeredCredentialProvider = services.BuildServiceProvider().GetRequiredService<ITapoCredentialProvider>();

    Assert.IsNotNull(registeredCredentialProvider, nameof(registeredCredentialProvider));
    Assert.AreSame(firstCredentialProvider, registeredCredentialProvider);
  }

  [Test]
  public void AddTapoCredentialProvider_ArgumentNull()
  {
    var services = new ServiceCollection();

    Assert.Throws<ArgumentNullException>(
      () => services.AddTapoCredentialProvider(
        credentialProvider: null!
      )
    );
  }
}
