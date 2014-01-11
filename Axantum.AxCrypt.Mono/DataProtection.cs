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
using System.Security.Cryptography;
using System.Text;
using Axantum.AxCrypt.Core.Runtime;

namespace Axantum.AxCrypt.Mono
{
    internal class DataProtection : IDataProtection
    {
        #region IDataProtection Members

        public byte[] Protect(byte[] unprotectedData)
        {
            return ProtectedData.Protect(unprotectedData, null, DataProtectionScope.CurrentUser);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            byte[] bytes = ProtectedData.Unprotect(protectedData, null, DataProtectionScope.CurrentUser);
            return bytes;
        }

        #endregion IDataProtection Members
    }
}