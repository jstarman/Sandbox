using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MsCrypto
{
    public class Encryptor
    {
        private string _myRsaKeys = "...";
        private string _receiversPublicKey = "...";

        private const string _rsaKeyForEncryption = @"<RSAKeyValue>
 <Modulus>tBnCNaeE4vVRmCetSN0T5vk0T464xfrLvQaGPzoVmjxVapv/2MhCBa2Y/jBKjTk/g855mI0d0azzCt+fyGwjW5ESCq8f0x2Qio9ARqD47hfSQZdDEQhAd0v6lmOJBudL4XKed6OEpSUiy3sGOzrwgEdgbgx9SlHNPOM4aeNLDNk=</Modulus>
 <Exponent>AQAB</Exponent>
</RSAKeyValue>";

        private const string _rsaKeyForDecryption = @"<RSAKeyValue>
 <Modulus>tBnCNaeE4vVRmCetSN0T5vk0T464xfrLvQaGPzoVmjxVapv/2MhCBa2Y/jBKjTk/g855mI0d0azzCt+fyGwjW5ESCq8f0x2Qio9ARqD47hfSQZdDEQhAd0v6lmOJBudL4XKed6OEpSUiy3sGOzrwgEdgbgx9SlHNPOM4aeNLDNk=</Modulus>
 <Exponent>AQAB</Exponent>
 <P>2H3W8AI2avW5QsolM8s/5bMGsGVXRiuPTeCBzTCeo5DfZKIgH+pLYGWKX4jsba19dxSjBq19orj0zC7N3Vvi/w==</P>
 <Q>1PfJ9/vNI4uuc6K28wu4ztD+BslkjinvuqKKL0K0v5s+Wij/56Nmlw5Wsq6f7SBK/3gPzU6XAl6XFYexCkyIJw==</Q>
 <DP>d4nMk8v09kxmFj3+GsJArvmSWPYsIX/N6gdsRLty7QgCgdOPf2/kXP7UT/oP6mOgYo9PkVFcjOwpp1C0tWSWSw==</DP>
 <DQ>fERuLG5YYBJkZAMhH6bKWpumbo1bSHz97nhji3ov/9H1SyS7Vg2JkkECo9azaOk7+d+z0Me34+EP5bc8vda0Zw==</DQ>
 <InverseQ>Mr9UDFTmNprEoIZj3cKE15gNErUeVNXluce9y9RS1teZ4fZ9s0wroGXX4esEzsKE+kBtBMTjBwkALLm/dJyGGw==</InverseQ>
 <D>j4SrV9hbtASr9eVAWH0cPAZAollocxQtQT+uwTnHNzZ3FtKjqhvDSQUSkxTmg+2n6KAkE+X4ajs5HmQfzzYlYZ9Y6I4wKlelRy6kIwiagRKWqToj2dXam+Yfn6IQEBLswSZO91SpERZJejD06zRfVj3KTDp19NDhpOE/p4LJEbU=</D>
 </RSAKeyValue>";


        private RSACryptoServiceProvider CreateCipherForDecryption()
        {
            var cipher = new RSACryptoServiceProvider();
            cipher.FromXmlString(_rsaKeyForDecryption);
            return cipher;
        }

        private RSACryptoServiceProvider CreateCipherForEncryption()
        {
            var cipher = new RSACryptoServiceProvider();
            cipher.FromXmlString(_rsaKeyForEncryption);
            return cipher;
        }

        public string GetCipherText(string plainText)
        {
            RSACryptoServiceProvider cipher = CreateCipherForEncryption();
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherText = cipher.Encrypt(data, false);
            return Convert.ToBase64String(cipherText);
        }

        public string DecryptCipherText(string cipherText)
        {
            RSACryptoServiceProvider cipher = CreateCipherForDecryption();
            byte[] original = cipher.Decrypt(Convert.FromBase64String(cipherText), false);
            return Encoding.UTF8.GetString(original);
        }

        public DigitalSignatureResult BuildSignedMessage(string symmetricKey, string message)
        {
            var result = Encrypt(symmetricKey, message);

            //Asymm encryption of symm key.
            byte[] keyBytes = Encoding.UTF8.GetBytes(symmetricKey);
            byte[] cipherBytes = CreateCipherForEncryption().Encrypt(keyBytes, false);
            byte[] signatureHash = CalculateSignatureBytes(cipherBytes);

            string cipher = Convert.ToBase64String(cipherBytes);
            string signature = Convert.ToBase64String(signatureHash);
            return new DigitalSignatureResult() { CipherText = cipher, SignatureText = signature };
        }

        private SymmetricEncryptionResult Encrypt(string symmetricKey, string plainText)
        {
            var res = new SymmetricEncryptionResult();
            RijndaelManaged rijndael = CreateSymmetricCipher(symmetricKey);
            res.InitialisationVector = Convert.ToBase64String(rijndael.IV);
            ICryptoTransform cryptoTransform = rijndael.CreateEncryptor();
            byte[] plain = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherText = cryptoTransform.TransformFinalBlock(plain, 0, plain.Length);
            res.CipherText = Convert.ToBase64String(cipherText);
            return res;
        }

        private RijndaelManaged CreateSymmetricCipher(string symmkey)
        {
            var cipher = new RijndaelManaged();
            cipher.KeySize = 128;
            cipher.BlockSize = 128;
            cipher.Padding = PaddingMode.ISO10126;
            cipher.Mode = CipherMode.CBC;
            byte[] key = HexToByteArray(symmkey);
            cipher.Key = key;
            return cipher;
        }

        private byte[] HexToByteArray(string hexString)
        {
            if (0 != (hexString.Length % 2))
            {
                throw new ApplicationException("Hex string must be multiple of 2 in length");
            }

            int byteCount = hexString.Length / 2;
            byte[] byteValues = new byte[byteCount];
            for (int i = 0; i < byteCount; i++)
            {
                byteValues[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return byteValues;
        }

        private byte[] CalculateSignatureBytes(byte[] cipherBytes)
        {
            //Create a new instance of RSACryptoServiceProvider. 
            using (var rsa = new RSACryptoServiceProvider())
            {
                //The hash to sign. 
                byte[] hash;
                using (SHA256 sha256 = SHA256.Create())
                {
                    hash = sha256.ComputeHash(cipherBytes);
                }

                //Create an RSASignatureFormatter object and pass it the  
                //RSACryptoServiceProvider to transfer the key information.
                var RSAFormatter = new RSAPKCS1SignatureFormatter(rsa);

                //Set the hash algorithm to SHA256.
                RSAFormatter.SetHashAlgorithm("SHA256");

                //Create a signature for HashValue and return it. 
                return RSAFormatter.CreateSignature(hash);
            }
        }


        public RSACryptoServiceProvider GetKeyContainer(string containerName)
        {
            var cspParams = new CspParameters();
            cspParams.KeyContainerName = containerName;
            cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
            var sender = new RSACryptoServiceProvider(cspParams);
            Console.WriteLine("Key retrieved from container : \n {0}", sender.ToXmlString(true));
            return sender;
        }
    }
}
