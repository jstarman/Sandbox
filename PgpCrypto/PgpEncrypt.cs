using System;
using System.IO;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;

namespace PgpCrypto
{
    /// <summary>
    /// Wrapper around Bouncy Castle OpenPGP library.
    /// Bouncy documentation can be found here: http://www.bouncycastle.org/docs/pgdocs1.6/index.html
    /// </summary>
    public class PgpEncrypt
    {
        private readonly PgpEncryptionKeys _encryptionKeys;

        private const int BufferSize = 0x10000; // should always be power of 2 

        /// <summary>
        /// Instantiate a new PgpEncrypt class with initialized PgpEncryptionKeys.
        /// </summary>
        /// <param name="encryptionKeys"></param>
        /// <exception cref="ArgumentNullException">encryptionKeys is null</exception>
        public PgpEncrypt(PgpEncryptionKeys encryptionKeys)
        {
            if (encryptionKeys == null)

                throw new ArgumentNullException("encryptionKeys", "encryptionKeys is null.");

            _encryptionKeys = encryptionKeys;
        }

        /// <summary>
        /// Encrypt and sign the file pointed to by unencryptedFileInfo and
        /// write the encrypted content to outputStream.
        /// </summary>
        /// <param name="outputStream">The stream that will contain the
        /// encrypted data when this method returns.</param>
        /// <param name="unencryptedFileInfo">FileInfo of the file to encrypt</param>
        public void EncryptAndSign(Stream outputStream, FileInfo unencryptedFileInfo)
        {
            if (outputStream == null)

                throw new ArgumentNullException("outputStream", "outputStream is null.");

            if (unencryptedFileInfo == null)

                throw new ArgumentNullException("unencryptedFileInfo", "unencryptedFileInfo is null.");

            if (!File.Exists(unencryptedFileInfo.FullName))

                throw new ArgumentException("File to encrypt not found.");

            using (var encryptedOut = ChainEncryptedOut(outputStream))
            using (var compressedOut = ChainCompressedOut(encryptedOut))
            {
                var signatureGenerator = InitSignatureGenerator(compressedOut);

                using (var literalOut = ChainLiteralOut(compressedOut, unencryptedFileInfo))

                using (var inputFile = unencryptedFileInfo.OpenRead())
                {
                    WriteOutputAndSign(compressedOut, literalOut, inputFile, signatureGenerator);
                }
            }
        }

        private static void WriteOutputAndSign(Stream compressedOut,
            Stream literalOut,
            FileStream inputFile,
            PgpSignatureGenerator signatureGenerator)
        {
            var length = 0;

            var buf = new byte[BufferSize];

            while ((length = inputFile.Read(buf, 0, buf.Length)) > 0)
            {
                literalOut.Write(buf, 0, length);
                signatureGenerator.Update(buf, 0, length);
            }

            signatureGenerator.Generate().Encode(compressedOut);
        }

        private Stream ChainEncryptedOut(Stream outputStream)
        {
            var encryptedDataGenerator = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Aes256,
                new SecureRandom());

            encryptedDataGenerator.AddMethod(_encryptionKeys.PublicKey);

            return encryptedDataGenerator.Open(outputStream, new byte[BufferSize]);
        }

        private static Stream ChainCompressedOut(Stream encryptedOut)
        {
            var compressedDataGenerator =
                new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);

            return compressedDataGenerator.Open(encryptedOut);
        }

        private static Stream ChainLiteralOut(Stream compressedOut, FileInfo file)
        {
            var pgpLiteralDataGenerator = new PgpLiteralDataGenerator();

            return pgpLiteralDataGenerator.Open(compressedOut, PgpLiteralData.Binary, file);
        }

        private PgpSignatureGenerator InitSignatureGenerator(Stream compressedOut)
        {
            const bool IsCritical = false;

            const bool IsNested = false;

            var tag = _encryptionKeys.PublicKey.Algorithm;

            var pgpSignatureGenerator =
                new PgpSignatureGenerator(tag, HashAlgorithmTag.Sha256);

            pgpSignatureGenerator.InitSign(PgpSignature.BinaryDocument, _encryptionKeys.PrivateKey);

            foreach (string userId in _encryptionKeys.PublicKey.GetUserIds()) 
            {
                var subPacketGenerator =
                    new PgpSignatureSubpacketGenerator();

                subPacketGenerator.SetSignerUserId(IsCritical, userId);

                pgpSignatureGenerator.SetHashedSubpackets(subPacketGenerator.Generate());

                // Just the first one!

                break;
            }

            pgpSignatureGenerator.GenerateOnePassVersion(IsNested).Encode(compressedOut);

            return pgpSignatureGenerator;
        }
    }
}