﻿#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using System;

namespace AxCrypt.Core.Header
{
    public class V1EncryptionInfoEncryptedHeaderBlock : EncryptedHeaderBlock
    {
        public V1EncryptionInfoEncryptedHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.EncryptionInfo, dataBlock)
        {
        }

        public V1EncryptionInfoEncryptedHeaderBlock(ICrypto headerCrypto)
            : this(new byte[0])
        {
            HeaderCrypto = headerCrypto;
            PlaintextLength = 0;
            IV = new SymmetricIV(new byte[16]);
        }

        public override object Clone()
        {
            V1EncryptionInfoEncryptedHeaderBlock block = new V1EncryptionInfoEncryptedHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return CopyTo(block);
        }

        private void EnsureDataBlock()
        {
            if (GetDataBlockBytesReference().Length > 0)
            {
                return;
            }

            SetDataBlockBytesReference(HeaderCrypto.Encrypt(new byte[32]));
        }

        public long PlaintextLength
        {
            get
            {
                EnsureDataBlock();
                byte[] rawData = HeaderCrypto.Decrypt(GetDataBlockBytesReference());

                long plaintextLength = rawData.GetLittleEndianValue(0, sizeof(long));
                return plaintextLength;
            }

            set
            {
                EnsureDataBlock();
                byte[] rawData = HeaderCrypto.Decrypt(GetDataBlockBytesReference());

                byte[] plaintextLengthBytes = value.GetLittleEndianBytes();
                Array.Copy(plaintextLengthBytes, 0, rawData, 0, plaintextLengthBytes.Length);

                byte[] encryptedData = HeaderCrypto.Encrypt(rawData);
                SetDataBlockBytesReference(encryptedData);
            }
        }

        public SymmetricIV IV
        {
            get
            {
                EnsureDataBlock();
                byte[] rawData = HeaderCrypto.Decrypt(GetDataBlockBytesReference());

                byte[] iv = new byte[16];
                Array.Copy(rawData, 8, iv, 0, iv.Length);

                return new SymmetricIV(iv);
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                EnsureDataBlock();
                byte[] rawData = HeaderCrypto.Decrypt(GetDataBlockBytesReference());

                byte[] encryptedIV = HeaderCrypto.Encrypt(value.GetBytes());
                Array.Copy(encryptedIV, 0, rawData, 8, encryptedIV.Length);

                byte[] encryptedData = HeaderCrypto.Encrypt(rawData);
                SetDataBlockBytesReference(encryptedData);
            }
        }
    }
}