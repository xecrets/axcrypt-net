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

namespace AxCrypt.Core.Header
{
    public class VersionHeaderBlock : HeaderBlock
    {
        public VersionHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.Version, dataBlock)
        {
        }

        public override object Clone()
        {
            VersionHeaderBlock block = new VersionHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return block;
        }

        public void SetCurrentVersion(byte[] version)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }

            Array.Copy(version, GetDataBlockBytesReference(), version.Length);
        }

        /// <summary>
        /// FileMajor - Older versions cannot not read the format.
        /// </summary>
        public byte FileVersionMajor
        {
            get
            {
                return GetDataBlockBytesReference()[0];
            }
            set
            {
                GetDataBlockBytesReference()[0] = value;
            }
        }

        /// <summary>
        /// FileMinor - Older versions can read the format, but will not retain on save.
        /// </summary>
        public byte FileVersionMinor
        {
            get
            {
                return GetDataBlockBytesReference()[1];
            }
        }

        /// <summary>
        /// Major - New release, major functionality change.
        /// </summary>
        public byte VersionMajor
        {
            get
            {
                return GetDataBlockBytesReference()[2];
            }
        }

        /// <summary>
        /// Minor - Changes, but no big deal.
        /// </summary>
        public byte VersionMinor
        {
            get
            {
                return GetDataBlockBytesReference()[3];
            }
        }

        /// <summary>
        /// Minuscule - bug fix.
        /// </summary>
        public byte VersionMinuscule
        {
            get
            {
                return GetDataBlockBytesReference()[4];
            }
        }
    }
}