using Smdn.TPSmartHomeDevices.Tapo.Protocol;

const string email = "user@mail.test";
const string password = "password";

Console.WriteLine("Base64UserNameSHA1Digest: {0}", TapoCredentialUtils.ToBase64EncodedSHA1DigestString(email));
Console.WriteLine("Base64Password: {0}", TapoCredentialUtils.ToBase64EncodedString(password));
