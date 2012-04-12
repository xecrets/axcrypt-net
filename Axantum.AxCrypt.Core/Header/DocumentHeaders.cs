﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Reader;

namespace Axantum.AxCrypt.Core.Reader
{
    public class DocumentHeaders
    {
        private IList<HeaderBlock> HeaderBlocks { get; set; }

        private AesKey _keyEncryptingKey;

        public DocumentHeaders(AesKey keyEncryptingKey)
        {
            _keyEncryptingKey = keyEncryptingKey;

            HeaderBlocks = new List<HeaderBlock>();
            HeaderBlocks.Add(new PreambleHeaderBlock());
            HeaderBlocks.Add(new VersionHeaderBlock());
            HeaderBlocks.Add(new KeyWrap1HeaderBlock(keyEncryptingKey));
            HeaderBlocks.Add(new EncryptionInfoHeaderBlock());
            HeaderBlocks.Add(new CompressionInfoHeaderBlock());
            HeaderBlocks.Add(new FileInfoHeaderBlock());
            HeaderBlocks.Add(new UnicodeFileNameInfoHeaderBlock());
            HeaderBlocks.Add(new FileNameInfoHeaderBlock());
            HeaderBlocks.Add(new DataHeaderBlock());

            SetMasterKeyForEncryptedHeaderBlocks(HeaderBlocks);
        }

        public DocumentHeaders(DocumentHeaders documentHeaders)
        {
            List<HeaderBlock> headerBlocks = new List<HeaderBlock>();
            foreach (HeaderBlock headerBlock in documentHeaders.HeaderBlocks)
            {
                headerBlocks.Add((HeaderBlock)headerBlock.Clone());
            }
            HeaderBlocks = headerBlocks;

            _keyEncryptingKey = documentHeaders._keyEncryptingKey;
        }

        public bool Load(AxCryptReader axCryptReader)
        {
            axCryptReader.Read();
            if (axCryptReader.CurrentItemType != AxCryptItemType.MagicGuid)
            {
                throw new FileFormatException("No magic Guid was found.", ErrorStatus.MagicGuidMissing);
            }
            List<HeaderBlock> headerBlocks = new List<HeaderBlock>();
            while (axCryptReader.Read())
            {
                switch (axCryptReader.CurrentItemType)
                {
                    case AxCryptItemType.HeaderBlock:
                        headerBlocks.Add(axCryptReader.CurrentHeaderBlock);
                        break;
                    case AxCryptItemType.Data:
                        headerBlocks.Add(axCryptReader.CurrentHeaderBlock);
                        HeaderBlocks = headerBlocks;
                        EnsureFileFormatVersion();
                        if (GetMasterKey() != null)
                        {
                            SetMasterKeyForEncryptedHeaderBlocks(headerBlocks);
                            return true;
                        }
                        HeaderBlocks = null;
                        return false;
                    default:
                        throw new InternalErrorException("The reader returned an AxCryptItemType it should not be possible for it to return.");
                }
            }
            throw new FileFormatException("Premature end of stream.", ErrorStatus.EndOfStream);
        }

        private void SetMasterKeyForEncryptedHeaderBlocks(IList<HeaderBlock> headerBlocks)
        {
            Subkey headersSubkey = new Subkey(GetMasterKey(), HeaderSubkey.Headers);
            AesCrypto headerCrypto = new AesCrypto(headersSubkey.Key);

            foreach (HeaderBlock headerBlock in headerBlocks)
            {
                EncryptedHeaderBlock encryptedHeaderBlock = headerBlock as EncryptedHeaderBlock;
                if (encryptedHeaderBlock != null)
                {
                    encryptedHeaderBlock.HeaderCrypto = headerCrypto;
                }
            }
        }

