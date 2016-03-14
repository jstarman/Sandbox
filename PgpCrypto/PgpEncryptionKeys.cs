using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;

namespace PgpCrypto
{
    public class PgpEncryptionKeys
    {
        public PgpPublicKey PublicKey { get; private set; }

        public PgpPrivateKey PrivateKey { get; private set; }

        public PgpSecretKey SecretKey { get; private set; }

        /// <summary>
        /// Initializes a new instance of the EncryptionKeys class.
        /// Two keys are required to encrypt and sign data. Your private key and the recipients public key.
        /// The data is encrypted with the recipients public key and signed with your private key.
        /// </summary>
        /// <param name="publicKeyPath">The key used to encrypt the data</param>
        /// <param name="privateKeyPath">The key used to sign the data.</param>
        /// <param name="passPhrase">The (your) password required to access the private key</param>
        /// <exception cref="ArgumentException">Public key not found. Private key not found. Missing password</exception>
        public PgpEncryptionKeys(string publicKeyPath, string privateKeyPath, string passPhrase)
        {
            if (!File.Exists(publicKeyPath))

                throw new ArgumentException("Public key file not found", "publicKeyPath");

            if (!File.Exists(privateKeyPath))

                throw new ArgumentException("Private key file not found", "privateKeyPath");

            if (String.IsNullOrEmpty(passPhrase))

                throw new ArgumentException("passPhrase is null or empty.", "passPhrase");

            PublicKey = ReadPublicKey(publicKeyPath);

            SecretKey = ReadSecretKey(privateKeyPath);

            PrivateKey = ReadPrivateKey(passPhrase);

        }

        public PgpEncryptionKeys(string publicKeyPath, string containerName)
        {
            var cspParams = new CspParameters
            {
                KeyContainerName = containerName,
                Flags = CspProviderFlags.UseMachineKeyStore
            };
            var keys = new RSACryptoServiceProvider(cspParams);

            var rsaParams = keys.ExportParameters(true);
            var kp = DotNetUtilities.GetRsaKeyPair(rsaParams);

        }

        #region Secret Key

        private PgpSecretKey ReadSecretKey(string privateKeyPath)
        {
            using (var keyIn = File.OpenRead(privateKeyPath))
            using (var inputStream = PgpUtilities.GetDecoderStream(keyIn))
            {
                var secretKeyRingBundle = new PgpSecretKeyRingBundle(inputStream);

                var foundKey = GetFirstSecretKey(secretKeyRingBundle);

                if (foundKey != null)

                    return foundKey;
            }

            throw new ArgumentException("Can't find signing key in key ring.");
        }

        /// <summary>
        /// Return the first key we can use to encrypt.
        /// Note: A file can contain multiple keys (stored in "key rings")
        /// </summary>
        private PgpSecretKey GetFirstSecretKey(PgpSecretKeyRingBundle secretKeyRingBundle)
        {
            foreach (PgpSecretKeyRing kRing in secretKeyRingBundle.GetKeyRings())
            {
                var key = kRing.GetSecretKeys()
                    .Cast<PgpSecretKey>().FirstOrDefault(k => k.IsSigningKey);

                if (key != null)

                    return key;
            }

            return null;
        }

        #endregion

        #region Public Key

        private PgpPublicKey ReadPublicKey(string publicKeyPath)
        {
            using (var keyIn = File.OpenRead(publicKeyPath))

            using (var inputStream = PgpUtilities.GetDecoderStream(keyIn))
            {
                var publicKeyRingBundle = new PgpPublicKeyRingBundle(inputStream);

                var foundKey = GetFirstPublicKey(publicKeyRingBundle);

                if (foundKey != null)

                    return foundKey;
            }

            throw new ArgumentException("No encryption key found in public key ring.");
        }

        private PgpPublicKey GetFirstPublicKey(PgpPublicKeyRingBundle publicKeyRingBundle)
        {
            foreach (PgpPublicKeyRing kRing in publicKeyRingBundle.GetKeyRings())
            {
                var key = kRing.GetPublicKeys()
                    .Cast<PgpPublicKey>()
                    .FirstOrDefault(k => k.IsEncryptionKey);

                if (key != null)

                    return key;
            }

            return null;
        }

        #endregion

        #region Private Key

        private PgpPrivateKey ReadPrivateKey(string passPhrase)
        {
            var privateKey = SecretKey.ExtractPrivateKey(passPhrase.ToCharArray());

            if (privateKey != null)

                return privateKey;

            throw new ArgumentException("No private key found in secret key.");
        }

        #endregion
    }
}