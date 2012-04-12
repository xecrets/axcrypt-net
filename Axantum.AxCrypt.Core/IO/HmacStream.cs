﻿#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;

namespace Axantum.AxCrypt.Core.IO
{
    public class HmacStream : Stream
    {
        private HashAlgorithm _hmac;

        private Stream _chainedStream;

        private long _count = 0;

        /// <summary>
        /// A AxCrypt HMAC-calculating stream. This uses the AxCrypt variant with a block size of 20 for the key.
        /// </summary>
        /// <param name="key">The key for the HMAC</param>
        public HmacStream(AesKey key)
            : this(key, null)
        {
        }

        /// <summary>
        /// An AxCrypt HMAC SHA1-calculating stream. This uses the AxCrypt variant with a block size of 20 for the key.
        /// </summary>
        /// <param name="key">The key for the HMAC</param>
        /// <param name="chainedStream">A stream where data is chain-written to. This stream is not disposed of when this instance is disposed.</param>
        public HmacStream(AesKey key, Stream chainedStream)
        {
            _hmac = AxCryptHMACSHA1.Create(key);
            _chainedStream = chainedStream;
        }

        private byte[] _result = null;

        /// <summary>
        /// Get the calculated HMAC
        /// </summary>
        /// <returns>A HMAC truncated to 128 bits</returns>
        public DataHmac HmacResult
        {
            get
            {
                if (_result == null)
                {
                    _hmac.TransformFinalBlock(new byte[] { }, 0, 0);
                    _result = _hmac.Hash;
                }
                byte[] result = new byte[16];
                Array.Copy(_result, 0, result, 0, result.Length);
                return new DataHmac(result);
            }
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get { return _count; }
        }

        public override long Position
        {
            get
            {
                return _count;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteInternal(buffer, offset, count);
            if (_chainedStream != null)
            {
                _chainedStream.Write(buffer, offset, count);
            }
        }

        private void WriteInternal(byte[] buffer, int offset, int count)
        {
            _hmac.TransformBlock(buffer, offset, count, null, 0);
            _count += count;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_hmac != null)
                {
                    _hmac.Clear();
                    _hmac = null;
                }
            }
            base.Dispose(disposing);
        }

        public void ReadFrom(Stream outputCipherStream)
        {
            byte[] buffer = new byte[4096];
            int count;
            while ((count = outputCipherStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                WriteInternal(buffer, 0, count);
            }
        }
    }
}