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

using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core.Algorithm;
using AxCrypt.Core.Extensions;
using System;
using System.Text;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Crypto
{
    /// <summary>
    /// Derive a SymmetricKey from a string passphrase representation for AxCrypt. Instances of this class are immutable.
    /// </summary>
    public class V1DerivedKey : DerivedKeyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="V1DerivedKey"/> class.
        /// </summary>
        /// <param name="passphrase">The passphrase.</param>
        public V1DerivedKey(Passphrase passphrase)
        {
            if (passphrase == null)
            {
                throw new ArgumentNullException("passphrase");
            }

            HashAlgorithm hashAlgorithm = New<Sha1>();
            byte[] ansiBytes = Encoding.GetEncoding("Windows-1252").GetBytes(passphrase.Text);
            byte[] ansiBytesExtra = ansiBytes.Append(passphrase.Extra());
            byte[] hash = hashAlgorithm.ComputeHash(ansiBytesExtra);
            byte[] derivedKey = new byte[16];
            Array.Copy(hash, derivedKey, derivedKey.Length);

            DerivationSalt = Salt.Zero;
            DerivationIterations = 0;
            DerivedKey = new SymmetricKey(derivedKey);
        }
    }
}