        public void Write(Stream cipherStream, Stream hmacStream)
        {
            VersionHeaderBlock versionHeaderBlock = FindHeaderBlock<VersionHeaderBlock>();
            versionHeaderBlock.SetCurrentVersion();

            cipherStream.Position = 0;
            AxCrypt1Guid.Write(cipherStream);
            PreambleHeaderBlock preambleHaderBlock = FindHeaderBlock<PreambleHeaderBlock>();
            preambleHaderBlock.Write(cipherStream);
            foreach (HeaderBlock headerBlock in HeaderBlocks)
            {
                if (headerBlock is DataHeaderBlock)
                {
                    continue;
                }
                if (headerBlock is PreambleHeaderBlock)
                {
                    continue;
                }
                headerBlock.Write(hmacStream != null ? hmacStream : cipherStream);
            }
            DataHeaderBlock dataHeaderBlock = FindHeaderBlock<DataHeaderBlock>();
            dataHeaderBlock.Write(hmacStream != null ? hmacStream : cipherStream);
        }

        private AesKey GetMasterKey()
        {
            KeyWrap1HeaderBlock keyHeaderBlock = FindHeaderBlock<KeyWrap1HeaderBlock>();
            VersionHeaderBlock versionHeaderBlock = FindHeaderBlock<VersionHeaderBlock>();
            byte[] unwrappedKeyData = keyHeaderBlock.UnwrapMasterKey(_keyEncryptingKey, versionHeaderBlock.FileVersionMajor);
            if (unwrappedKeyData.Length == 0)
            {
                return null;
            }
            return new AesKey(unwrappedKeyData);
        }

        public void RewrapMasterKey(AesKey keyEncryptingKey)
        {
            KeyWrap1HeaderBlock keyHeaderBlock = FindHeaderBlock<KeyWrap1HeaderBlock>();
            keyHeaderBlock.RewrapMasterKey(GetMasterKey(), keyEncryptingKey);
            _keyEncryptingKey = keyEncryptingKey;
        }

        public Subkey HmacSubkey
        {
            get
            {
                AesKey masterKey = GetMasterKey();
                if (masterKey == null)
                {
                    return null;
                }
                return new Subkey(masterKey, HeaderSubkey.Hmac);
            }
        }

        public Subkey DataSubkey
        {
            get
            {
                AesKey masterKey = GetMasterKey();
                if (masterKey == null)
                {
                    return null;
                }
                return new Subkey(masterKey, HeaderSubkey.Data);
            }
        }

        public DataHmac Hmac
        {
            get
            {
                PreambleHeaderBlock headerBlock = FindHeaderBlock<PreambleHeaderBlock>();

                return headerBlock.Hmac;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                PreambleHeaderBlock headerBlock = FindHeaderBlock<PreambleHeaderBlock>();
                headerBlock.Hmac = value;
            }
        }

        public long NormalSize
        {
            get
            {
                CompressionInfoHeaderBlock compressionInfo = FindHeaderBlock<CompressionInfoHeaderBlock>();
                if (compressionInfo == null)
                {
                    return -1;
                }
                return compressionInfo.NormalSize;
            }

            set
            {
                CompressionInfoHeaderBlock compressionInfo = FindHeaderBlock<CompressionInfoHeaderBlock>();
                compressionInfo.NormalSize = value;
            }
        }

        public string AnsiFileName
        {
            get
            {
                FileNameInfoHeaderBlock headerBlock = FindHeaderBlock<FileNameInfoHeaderBlock>();
                return headerBlock.FileName;
            }

            set
            {
                FileNameInfoHeaderBlock headerBlock = FindHeaderBlock<FileNameInfoHeaderBlock>();
                headerBlock.FileName = value;
            }
        }

        public string UnicodeFileName
        {
            get
            {
                UnicodeFileNameInfoHeaderBlock headerBlock = FindHeaderBlock<UnicodeFileNameInfoHeaderBlock>();
                if (headerBlock == null)
                {
                    // Unicode file name was added in 1.6.3.3 - if we can't find it signal it's absence with an empty string.
                    return String.Empty;
                }

                return headerBlock.FileName;
            }

            set
            {
                UnicodeFileNameInfoHeaderBlock headerBlock = FindHeaderBlock<UnicodeFileNameInfoHeaderBlock>();
                headerBlock.FileName = value;

                AnsiFileName = value;
            }
        }

