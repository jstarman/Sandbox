using System;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;


namespace PgpCrypto
{
    public class KeyConverter
    {
        private readonly PgpEncryptionKeys _encryptionKeys;

        public KeyConverter(PgpEncryptionKeys keys)
        {
            _encryptionKeys = keys;
        }

        public RSACryptoServiceProvider GetKeyContainer(string containerName)
        {
            var cspParams = new CspParameters
            {
                KeyContainerName = containerName,
                Flags = CspProviderFlags.UseMachineKeyStore
            };
            var sender = new RSACryptoServiceProvider(cspParams);
            Console.WriteLine("Key retrieved from container : \n {0}", sender.ToXmlString(true));
            return sender;
        }

        public string ConvertPemEncodedKeyToXml()
        {
            var rsaParams = (RsaPrivateCrtKeyParameters) _encryptionKeys.PrivateKey.Key;
            var dotNetParams = DotNetUtilities.ToRSAParameters(rsaParams);
            var rsa = RSA.Create();
            rsa.ImportParameters(dotNetParams);
            return rsa.ToXmlString(true);
        }
    }
}
