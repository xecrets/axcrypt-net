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

using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AxCrypt.Core.Crypto
{
    /// <summary>
    /// A salt for the Symmetrical Key Wrap. Instances of this class are immutable.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Salt
    {
        [JsonProperty("salt")]
        private readonly byte[] _salt;

        /// <summary>
        /// An instance of KeyWrapSalt with all zeroes.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "The reference type 'Salt' is, in fact, immutable.")]
        public static readonly Salt Zero = new Salt(new byte[0]);

        [JsonConstructor]
        private Salt()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Salt"/> class.
        /// </summary>
        /// <param name="length">The length of the salt in bits.</param>
        public Salt(int size)
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }
            _salt = Resolve.RandomGenerator.Generate(size / 8);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Salt"/> class.
        /// </summary>
        /// <param name="length">The salt. It must be a valid symmetric key length.</param>
        public Salt(byte[] salt)
        {
            if (salt == null)
            {
                throw new ArgumentNullException("salt");
            }
            _salt = (byte[])salt.Clone();
        }

        /// <summary>
        /// Gets the length of the salt.
        /// </summary>
        ///
        public int Length
        {
            get
            {
                return _salt.Length;
            }
        }

        /// <summary>
        /// Gets the bytes of the salt.
        /// </summary>
        /// <returns>Returns the bytes of the salt.</returns>
        public byte[] GetBytes()
        {
            return (byte[])_salt.Clone();
        }
    }
}