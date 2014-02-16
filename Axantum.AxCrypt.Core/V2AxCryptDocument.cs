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
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using Org.BouncyCastle.Utilities.Zlib;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core
{
    /// <summary>
    /// Enables a single point of interaction for an AxCrypt File Format Version 4 encrypted stream with all but the data available
    /// in-memory. File Format Version 4 is only supported by AxCrypt 2.x or higher. It builds on, and is similar to, File Format
    /// Version 3. See the specification titled "AxCrypt Version 2 Algorithms and File Format" for details.
    /// </summary>
    public class V2AxCryptDocument
    {
        private long _plainTextLength;

        private long _compressedPlainTextLength;

        public V2AxCryptDocument(ICrypto keyEncryptingCrypto)
        {
            DocumentHeaders = new V2DocumentHeaders(keyEncryptingCrypto);
        }

        public V2AxCryptDocument(ICrypto keyEncryptingCrypto, long iterations)
        {
            DocumentHeaders = new V2DocumentHeaders(keyEncryptingCrypto, iterations);
        }

        public V2DocumentHeaders DocumentHeaders { get; private set; }

        private AxCryptReader _reader;

        public bool PassphraseIsValid { get; set; }

        /// <summary>
        /// Loads an AxCrypt file from the specified reader. After this, the reader is positioned to
        /// read encrypted data.
        /// </summary>
        /// <param name="stream">The stream to read from. Will be disposed when this instance is disposed.</param>
        /// <returns>True if the key was valid, false if it was wrong.</returns>
        public bool Load(Stream stream)
        {
            _reader = new V2AxCryptReader(stream);
            PassphraseIsValid = DocumentHeaders.Load(_reader);
            if (!PassphraseIsValid)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Encrypt a stream with a given set of headers and write to an output stream. The caller is responsible for consistency and completeness
        /// of the headers. Headers that are not known until encryption and compression are added here.
        /// </summary>
        /// <param name="outputDocumentHeaders"></param>
        /// <param name="inputStream"></param>
        /// <param name="outputStream"></param>
        public void EncryptTo(Stream inputStream, Stream outputStream, AxCryptOptions options, IProgressContext progress)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }
            if (outputStream == null)
            {
                throw new ArgumentNullException("outputStream");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }
            if (options.HasMask(AxCryptOptions.EncryptWithCompression) && options.HasMask(AxCryptOptions.EncryptWithoutCompression))
            {
                throw new ArgumentException("Invalid options, cannot specify both with and without compression.");
            }
            if (!options.HasMask(AxCryptOptions.EncryptWithCompression) && !options.HasMask(AxCryptOptions.EncryptWithoutCompression))
            {
                throw new ArgumentException("Invalid options, must specify either with or without compression.");
            }
            DocumentHeaders.IsCompressed = options.HasMask(AxCryptOptions.EncryptWithCompression);
            using (V2HmacStream outputHmacStream = new V2HmacStream(DocumentHeaders.GetHmacKey(), outputStream))
            {
                DocumentHeaders.WriteStartWithHmac(outputHmacStream);
                using (ICryptoTransform encryptor = DocumentHeaders.GetDataCrypto().CreateEncryptingTransform())
                {
                    using (Stream axCryptDataStream = new V2AxCryptDataStream(outputHmacStream))
                    {
                        using (CryptoStream encryptingStream = new CryptoStream(new NonClosingStream(axCryptDataStream), encryptor, CryptoStreamMode.Write))
                        {
                            if (DocumentHeaders.IsCompressed)
                            {
                                EncryptWithCompressionInternal(DocumentHeaders, inputStream, encryptingStream, progress);
                            }
                            else
                            {
                                _compressedPlainTextLength = _plainTextLength = inputStream.CopyToWithCount(encryptingStream, inputStream, progress);
                            }
                        }
                    }
                }
                DocumentHeaders.WriteEndWithHmac(outputHmacStream, _plainTextLength, _compressedPlainTextLength);
            }
        }

        private void EncryptWithCompressionInternal(V2DocumentHeaders outputDocumentHeaders, Stream inputStream, CryptoStream encryptingStream, IProgressContext progress)
        {
            using (ZOutputStream deflatingStream = new ZOutputStream(encryptingStream, -1))
            {
                deflatingStream.FlushMode = JZlib.Z_SYNC_FLUSH;
                inputStream.CopyToWithCount(deflatingStream, inputStream, progress);
                deflatingStream.FlushMode = JZlib.Z_FINISH;
                deflatingStream.Finish();

                _plainTextLength = deflatingStream.TotalIn;
                _compressedPlainTextLength = deflatingStream.TotalOut;
            }
        }

        /// <summary>
        /// Decrypts the encrypted data to the given stream
        /// </summary>
        /// <param name="outputPlaintextStream">The resulting plain text stream.</param>
        public void DecryptTo(Stream outputPlaintextStream, IProgressContext progress)
        {
            if (!PassphraseIsValid)
            {
                throw new InternalErrorException("Passsphrase is not valid!");
            }

            using (ICryptoTransform decryptor = DocumentHeaders.GetDataCrypto().CreateDecryptingTransform())
            {
                using (Stream encryptedDataStream = CreateEncryptedDataStream())
                {
                    DecryptEncryptedDataStream(outputPlaintextStream, decryptor, encryptedDataStream, progress);
                }
            }

            DocumentHeaders.Trailers(_reader);
            if (DocumentHeaders.HmacStream.Hmac != DocumentHeaders.Hmac)
            {
                throw new Axantum.AxCrypt.Core.Runtime.InvalidDataException("HMAC validation error.", ErrorStatus.HmacValidationError);
            }
        }

        private Stream CreateEncryptedDataStream()
        {
            if (_reader.CurrentItemType != AxCryptItemType.Data)
            {
                throw new InvalidOperationException("GetEncryptedDataStream() was called when the reader is not positioned at the data.");
            }

            _reader.SetStartOfData();
            V2AxCryptDataStream encryptedDataStream = new V2AxCryptDataStream(_reader, DocumentHeaders.HmacStream);
            return encryptedDataStream;
        }

        private void DecryptEncryptedDataStream(Stream outputPlaintextStream, ICryptoTransform decryptor, Stream encryptedDataStream, IProgressContext progress)
        {
            Exception savedExceptionIfCloseCausesCryptographicException = null;
            try
            {
                if (DocumentHeaders.IsCompressed)
                {
                    using (CryptoStream deflatedPlaintextStream = new CryptoStream(encryptedDataStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (ZInputStream inflatedPlaintextStream = new ZInputStream(deflatedPlaintextStream))
                        {
                            try
                            {
                                inflatedPlaintextStream.CopyToWithCount(outputPlaintextStream, encryptedDataStream, progress);
                            }
                            catch (Exception ex)
                            {
                                savedExceptionIfCloseCausesCryptographicException = ex;
                                throw;
                            }
                        }
                    }
                }
                else
                {
                    using (Stream plainStream = new CryptoStream(encryptedDataStream, decryptor, CryptoStreamMode.Read))
                    {
                        try
                        {
                            plainStream.CopyToWithCount(outputPlaintextStream, encryptedDataStream, progress);
                        }
                        catch (Exception ex)
                        {
                            savedExceptionIfCloseCausesCryptographicException = ex;
                            throw;
                        }
                    }
                }
            }
            catch (CryptographicException)
            {
                throw savedExceptionIfCloseCausesCryptographicException;
            }
        }
    }
}