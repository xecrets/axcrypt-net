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

namespace Axantum.AxCrypt.Core.IO
{
    /// <summary>
    /// Mark that this instance actually represents a folder. Used together with IRuntimeFileInfo
    /// </summary>
    public interface IRuntimeFolderInfo : IRuntimeFileInfo
    {
        /// <summary>
        /// Get a file item from this instance (which must represent a folder or container)..
        /// </summary>
        /// <param name="item">The name of the file item.</param>
        /// <returns>
        /// A new instance representing the file item in the folder or container.
        /// </returns>
        IRuntimeFileInfo FileItemInfo(string item);

        /// <summary>
        /// Get a folder item from this instance (which must represent a folder or container)..
        /// </summary>
        /// <param name="item">The name of the file item.</param>
        /// <returns>
        /// A new instance representing the file item in the folder or container.
        /// </returns>
        IRuntimeFolderInfo FolderItemInfo(string item);
    }
}