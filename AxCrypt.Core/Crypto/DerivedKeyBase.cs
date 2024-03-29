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

using System;
using System.Linq;

namespace AxCrypt.Core.Crypto
{
    public abstract class DerivedKeyBase : IDerivedKey
    {
        public SymmetricKey DerivedKey
        {
            get;
            protected set;
        }

        public Salt DerivationSalt
        {
            get;
            protected set;
        }

        public int DerivationIterations { get; protected set; }

        #region IEquatable<SymmetricKey> Members

        /// <summary>
        /// Check if one instance is equivalent to another.
        /// </summary>
        /// <param name="other">The instance to compare to</param>
        /// <returns>true if the keys are equivalent</returns>
        public bool Equals(IDerivedKey other)
        {
            if (Object.ReferenceEquals(other, null))
            {
                return false;
            }
            return DerivedKey == other.DerivedKey;
        }

        #endregion IEquatable<SymmetricKey> Members

        public override bool Equals(object obj)
        {
            IDerivedKey other = obj as IDerivedKey;
            if (other == null)
            {
                return false;
            }

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return DerivedKey.GetHashCode();
        }
    }
}