        public string FileName
        {
            get
            {
                string unicodeFileName = UnicodeFileName;
                if (!String.IsNullOrEmpty(unicodeFileName))
                {
                    return unicodeFileName;
                }
                return AnsiFileName;
            }
        }

        public bool IsCompressed
        {
            get
            {
                CompressionHeaderBlock headerBlock = FindHeaderBlock<CompressionHeaderBlock>();
                if (headerBlock == null)
                {
                    // Conditional compression was added in 1.2.2, before then it was always compressed.
                    return true;
                }
                return headerBlock.IsCompressed;
            }
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                FileInfoHeaderBlock headerBlock = FindHeaderBlock<FileInfoHeaderBlock>();
                return headerBlock.CreationTimeUtc;
            }
            set
            {
                FileInfoHeaderBlock headerBlock = FindHeaderBlock<FileInfoHeaderBlock>();
                headerBlock.CreationTimeUtc = value;
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                FileInfoHeaderBlock headerBlock = FindHeaderBlock<FileInfoHeaderBlock>();
                return headerBlock.LastAccessTimeUtc;
            }
            set
            {
                FileInfoHeaderBlock headerBlock = FindHeaderBlock<FileInfoHeaderBlock>();
                headerBlock.LastAccessTimeUtc = value;
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                FileInfoHeaderBlock headerBlock = FindHeaderBlock<FileInfoHeaderBlock>();
                return headerBlock.LastWriteTimeUtc;
            }
            set
            {
                FileInfoHeaderBlock headerBlock = FindHeaderBlock<FileInfoHeaderBlock>();
                headerBlock.LastWriteTimeUtc = value;
            }
        }

        /// <summary>
        /// The Initial Vector used for CBC encryption of the data
        /// </summary>
        /// <returns>The Initial Vector</returns>
        public AesIV IV
        {
            get
            {
                EncryptionInfoHeaderBlock headerBlock = FindHeaderBlock<EncryptionInfoHeaderBlock>();
                return headerBlock.IV;
            }
        }

        /// <summary>
        /// The length in bytes of the plain text. This may still require decompression (inflate).
        /// </summary>
        public long PlaintextLength
        {
            get
            {
                EncryptionInfoHeaderBlock headerBlock = FindHeaderBlock<EncryptionInfoHeaderBlock>();
                return headerBlock.PlaintextLength;
            }

            set
            {
                EncryptionInfoHeaderBlock headerBlock = FindHeaderBlock<EncryptionInfoHeaderBlock>();
                headerBlock.PlaintextLength = value;
            }
        }

        public long DataLength
        {
            get
            {
                DataHeaderBlock headerBlock = FindHeaderBlock<DataHeaderBlock>();
                return headerBlock.DataLength;
            }
            set
            {
                DataHeaderBlock headerBlock = FindHeaderBlock<DataHeaderBlock>();
                headerBlock.DataLength = value;
            }
        }

        private void EnsureFileFormatVersion()
        {
            VersionHeaderBlock versionHeaderBlock = FindHeaderBlock<VersionHeaderBlock>();
            if (versionHeaderBlock.FileVersionMajor > 3)
            {
                throw new FileFormatException("Too new file format.", ErrorStatus.TooNewFileFormatVersion);
            }
        }

        private T FindHeaderBlock<T>() where T : HeaderBlock
        {
            foreach (HeaderBlock headerBlock in HeaderBlocks)
            {
                T typedHeaderHeaderBlock = headerBlock as T;
                if (typedHeaderHeaderBlock != null)
                {
                    return typedHeaderHeaderBlock;
                }
            }
            return null;
        }
    }
}