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
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core
{
    public class AxCryptFactory
    {
        public IEnumerable<string> CryptographicModes
        {
            get { return new string[] { V1AesCrypto.InternalName, V2AesCrypto.InternalName }; }
        }

        public ICrypto CreateCrypto(IPassphrase key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (key.CryptoName == V1AesCrypto.InternalName)
            {
                return new V1AesCrypto(key);
            }
            if (key.CryptoName == V2AesCrypto.InternalName)
            {
                return new V2AesCrypto(key);
            }
            if (key.CryptoName.Length == 0 && key.DerivedKey.Length == 16)
            {
                return new V1AesCrypto(key);
            }
            if (key.CryptoName.Length == 0 && key.DerivedKey.Length == 32)
            {
                return new V2AesCrypto(key);
            }
            throw new InternalErrorException("Invalid CryptoName in parameter 'key'.");
        }

        public IPassphrase CreatePassphrase(string passphrase, string cryptoName)
        {
            switch (cryptoName)
            {
                case V2AesCrypto.InternalName:
                    return new V2Passphrase(passphrase, 256);

                case V1AesCrypto.InternalName:
                    return new V1Passphrase(passphrase);
            }
            throw new InternalErrorException("Invalid CryptoName in parameter 'cryptoName'.");
        }

        public IPassphrase CreatePassphrase(string passphrase)
        {
            return new V2Passphrase(passphrase, 256);
        }

        public IAxCryptDocument CreateDocument(IPassphrase key)
        {
            IAxCryptDocument document;
            switch (key.CryptoName)
            {
                case V1AesCrypto.InternalName:
                    document = new V1AxCryptDocument(new V1AesCrypto(key), Instance.UserSettings.V1KeyWrapIterations);
                    break;

                case V2AesCrypto.InternalName:
                    document = new V2AxCryptDocument(new V2AesCrypto(key), Instance.UserSettings.V2KeyWrapIterations, Instance.UserSettings.V2KeyDerivationIterations);
                    break;

                default:
                    throw new InternalErrorException("Invalid CryptoName in parameter 'cryptoName'.");
            }
            return document;
        }

        /// <summary>
        /// Instantiate an instance of IAxCryptDocument appropriate for the file provided, i.e. V1 or V2.
        /// </summary>
        /// <param name="passphrase">The passphrase.</param>
        /// <param name="fileInfo">The file to use.</param>
        /// <returns></returns>
        public IAxCryptDocument CreateDocument(string passphrase, Stream inputStream)
        {
            Headers headers = new Headers();
            AxCryptReader reader = headers.Load(inputStream);

            IPassphrase key = reader.Crypto(headers, passphrase).Key;
            return CreateDocument(key, headers, reader);
        }

        /// <summary>
        /// Instantiate an instance of IAxCryptDocument appropriate for the file provided, i.e. V1 or V2.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public IAxCryptDocument CreateDocument(IPassphrase key, Stream inputStream)
        {
            Headers headers = new Headers();
            AxCryptReader reader = headers.Load(inputStream);

            return CreateDocument(key, headers, reader);
        }

        private static IAxCryptDocument CreateDocument(IPassphrase key, Headers headers, AxCryptReader reader)
        {
            VersionHeaderBlock versionHeader = headers.FindHeaderBlock<VersionHeaderBlock>();
            IAxCryptDocument document;
            switch (versionHeader.FileVersionMajor)
            {
                case 1:
                case 2:
                case 3:
                    V1AxCryptDocument v1Document = new V1AxCryptDocument();
                    v1Document.Load(key, reader, headers);
                    document = v1Document;
                    break;

                case 4:
                    V2AxCryptDocument v2Document = new V2AxCryptDocument();
                    v2Document.Load(key, reader, headers);
                    document = v2Document;
                    break;

                default:
                    throw new FileFormatException("Too new file version. Please upgrade.");
            }

            return document;
        }
    }
}