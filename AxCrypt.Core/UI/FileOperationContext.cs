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

using AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Core.UI
{
    public class FileOperationContext
    {
        public FileOperationContext(string fullName, string internalMessage, ErrorStatus errorStatus)
        {
            FullName = fullName ?? string.Empty;
            ErrorStatus = errorStatus;
            InternalMessage = internalMessage ?? string.Empty;
        }

        public FileOperationContext(string fullName, ErrorStatus errorStatus)
            : this(fullName, string.Empty, errorStatus)
        {
        }

        public FileOperationContext(ProgressTotals totals)
            : this(string.Empty, ErrorStatus.Success)
        {
            Totals = totals;
        }

        public string FullName { get; private set; }

        public ErrorStatus ErrorStatus { get; private set; }

        public string InternalMessage { get; private set; }

        public ProgressTotals Totals { get; private set; } = new ProgressTotals();
    }
}