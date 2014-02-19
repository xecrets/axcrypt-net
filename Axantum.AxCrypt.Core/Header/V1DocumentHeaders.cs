﻿#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using System;
using System.Collections.Generic;
using System.IO;

namespace Axantum.AxCrypt.Core.Header
{
    public class V1DocumentHeaders
    {
        private static readonly byte[] _version = new byte[] { 3, 2, 2, 0, 0 };

        private Headers _headers = new Headers();

        private ICrypto _keyEncryptingCrypto;

        public V1DocumentHeaders(ICrypto keyEncryptingCrypto, long iterations)
            : this(keyEncryptingCrypto)
        {
            _headers.HeaderBlocks.Add(new V1KeyWrap1HeaderBlock(keyEncryptingCrypto, iterations));

            SetMasterKeyForEncryptedHeaderBlocks(_headers.HeaderBlocks);

            V1EncryptionInfoHeaderBlock encryptionInfoHeaderBlock = _headers.FindHeaderBlock<V1EncryptionInfoHeaderBlock>();
            encryptionInfoHeaderBlock.IV = new SymmetricIV(128);
            encryptionInfoHeaderBlock.PlaintextLength = 0;
        }

        public V1DocumentHeaders(ICrypto keyEncryptingCrypto)
        {
            _keyEncryptingCrypto = keyEncryptingCrypto;

            _headers.HeaderBlocks.Add(new PreambleHeaderBlock());
            _headers.HeaderBlocks.Add(new VersionHeaderBlock(_version));
            _headers.HeaderBlocks.Add(new V1EncryptionInfoHeaderBlock());
            _headers.HeaderBlocks.Add(new V1CompressionHeaderBlock());
            _headers.HeaderBlocks.Add(new FileInfoHeaderBlock());
            _headers.HeaderBlocks.Add(new V1UnicodeFileNameInfoHeaderBlock());
            _headers.HeaderBlocks.Add(new V1FileNameInfoHeaderBlock());
            _headers.HeaderBlocks.Add(new DataHeaderBlock());
        }

        public V1DocumentHeaders(V1DocumentHeaders documentHeaders)
        {
            _keyEncryptingCrypto = documentHeaders._keyEncryptingCrypto;
            foreach (HeaderBlock headerBlock in documentHeaders._headers.HeaderBlocks)
            {
                _headers.HeaderBlocks.Add((HeaderBlock)headerBlock.Clone());
            }
        }

        public bool Load(AxCryptReader reader)
        {
            Headers headers = new Headers();
            headers.Load(reader);

            return Load(headers);
        }

        public bool Load(Headers headers)
        {
            _headers = headers;
            _headers.EnsureFileFormatVersion(0, 3);

            if (GetMasterKey() != null)
            {
                SetMasterKeyForEncryptedHeaderBlocks(_headers.HeaderBlocks);
                return true;
            }
            return false;
        }

        public Headers Headers
        {
            get { return _headers; }
        }

        public VersionHeaderBlock VersionHeaderBlock
        {
            get
            {
                return _headers.VersionHeaderBlock;
            }
        }

        private void SetMasterKeyForEncryptedHeaderBlocks(IList<HeaderBlock> headerBlocks)
        {
            ICrypto headerCrypto = new V1AesCrypto(HeadersSubkey.Key);

            foreach (HeaderBlock headerBlock in headerBlocks)
            {
                EncryptedHeaderBlock encryptedHeaderBlock = headerBlock as EncryptedHeaderBlock;
                if (encryptedHeaderBlock != null)
                {
                    encryptedHeaderBlock.HeaderCrypto = headerCrypto;
                }
            }
        }

        public void WriteWithoutHmac(Stream cipherStream)
        {
            if (cipherStream == null)
            {
                throw new ArgumentNullException("cipherStream");
            }

            WriteInternal(cipherStream, cipherStream);
        }

        public void WriteWithHmac(V1HmacStream hmacStream)
        {
            if (hmacStream == null)
            {
                throw new ArgumentNullException("hmacStream");
            }

            WriteInternal(hmacStream.ChainedStream, hmacStream);
        }

        private void WriteInternal(Stream cipherStream, Stream hmacStream)
        {
            cipherStream.Position = 0;
            AxCrypt1Guid.Write(cipherStream);
            PreambleHeaderBlock preambleHaderBlock = _headers.FindHeaderBlock<PreambleHeaderBlock>();
            preambleHaderBlock.Write(cipherStream);
            foreach (HeaderBlock headerBlock in _headers.HeaderBlocks)
            {
                if (headerBlock is DataHeaderBlock)
                {
                    continue;
                }
                if (headerBlock is PreambleHeaderBlock)
                {
                    continue;
                }
                headerBlock.Write(hmacStream);
            }
            DataHeaderBlock dataHeaderBlock = _headers.FindHeaderBlock<DataHeaderBlock>();
            dataHeaderBlock.Write(hmacStream);
        }

        public ICrypto KeyEncryptingCrypto
        {
            get
            {
                return _keyEncryptingCrypto;
            }
        }

        private SymmetricKey GetMasterKey()
        {
            V1KeyWrap1HeaderBlock keyHeaderBlock = _headers.FindHeaderBlock<V1KeyWrap1HeaderBlock>();
            VersionHeaderBlock versionHeaderBlock = _headers.FindHeaderBlock<VersionHeaderBlock>();
            byte[] unwrappedKeyData = keyHeaderBlock.UnwrapMasterKey(_keyEncryptingCrypto, versionHeaderBlock.FileVersionMajor);
            if (unwrappedKeyData.Length == 0)
            {
                return null;
            }
            return new SymmetricKey(unwrappedKeyData);
        }

