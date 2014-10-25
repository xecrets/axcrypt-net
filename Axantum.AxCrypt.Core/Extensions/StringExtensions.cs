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

using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

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

        /// <summary>
        /// Create a file name based on an existing, but convert the file name to the pattern used by
        /// AxCrypt for encrypted files. The original must not already be in that form.
        /// </summary>
        /// <param name="fileInfo">A file name representing a file that is not encrypted</param>
        /// <returns>A corresponding file name representing the encrypted version of the original</returns>
        public static string CreateEncryptedName(this string fullName)
        {
            string extension = Resolve.Portable.Path().GetExtension(fullName);
            string encryptedName = fullName;
            encryptedName = encryptedName.Substring(0, encryptedName.Length - extension.Length);
            encryptedName += extension.Replace('.', '-');
            encryptedName += OS.Current.AxCryptExtension;

            return encryptedName;
        }

        public static string CreateUniqueFile(this string fullName)
        {
            IDataStore pathInfo = TypeMap.Resolve.New<IDataStore>(fullName);
            string extension = Resolve.Portable.Path().GetExtension(fullName);
            int version = 0;
            while (true)
            {
                try
                {
                    string alternateExtension = (version > 0 ? "." + version.ToString(CultureInfo.InvariantCulture) : String.Empty) + extension;
                    string alternateName = Resolve.Portable.Path().GetFileNameWithoutExtension(pathInfo.Name) + alternateExtension;
                    IDataStore alternateFileInfo = pathInfo.Container.CreateNewFile(alternateName);
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

        /// <summary>
        /// Gets a representation of a data container (folder) from an environment name or similar.
        /// </summary>
        /// <param name="name">The environment name.</param>
        /// <returns>A container, or null if the name was not found or was empty.</returns>
        /// <exception cref="System.ArgumentNullException">name</exception>
        public static IDataContainer FolderFromEnvironment(this string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            string value = OS.Current.EnvironmentVariable(name);
            if (String.IsNullOrEmpty(value))
            {
                return null;
            }

            return TypeMap.Resolve.New<IDataContainer>(value);
        }

        public static string NormalizeFilePath(this string filePath)
        {
            filePath = filePath.Replace(Resolve.Portable.Path().DirectorySeparatorChar == '/' ? '\\' : '/', Resolve.Portable.Path().DirectorySeparatorChar);
            return filePath;
        }

        public static string NormalizeFolderPath(this string folder)
        {
            folder = folder.NormalizeFilePath();
            if (String.Compare(folder, Resolve.Portable.Path().GetPathRoot(folder), StringComparison.OrdinalIgnoreCase) == 0)
            {
                return folder;
            }
            int directorySeparatorChars = 0;
            while (folder.Length - (directorySeparatorChars + 1) > 0 && folder[folder.Length - (directorySeparatorChars + 1)] == Resolve.Portable.Path().DirectorySeparatorChar)
            {
                ++directorySeparatorChars;
            }

            if (directorySeparatorChars == 0)
            {
                return folder + Resolve.Portable.Path().DirectorySeparatorChar;
            }
            return folder.Substring(0, folder.Length - (directorySeparatorChars - 1));
        }

        public static byte[] FromHex(this string hex)
        {
            if (hex == null)
            {
                throw new ArgumentNullException("hex");
            }
            hex = hex.Replace(" ", String.Empty).Replace("\r", String.Empty).Replace("\n", String.Empty);
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException("Odd number of characters is not allowed in a hex string.");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; ++i)
            {
                bytes[i] = Byte.Parse(hex.Substring(i + i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return bytes;
        }

        public static bool IsValidEmailOrEmpty(this string email)
        {
            if (String.IsNullOrEmpty(email))
            {
                return true;
            }
            return email.IsValidEmail();
        }

        public static bool IsValidEmail(this string email)
        {
            try
            {
                new EmailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}