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

using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.IO;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Session
{
    /// <summary>
    /// Holds information about a folder that is watched for file changes, to enable
    /// automatic encryption of files for example. Instances of this class are
    /// immutable
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class WatchedFolder : IDisposable
    {
        [JsonProperty("path")]
        public string Path { get; private set; }

        private IFileWatcher _fileWatcher;

        public event EventHandler<FileWatcherEventArgs> Changed;

        [JsonConstructor]
        private WatchedFolder()
        {
            Tag = IdentityPublicTag.Empty;
            KeyShares = new List<EmailAddress>();
        }

        public WatchedFolder(string path, IdentityPublicTag publicTag)
            : this()
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (publicTag == null)
            {
                throw new ArgumentNullException("publicTag");
            }

            Path = path.NormalizeFolderPath();
            Tag = publicTag;
            InitializeFileWatcher();
        }

        public WatchedFolder(WatchedFolder watchedFolder, IEnumerable<UserPublicKey> keyShares)
        {
            if (watchedFolder == null)
            {
                throw new ArgumentNullException(nameof(watchedFolder));
            }

            Path = watchedFolder.Path;
            Tag = watchedFolder.Tag;

            KeyShares = keyShares.Select(ks => ks.Email).ToArray();

            InitializeFileWatcher();
        }

        [JsonProperty("publicTag")]
        public IdentityPublicTag Tag
        {
            get;
            private set;
        }

        [JsonProperty("keyShares")]
        public IEnumerable<EmailAddress> KeyShares
        {
            get;
            private set;
        }

        [JsonProperty("isDeleted")]
        public bool IsDeleted
        {
            get;
            set;
        }

        public bool IsKnownFolder
        {
            get
            {
                return New<IKnownFoldersDiscovery>().Discover().Any(kf => kf.My.FullName == Path);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context")]
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            InitializeFileWatcher();
        }

        private void InitializeFileWatcher()
        {
            if (!IsDeleted && New<IDataContainer>(Path).IsAvailable)
            {
                _fileWatcher = New<IFileWatcher>(Path);
                _fileWatcher.FileChanged += _fileWatcher_FileChanged;
                _fileWatcher.IncludeSubdirectories = true;
            }
        }

        private void _fileWatcher_FileChanged(object sender, FileWatcherEventArgs e)
        {
            if (!New<LicensePolicy>().Capabilities.Has(LicenseCapability.SecureFolders))
            {
                return;
            }
            OnChanged(e);
        }

        protected virtual void OnChanged(FileWatcherEventArgs e)
        {
            Changed?.Invoke(this, e);
        }

        public bool Matches(string path)
        {
            return string.Compare(Path, path, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public bool Matches(WatchedFolder watchedFolder)
        {
            if (watchedFolder == null)
            {
                throw new ArgumentNullException("watchedFolder");
            }

            return Matches(watchedFolder.Path);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
        }

        private void DisposeInternal()
        {
            if (_fileWatcher != null)
            {
                _fileWatcher.Dispose();
                _fileWatcher = null;
            }
        }

        #endregion IDisposable Members
    }
}