        public void RewrapMasterKey(ICrypto keyEncryptingCrypto)
        {
            V1KeyWrap1HeaderBlock keyHeaderBlock = _headers.FindHeaderBlock<V1KeyWrap1HeaderBlock>();
            keyHeaderBlock.RewrapMasterKey(GetMasterKey(), keyEncryptingCrypto.Key);
            _keyEncryptingCrypto = keyEncryptingCrypto;
        }

        public Subkey HmacSubkey
        {
            get
            {
                SymmetricKey masterKey = GetMasterKey();
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
                SymmetricKey masterKey = GetMasterKey();
                if (masterKey == null)
                {
                    return null;
                }
                return new Subkey(masterKey, HeaderSubkey.Data);
            }
        }

        public Subkey HeadersSubkey
        {
            get
            {
                SymmetricKey masterKey = GetMasterKey();
                if (masterKey == null)
                {
                    return null;
                }
                return new Subkey(masterKey, HeaderSubkey.Headers);
            }
        }

        public long UncompressedLength
        {
            get
            {
                V1CompressionInfoHeaderBlock compressionInfo = _headers.FindHeaderBlock<V1CompressionInfoHeaderBlock>();
                if (compressionInfo == null)
                {
                    return -1;
                }
                return compressionInfo.UncompressedLength;
            }

            set
            {
                V1CompressionInfoHeaderBlock compressionInfo = _headers.FindHeaderBlock<V1CompressionInfoHeaderBlock>();
                if (compressionInfo == null)
                {
                    compressionInfo = new V1CompressionInfoHeaderBlock();
                    compressionInfo.HeaderCrypto = new V1AesCrypto(HeadersSubkey.Key);
                    _headers.HeaderBlocks.Add(compressionInfo);
                }
                compressionInfo.UncompressedLength = value;
            }
        }

        private string AnsiFileName
        {
            get
            {
                V1FileNameInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<V1FileNameInfoHeaderBlock>();
                return headerBlock.FileName;
            }

            set
            {
                V1FileNameInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<V1FileNameInfoHeaderBlock>();
                headerBlock.FileName = value;
            }
        }

        private string UnicodeFileName
        {
            get
            {
                V1UnicodeFileNameInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<V1UnicodeFileNameInfoHeaderBlock>();
                if (headerBlock == null)
                {
                    // Unicode file name was added in 1.6.3.3 - if we can't find it signal it's absence with null.
                    return null;
                }

                return headerBlock.FileName;
            }

            set
            {
                V1UnicodeFileNameInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<V1UnicodeFileNameInfoHeaderBlock>();
                headerBlock.FileName = value;
            }
        }

        public string FileName
        {
            get
            {
                return UnicodeFileName ?? AnsiFileName;
            }

            set
            {
                UnicodeFileName = value;
                AnsiFileName = value;
            }
        }

        public bool IsCompressed
        {
            get
            {
                V1CompressionHeaderBlock headerBlock = _headers.FindHeaderBlock<V1CompressionHeaderBlock>();
                if (headerBlock == null)
                {
                    // Conditional compression was added in 1.2.2, before then it was always compressed.
                    return true;
                }
                return headerBlock.IsCompressed;
            }
            set
            {
                V1CompressionHeaderBlock headerBlock = _headers.FindHeaderBlock<V1CompressionHeaderBlock>();
                headerBlock.IsCompressed = value;
                if (value)
                {
                    // When compressed, ensure we reserve room in headers for the CompressionInfo block
                    UncompressedLength = 0;
                }
            }
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                FileInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoHeaderBlock>();
                return headerBlock.CreationTimeUtc;
            }
            set
            {
                FileInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoHeaderBlock>();
                headerBlock.CreationTimeUtc = value;
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                FileInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoHeaderBlock>();
                return headerBlock.LastAccessTimeUtc;
            }
            set
            {
                FileInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoHeaderBlock>();
                headerBlock.LastAccessTimeUtc = value;
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                FileInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoHeaderBlock>();
                return headerBlock.LastWriteTimeUtc;
            }
            set
            {
                FileInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoHeaderBlock>();
                headerBlock.LastWriteTimeUtc = value;
            }
        }

        /// <summary>
        /// The Initial Vector used for CBC encryption of the data
        /// </summary>
        /// <returns>The Initial Vector</returns>
        public SymmetricIV IV
        {
            get
            {
                V1EncryptionInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<V1EncryptionInfoHeaderBlock>();
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
                V1EncryptionInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<V1EncryptionInfoHeaderBlock>();
                return headerBlock.PlaintextLength;
            }

            set
            {
                V1EncryptionInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<V1EncryptionInfoHeaderBlock>();
                headerBlock.PlaintextLength = value;
            }
        }

        public long CipherTextLength
        {
            get
            {
                DataHeaderBlock headerBlock = _headers.FindHeaderBlock<DataHeaderBlock>();
                return headerBlock.CipherTextLength;
            }
            set
            {
                DataHeaderBlock headerBlock = _headers.FindHeaderBlock<DataHeaderBlock>();
                headerBlock.CipherTextLength = value;
            }
        }
    }
}