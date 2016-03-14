using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Utilities.IO;
using System;
using System.IO;
using System.Linq;

namespace PgpCrypto
{
    public class PgpDecrypt
    {
        private readonly PgpEncryptionKeys _encryptionKeys;

        public PgpDecrypt(PgpEncryptionKeys encryptionKeys)
        {
            if (encryptionKeys == null)

                throw new ArgumentNullException("encryptionKeys", "encryptionKeys is null.");

            _encryptionKeys = encryptionKeys;
        }

        public void DecryptAndVerify(Stream input, Stream outputStream)
        {
            var fileContentsList = ListEncryptedFileContents(input);
            var encryptedData = FindPublicKeyEncryptedData(fileContentsList);
            var compressedData = DecryptData(encryptedData);
            var clearData = DecompressData(compressedData);
            PipeToOutputStream(clearData, outputStream);
        }

        private PgpEncryptedDataList ListEncryptedFileContents(Stream input)
        {
            input = PgpUtilities.GetDecoderStream(input);
            var pgpObjFactory = new PgpObjectFactory(input);
            PgpEncryptedDataList dataList;

            if (TryFindObject(pgpObjFactory, out dataList)) return dataList;

            throw new Exception("Encrypted contents not found");
        }

        private PgpPublicKeyEncryptedData FindPublicKeyEncryptedData(PgpEncryptedDataList dataList)
        {
            var publicData = dataList.GetEncryptedDataObjects()
                                                .Cast<PgpPublicKeyEncryptedData>()
                                                .FirstOrDefault(pked => _encryptionKeys.PrivateKey != null);

            if(publicData == null) throw new Exception("Public key encrypted data not found.");

            return publicData;
        }

        private PgpCompressedData DecryptData(PgpPublicKeyEncryptedData pbe)
        {
            using (var clear = pbe.GetDataStream(_encryptionKeys.PrivateKey))
            {
                var pgpFactory = new PgpObjectFactory(clear);
                PgpCompressedData message;

                if (TryFindObject(pgpFactory, out message)) return message;

                throw new Exception("PGP data should be compressed");
            }
        }

        private PgpLiteralData DecompressData(PgpCompressedData compressedData)
        {
            PgpLiteralData message = null;

            using (var compressedDataStream = compressedData.GetDataStream())
            {
                var pgpCompressedFactory = new PgpObjectFactory(compressedDataStream);
                PgpLiteralData data;

                if (TryFindObject(pgpCompressedFactory, out data)) return data;

                throw new Exception("Compressed data could not be converted to literal data");
            }
        }

        private void PipeToOutputStream(PgpLiteralData clearData, Stream outputStream)
        {
            using(var unc = clearData.GetInputStream())
            {
                Streams.PipeAll(unc, outputStream);
            }
        }

        private bool TryFindObject<T>(PgpObjectFactory factory, out T obj) where T : class
        {
            obj = null;

            while (obj == null)
            {
                try
                {
                    var pgpObject = factory.NextPgpObject();
                    obj = pgpObject as T;
                }
                catch (Exception e)
                {
                    return false;
                }
            }

            return true;
        }
    }
}