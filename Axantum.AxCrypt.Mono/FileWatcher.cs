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

using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Mono
{
    public class FileWatcher : IFileWatcher
    {
        private IRuntimeFileInfo _fileInfo;

        private FileSystemWatcher _fileSystemWatcher;

        private DelayedAction _delayedAction;

        private List<FileWatcherEventArgs> _notifications = new List<FileWatcherEventArgs>();

        public FileWatcher(string path, DelayedAction delayedAction)
        {
            _delayedAction = delayedAction;
            _delayedAction.Action += (sender, e) => { OnDelayedNotification(); };

            _fileInfo = Factory.New<IRuntimeFileInfo>(path);
            _fileSystemWatcher = new FileSystemWatcher(_fileInfo.FullName);
            _fileSystemWatcher.Created += (sender, e) => FileSystemChanged(new FileWatcherEventArgs(e.FullPath));
            _fileSystemWatcher.Deleted += (sender, e) => FileSystemChanged(new FileWatcherEventArgs(e.FullPath));
            _fileSystemWatcher.Renamed += (sender, e) => FileSystemChanged(new FileWatcherEventArgs(e.OldFullPath, e.FullPath));
            _fileSystemWatcher.Error += (sender, e) => FileSystemChanged(new FileWatcherEventArgs(_fileInfo.FullName));

            _fileSystemWatcher.Filter = String.Empty;
            _fileSystemWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime;
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        public bool IncludeSubdirectories
        {
            get
            {
                return _fileSystemWatcher.IncludeSubdirectories;
            }
            set
            {
                _fileSystemWatcher.EnableRaisingEvents = false;
                _fileSystemWatcher.IncludeSubdirectories = value;
                _fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        protected virtual void OnDelayedNotification()
        {
            List<FileWatcherEventArgs> notifications;
            lock (_notifications)
            {
                if (!_notifications.Any())
                {
                    return;
                }
                notifications = new List<FileWatcherEventArgs>(_notifications);
                _notifications.Clear();
            }

            foreach (FileWatcherEventArgs notification in notifications)
            {
                OnChanged(notification);
            }
        }

        protected virtual void OnChanged(FileWatcherEventArgs eventArgs)
        {
            EventHandler<FileWatcherEventArgs> fileChanged = FileChanged;
            if (fileChanged != null)
            {
                fileChanged(null, eventArgs);
            }
        }

        private void FileSystemChanged(FileWatcherEventArgs e)
        {
            if (Instance.Log.IsInfoEnabled)
            {
                Instance.Log.LogInfo("Watcher says '{0}' changed.".InvariantFormat(e.FullName));
            }
            lock (_notifications)
            {
                _notifications.Add(e);
            }
            _delayedAction.StartIdleTimer();
        }

        #region IFileWatcher Members

        public event EventHandler<FileWatcherEventArgs> FileChanged;

        #endregion IFileWatcher Members

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                if (_fileSystemWatcher != null)
                {
                    _fileSystemWatcher.Dispose();
                    _fileSystemWatcher = null;
                }
                if (_delayedAction != null)
                {
                    _delayedAction.Dispose();
                    _delayedAction = null;
                }
            }
            _disposed = true;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members
    }
}