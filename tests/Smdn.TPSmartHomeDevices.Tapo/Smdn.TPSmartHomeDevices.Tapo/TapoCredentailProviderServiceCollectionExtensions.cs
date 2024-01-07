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

  private const string KlapLocalAuthHash = "0F2256EF19E6AC29DAD1F079E98FB53B7DDE48FB4E09ECDFB0B712F20C906F4F";

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

  private static string GetKlapLocalAuthHash(
    ITapoCredentialProvider provider,
    ITapoCredentialIdentity? identity
  )
  {
    using var credential = provider.GetKlapCredential(identity: identity);

    Span<byte> destination = stackalloc byte[32]; // SHA256.HashSizeInBytes

    credential.WriteLocalAuthHash(destination);

    return Convert.ToHexString(destination);
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

    Assert.That(credentialProvider, Is.Not.Null, nameof(credentialProvider));

    var (username, password) = GetEncodedCredential(credentialProvider, identity: null);

    Assert.That(username, Is.EqualTo(Base64UserNameSHA1Digest));
    Assert.That(password, Is.EqualTo(Base64Password));

    Assert.That(GetKlapLocalAuthHash(credentialProvider, identity: null), Is.EqualTo(KlapLocalAuthHash));
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

    Assert.That(credentialProvider, Is.Not.Null, nameof(credentialProvider));

    var (username, password) = GetEncodedCredential(credentialProvider, identity: null);

    Assert.That(username, Is.EqualTo(Base64UserNameSHA1Digest));
    Assert.That(password, Is.EqualTo(Base64Password));

    Assert.That(GetKlapLocalAuthHash(credentialProvider, identity: null), Is.EqualTo(KlapLocalAuthHash));
  }

  [TestCase(EMail, null)]
  [TestCase(null, Password)]
  public void AddTapoCredential_ArgumentNull(string? email, string? password)
  {
    var services = new ServiceCollection();

    Assert.Throws<ArgumentNullException>(
      () => services.AddTapoCredential(
        email: email!,
        password: password!
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

    Assert.That(credentialProvider, Is.Not.Null, nameof(credentialProvider));

    var (username, password) = GetEncodedCredential(credentialProvider, identity: null);

    Assert.That(username, Is.EqualTo(Base64UserNameSHA1Digest));
    Assert.That(password, Is.EqualTo(Base64Password));

    Assert.That(() => GetKlapLocalAuthHash(credentialProvider, identity: null), Throws.TypeOf<NotSupportedException>());
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

    Assert.That(credentialProvider, Is.Not.Null, nameof(credentialProvider));

    var (username, password) = GetEncodedCredential(credentialProvider, identity: null);

    Assert.That(username, Is.EqualTo(Base64UserNameSHA1Digest));
    Assert.That(password, Is.EqualTo(Base64Password));

    Assert.That(() => GetKlapLocalAuthHash(credentialProvider, identity: null), Throws.TypeOf<NotSupportedException>());
  }

  [TestCase(Base64UserNameSHA1Digest, null)]
  [TestCase(null, Base64Password)]
  public void AddTapoBase64EncodedCredential_ArgumentNull(string? base64UserNameSHA1Digest, string? base64Password)
  {
    var services = new ServiceCollection();

    Assert.Throws<ArgumentNullException>(
      () => services.AddTapoBase64EncodedCredential(
        base64UserNameSHA1Digest: base64UserNameSHA1Digest!,
        base64Password: base64Password!
      )
    );
  }

  [Test]
  public void AddTapoCredentialFromEnvironmentVariable()
  {
    const string EnvVarUsername = "SMDN_TPSMARTHOMEDEVICES_TAPO_CREDENTIAL_EMAIL";
    const string EnvVarPassword = "SMDN_TPSMARTHOMEDEVICES_TAPO_CREDENTIAL_PASSWORD";

    var services = new ServiceCollection();

    Assert.DoesNotThrow(
      () => services.AddTapoCredentialFromEnvironmentVariable(
        envVarUsername: EnvVarUsername,
        envVarPassword: EnvVarPassword
      )
    );

    var credentialProvider = services.BuildServiceProvider().GetRequiredService<ITapoCredentialProvider>();

    Assert.That(credentialProvider, Is.Not.Null, nameof(credentialProvider));

    try {
      Environment.SetEnvironmentVariable(EnvVarUsername, EMail);
      Environment.SetEnvironmentVariable(EnvVarPassword, Password);

      var (username, password) = GetEncodedCredential(credentialProvider, identity: null);

      Assert.That(username, Is.EqualTo(Base64UserNameSHA1Digest));
      Assert.That(password, Is.EqualTo(Base64Password));

      Assert.That(GetKlapLocalAuthHash(credentialProvider, identity: null), Is.EqualTo(KlapLocalAuthHash));
    }
    finally {
      Environment.SetEnvironmentVariable(EnvVarUsername, null);
      Environment.SetEnvironmentVariable(EnvVarPassword, null);
    }
  }

  [TestCase(EMail, null)]
  [TestCase(null, Password)]
  public void AddTapoCredentialFromEnvironmentVariable_EnvVarNotSet(string? username, string? password)
  {
    const string EnvVarUsername = "SMDN_TPSMARTHOMEDEVICES_TAPO_CREDENTIAL_EMAIL";
    const string EnvVarPassword = "SMDN_TPSMARTHOMEDEVICES_TAPO_CREDENTIAL_PASSWORD";

    var services = new ServiceCollection();

    Assert.DoesNotThrow(
      () => services.AddTapoCredentialFromEnvironmentVariable(
        envVarUsername: EnvVarUsername,
        envVarPassword: EnvVarPassword
      )
    );

    var credentialProvider = services.BuildServiceProvider().GetRequiredService<ITapoCredentialProvider>();

    Assert.That(credentialProvider, Is.Not.Null, nameof(credentialProvider));

    try {
      Environment.SetEnvironmentVariable(EnvVarUsername, username);
      Environment.SetEnvironmentVariable(EnvVarPassword, password);

      Assert.Throws<InvalidOperationException>(
        () => GetEncodedCredential(credentialProvider, identity: null)
      );

      Assert.Throws<InvalidOperationException>(
        () => GetKlapLocalAuthHash(credentialProvider, identity: null)
      );
    }
    finally {
      Environment.SetEnvironmentVariable(EnvVarUsername, null);
      Environment.SetEnvironmentVariable(EnvVarPassword, null);
    }
  }

  [Test]
  public void AddTapoCredentialFromEnvironmentVariable_TryAddMultiple()
  {
    const string EnvVarUsername = "SMDN_TPSMARTHOMEDEVICES_TAPO_CREDENTIAL_EMAIL";
    const string EnvVarPassword = "SMDN_TPSMARTHOMEDEVICES_TAPO_CREDENTIAL_PASSWORD";

    var services = new ServiceCollection();

    Assert.DoesNotThrow(
      () => services.AddTapoCredentialFromEnvironmentVariable(
        envVarUsername: EnvVarUsername,
        envVarPassword: EnvVarPassword
      )
    );

    Assert.DoesNotThrow(
      () => services.AddTapoCredentialFromEnvironmentVariable(
        envVarUsername: "this_must_not_be_selected",
        envVarPassword: "this_must_not_be_selected"
      )
    );

    var credentialProvider = services.BuildServiceProvider().GetRequiredService<ITapoCredentialProvider>();

    Assert.That(credentialProvider, Is.Not.Null, nameof(credentialProvider));

    try {
      Environment.SetEnvironmentVariable(EnvVarUsername, EMail);
      Environment.SetEnvironmentVariable(EnvVarPassword, Password);

      var (username, password) = GetEncodedCredential(credentialProvider, identity: null);

      Assert.That(username, Is.EqualTo(Base64UserNameSHA1Digest));
      Assert.That(password, Is.EqualTo(Base64Password));

      Assert.That(GetKlapLocalAuthHash(credentialProvider, identity: null), Is.EqualTo(KlapLocalAuthHash));
    }
    finally {
      Environment.SetEnvironmentVariable(EnvVarUsername, null);
      Environment.SetEnvironmentVariable(EnvVarPassword, null);
    }
  }

  [TestCase("SMDN_TPSMARTHOMEDEVICES_TAPO_CREDENTIAL_EMAIL", null)]
  [TestCase("SMDN_TPSMARTHOMEDEVICES_TAPO_CREDENTIAL_EMAIL", "")]
  [TestCase(null, "SMDN_TPSMARTHOMEDEVICES_TAPO_CREDENTIAL_PASSWORD")]
  [TestCase("", "SMDN_TPSMARTHOMEDEVICES_TAPO_CREDENTIAL_PASSWORD")]
  public void AddTapoCredentialFromEnvironmentVariable_ArgumentNullOrEmpty(string? envVarUsername, string? envVarPassword)
  {
    var services = new ServiceCollection();

    Assert.Throws<ArgumentException>(
      () => services.AddTapoCredentialFromEnvironmentVariable(
        envVarUsername: envVarUsername!,
        envVarPassword: envVarPassword!
      )
    );
  }

  private class ConcreteTapoCredentialProvider : ITapoCredentialProvider {
    public ITapoCredential GetCredential(ITapoCredentialIdentity? identity)
      => throw new NotSupportedException();

    public ITapoKlapCredential GetKlapCredential(ITapoCredentialIdentity? identity)
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

    Assert.That(registeredCredentialProvider, Is.Not.Null, nameof(registeredCredentialProvider));
    Assert.That(registeredCredentialProvider, Is.SameAs(credentialProvider));
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

    Assert.That(registeredCredentialProvider, Is.Not.Null, nameof(registeredCredentialProvider));
    Assert.That(registeredCredentialProvider, Is.SameAs(firstCredentialProvider));
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
