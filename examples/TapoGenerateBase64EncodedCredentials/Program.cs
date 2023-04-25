using Smdn.TPSmartHomeDevices.Tapo.Credentials;

const string email = "user@mail.test";
const string password = "password";

Console.WriteLine("Base64UserNameSHA1Digest: {0}", TapoCredentials.ToBase64EncodedSHA1DigestString(email));
Console.WriteLine("Base64Password: {0}", TapoCredentials.ToBase64EncodedString(password));
