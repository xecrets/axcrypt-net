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

using AxCrypt.Core.Extensions;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;

namespace AxCrypt.Core.Crypto
{
    /// <summary>
    /// Represent a salted thumb print for a symmetric key. Instances of this class are immutable. A thumb print is
    /// typically only valid and comparable on the same computer and log on where it was created. However, it *is*
    /// only based on the passphrase, and some user-specific values. It always uses the same crypto, the same
    /// passphrase will have the same thumbprint regardless of which crypto is actually used when encrypting data
    /// with the corresponding passphrase.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class SymmetricKeyThumbprint : IEquatable<SymmetricKeyThumbprint>
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "This class is immutable.")]
        public static readonly SymmetricKeyThumbprint Zero = new SymmetricKeyThumbprint(new byte[8]);

        [JsonProperty("thumbprint")]
        private byte[] _bytes;

        [JsonConstructor]
        private SymmetricKeyThumbprint(byte[] bytes)
        {
            _bytes = bytes;
        }

        /// <summary>
        /// Instantiate a thumb print
        /// </summary>
        /// <param name="passphrase">The passphrase to thumbprint.</param>
        /// <param name="salt">The salt to use.</param>
        public SymmetricKeyThumbprint(Passphrase passphrase, Salt salt, long keyWrapIterations)
        {
            if (passphrase == null)
            {
                throw new ArgumentNullException("passphrase");
            }
            if (salt == null)
            {
                throw new ArgumentNullException("salt");
            }

            ICryptoFactory factory = Resolve.CryptoFactory.Minimum;
            ICrypto crypto = factory.CreateCrypto(factory.RestoreDerivedKey(passphrase, salt, CryptoFactory.DerivationIterations).DerivedKey, null, 0);
            KeyWrap keyWrap = new KeyWrap(salt, keyWrapIterations, KeyWrapMode.Specification);
            byte[] wrap = keyWrap.Wrap(crypto, crypto.Key);

            _bytes = wrap.Reduce(6);
        }

        #region IEquatable<AesKeyThumbprint> Members

        public bool Equals(SymmetricKeyThumbprint other)
        {
            if ((object)other == null)
            {
                return false;
            }

            return _bytes.IsEquivalentTo(other._bytes);
        }

        #endregion IEquatable<AesKeyThumbprint> Members

        public override bool Equals(object obj)
        {
            if (obj == null || typeof(SymmetricKeyThumbprint) != obj.GetType())
            {
                return false;
            }
            SymmetricKeyThumbprint other = (SymmetricKeyThumbprint)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            int hashcode = 0;
            foreach (byte b in _bytes)
            {
                hashcode += b;
            }
            return hashcode;
        }

        public static bool operator ==(SymmetricKeyThumbprint left, SymmetricKeyThumbprint right)
        {
            if (Object.ReferenceEquals(left, right))
            {
                return true;
            }
            if ((object)left == null)
            {
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(SymmetricKeyThumbprint left, SymmetricKeyThumbprint right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            byte[] bytesExtended = new byte[8];
            Array.Copy(_bytes, bytesExtended, _bytes.Length);
            UInt64 thumbprint = BitConverter.ToUInt64(bytesExtended, 0);
            return thumbprint.ToString("x12", CultureInfo.InvariantCulture);
        }
    }
}