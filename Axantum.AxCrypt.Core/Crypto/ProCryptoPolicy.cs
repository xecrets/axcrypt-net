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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class ProCryptoPolicy : ICryptoPolicy
    {
        public ICryptoFactory DefaultCryptoFactory(IEnumerable<CryptoFactoryCreator> factories)
        {
            return factories.First(f => f().Id == CryptoFactory.Aes256Id)();
        }

        public ICryptoFactory LegacyCryptoFactory(IEnumerable<CryptoFactoryCreator> factories)
        {
            return factories.First(f => f().Id == CryptoFactory.Aes128V1Id)();
        }

        /// <summary>
        /// Return a list of CryptoId's in a suitable order of preference and relevance, to be used to
        /// try and match a passphrase against a file.
        /// </summary>
        /// <param name="factories">The available ICryptoFactory's to select from.</param>
        /// <returns>A list of CryptoId's to try in the order provided.</returns>
        public IEnumerable<Guid> OrderedCryptoIds(IEnumerable<CryptoFactoryCreator> factories)
        {
            List<Guid> orderedIds = new List<Guid>();
            orderedIds.Add(CryptoFactory.Aes256Id);
            orderedIds.AddRange(factories.Where(f => f().Id != CryptoFactory.Aes256Id && f().Id != CryptoFactory.Aes128V1Id).Select(f => f().Id));
            orderedIds.Add(CryptoFactory.Aes128V1Id);
            return orderedIds;
        }
    }
}