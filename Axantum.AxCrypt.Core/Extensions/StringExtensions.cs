﻿#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Extension for String.Format using InvariantCulture
        /// </summary>
        /// <param name="format"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string InvariantFormat(this string format, params object[] parameters)
        {
            string formatted = String.Format(CultureInfo.InvariantCulture, format, parameters);
            return formatted;
        }

        /// <summary>
        /// Convenience extension for String.Format using the provided CultureInfo
        /// </summary>
        /// <param name="format"></param>
        /// <param name="cultureInfo"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string FormatWith(this string format, CultureInfo cultureInfo, params object[] parameters)
        {
            string formatted = String.Format(cultureInfo, format, parameters);
            return formatted;
        }

        public static string CreateUniqueFile(this string fullName)
        {
            IRuntimeFileInfo pathInfo = OS.Current.FileInfo(fullName);
            string extension = Path.GetExtension(fullName);
            int version = 0;
            while (true)
            {
                try
                {
                    string alternateExtension = (version > 0 ? "." + version.ToString(CultureInfo.InvariantCulture) : String.Empty) + extension;
                    string alternatePath = Path.Combine(Path.GetDirectoryName(pathInfo.FullName), Path.GetFileNameWithoutExtension(pathInfo.Name) + alternateExtension);
                    IRuntimeFileInfo alternateFileInfo = OS.Current.FileInfo(alternatePath);
                    alternateFileInfo.CreateNewFile();
                    return alternateFileInfo.FullName;
                }
                catch (AxCryptException ace)
                {
                    if (ace.ErrorStatus != ErrorStatus.FileExists)
                    {
                        throw;
                    }
                }
                ++version;
            }
        }

        public static string PathCombine(this string path, params string[] parts)
        {
            foreach (string part in parts)
            {
                path = Path.Combine(path, part);
            }
            return path;
        }

        /// <summary>
        /// Trim a log message from extra information in front, specifically text preceding the
        /// log level text such as Information, Warning etc. There must be a space preceding
        /// the log level text. Recognized texts are 'Information', 'Warning', 'Debug', 'Error'
        /// and 'Fatal'.
        /// </summary>
        /// <param name="message">A log message</param>
        /// <returns>A possible trimmed message</returns>
        /// <remarks>
        /// This is primarily intended to facilitate more compact logging in a GUI
        /// </remarks>
        public static string TrimLogMessage(this string message)
        {
            int skipIndex = message.IndexOf(" Information", StringComparison.Ordinal);
            skipIndex = skipIndex < 0 ? message.IndexOf(" Warning", StringComparison.Ordinal) : skipIndex;
            skipIndex = skipIndex < 0 ? message.IndexOf(" Debug", StringComparison.Ordinal) : skipIndex;
            skipIndex = skipIndex < 0 ? message.IndexOf(" Error", StringComparison.Ordinal) : skipIndex;
            skipIndex = skipIndex < 0 ? message.IndexOf(" Fatal", StringComparison.Ordinal) : skipIndex;

            return message.Substring(skipIndex + 1);
        }

        public static string FolderFromEnvironment(this string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            string value = OS.Current.EnvironmentVariable(name);
            if (String.IsNullOrEmpty(value))
            {
                return String.Empty;
            }

            value = value.Replace(Path.DirectorySeparatorChar == '/' ? '\\' : '/', Path.DirectorySeparatorChar);
            return OS.Current.FileInfo(value).NormalizeFolder().FullName;
        }
    }
}