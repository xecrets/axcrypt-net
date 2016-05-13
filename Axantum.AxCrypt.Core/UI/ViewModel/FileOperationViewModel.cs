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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class FileOperationViewModel : ViewModelBase
    {
        private FileSystemState _fileSystemState;

        private SessionNotify _sessionNotify;

        private KnownIdentities _knownIdentities;

        private ParallelFileOperation _fileOperation;

        private IStatusChecker _statusChecker;

        public IdentityViewModel IdentityViewModel { get; private set; }

        public FileOperationViewModel(FileSystemState fileSystemState, SessionNotify sessionNotify, KnownIdentities knownIdentities, ParallelFileOperation fileOperation, IStatusChecker statusChecker, IdentityViewModel identityViewModel)
        {
            _fileSystemState = fileSystemState;
            _sessionNotify = sessionNotify;
            _knownIdentities = knownIdentities;
            _fileOperation = fileOperation;
            _statusChecker = statusChecker;

            IdentityViewModel = identityViewModel;

            InitializePropertyValues();
            SubscribeToModelEvents();
        }

        private void SubscribeToModelEvents()
        {
            _sessionNotify.Notification += HandleSessionChanged;
        }

        private void InitializePropertyValues()
        {
            DecryptFiles = new DelegateAction<IEnumerable<string>>((files) => DecryptFilesAction(files));
            EncryptFiles = new DelegateAction<IEnumerable<string>>((files) => EncryptFilesAction(files));
            OpenFiles = new DelegateAction<IEnumerable<string>>((files) => OpenFilesAction(files));
            DecryptFolders = new DelegateAction<IEnumerable<string>>((folders) => DecryptFoldersAction(folders), (folders) => _knownIdentities.IsLoggedOn);
            WipeFiles = new DelegateAction<IEnumerable<string>>((files) => WipeFilesAction(files));
            RandomRenameFiles = new DelegateAction<IEnumerable<string>>((files) => RandomRenameFilesAction(files));
            OpenFilesFromFolder = new DelegateAction<string>((folder) => OpenFilesFromFolderAction(folder), (folder) => true);
            AddRecentFiles = new DelegateAction<IEnumerable<string>>((files) => AddRecentFilesAction(files));
        }

        public IAction DecryptFiles { get; private set; }

        public IAction EncryptFiles { get; private set; }

        public IAction OpenFiles { get; private set; }

        public IAction DecryptFolders { get; private set; }

        public IAction WipeFiles { get; private set; }

        public IAction RandomRenameFiles { get; private set; }

        public IAction OpenFilesFromFolder { get; private set; }

        public IAction AddRecentFiles { get; private set; }

        public event EventHandler<FileSelectionEventArgs> SelectingFiles;

        protected virtual void OnSelectingFiles(FileSelectionEventArgs e)
        {
            EventHandler<FileSelectionEventArgs> handler = SelectingFiles;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<FileOperationEventArgs> FirstLegacyOpen;

        protected virtual void OnFirstLegacyOpen(FileOperationEventArgs e)
        {
            FirstLegacyOpen?.Invoke(this, e);
        }

        public event EventHandler<FileOperationEventArgs> ToggleLegacyConversion;

        protected virtual void OnToggleLegacyConversion(FileOperationEventArgs e)
        {
            ToggleLegacyConversion?.Invoke(this, e);
        }

        private void DecryptFoldersAction(IEnumerable<string> folders)
        {
            _fileOperation.DoFiles(folders.Select(f => New<IDataContainer>(f)).ToList(), DecryptFolderWork, (status) => { });
        }

        private void EncryptFilesAction(IEnumerable<string> files)
        {
            files = files ?? SelectFiles(FileSelectionType.Encrypt);
            if (!files.Any())
            {
                return;
            }
            if (!_knownIdentities.IsLoggedOn)
            {
                IdentityViewModel.AskForLogOnPassphrase.Execute(LogOnIdentity.Empty);
            }
            if (!_knownIdentities.IsLoggedOn)
            {
                return;
            }
            _fileOperation.DoFiles(files.Select(f => New<IDataStore>(f)).ToList(), EncryptFileWork, (status) => { });
        }

        private void DecryptFilesAction(IEnumerable<string> files)
        {
            files = files ?? SelectFiles(FileSelectionType.Decrypt);
            if (!files.Any())
            {
                return;
            }
            if (!_knownIdentities.IsLoggedOn)
            {
                IdentityViewModel.AskForDecryptPassphrase.Execute(files.First());
            }
            if (!_knownIdentities.IsLoggedOn)
            {
                return;
            }
            _fileOperation.DoFiles(files.Select(f => New<IDataStore>(f)).ToList(), DecryptFileWork, (status) => { });
        }

        private void WipeFilesAction(IEnumerable<string> files)
        {
            files = files ?? SelectFiles(FileSelectionType.Wipe);
            if (!files.Any())
            {
                return;
            }
            _fileOperation.DoFiles(files.Select(f => New<IDataStore>(f)).ToList(), WipeFileWork, (status) => { });
        }

        private void RandomRenameFilesAction(IEnumerable<string> files)
        {
            files = files ?? SelectFiles(FileSelectionType.Encrypt);
            if (!files.Any())
            {
                return;
            }
            _fileOperation.DoFiles(files.Select(f => New<IDataStore>(f)).ToList(), RandomRenameFileWork, (status) => { });
        }

        private IEnumerable<string> SelectFiles(FileSelectionType fileSelectionType)
        {
            FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs(new string[0])
            {
                FileSelectionType = fileSelectionType,
            };
            OnSelectingFiles(fileSelectionArgs);
            if (fileSelectionArgs.Cancel)
            {
                return new string[0];
            }
            return fileSelectionArgs.SelectedFiles;
        }

        private void OpenFilesAction(IEnumerable<string> files)
        {
            _fileOperation.DoFiles(files.Select(f => New<IDataStore>(f)).ToList(), OpenEncryptedWork, (status) => { });
        }

        private FileOperationContext DecryptFileWork(IDataStore file, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QueryDecryptionPassphrase += HandleQueryDecryptionPassphraseEvent;

            operationsController.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
            {
                FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs(new string[] { e.SaveFileFullName })
                {
                    FileSelectionType = FileSelectionType.SaveAsDecrypted,
                };
                OnSelectingFiles(fileSelectionArgs);
                if (fileSelectionArgs.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                e.SaveFileFullName = fileSelectionArgs.SelectedFiles[0];
            };

            operationsController.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                _knownIdentities.Add(e.LogOnIdentity);
            };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (_statusChecker.CheckStatusAndShowMessage(e.Status.ErrorStatus, e.Status.FullName, e.Status.InternalMessage))
                {
                    New<ActiveFileAction>().RemoveRecentFiles(new IDataStore[] { New<IDataStore>(e.OpenFileFullName) }, progress);
                }
            };

            return operationsController.DecryptFile(file);
        }

        private FileOperationContext WipeFileWork(IDataStore file, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.WipeQueryConfirmation += (object sender, FileOperationEventArgs e) =>
            {
                FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs(new string[] { file.FullName, })
                {
                    FileSelectionType = FileSelectionType.WipeConfirm,
                };
                OnSelectingFiles(fileSelectionArgs);
                e.Cancel = fileSelectionArgs.Cancel;
                e.Skip = fileSelectionArgs.Skip;
                e.ConfirmAll = fileSelectionArgs.ConfirmAll;
                e.SaveFileFullName = fileSelectionArgs.SelectedFiles.FirstOrDefault() ?? String.Empty;
            };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (e.Skip)
                {
                    return;
                }
                if (Resolve.StatusChecker.CheckStatusAndShowMessage(e.Status.ErrorStatus, e.Status.FullName, e.Status.InternalMessage))
                {
                    New<ActiveFileAction>().RemoveRecentFiles(new IDataStore[] { New<IDataStore>(e.SaveFileFullName) }, progress);
                }
            };

            return operationsController.WipeFile(file);
        }

        private FileOperationContext RandomRenameFileWork(IDataStore file, IProgressContext progress)
        {
            file.MoveTo(file.CreateRandomUniqueName().FullName);

            return new FileOperationContext(file.FullName, ErrorStatus.Success);
        }

        private FileOperationContext OpenEncryptedWork(IDataStore file, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QueryDecryptionPassphrase += HandleQueryOpenPassphraseEvent;

            operationsController.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                if (!_fileSystemState.KnownPassphrases.Any(i => i.Thumbprint == e.LogOnIdentity.Passphrase.Thumbprint))
                {
                    _fileSystemState.KnownPassphrases.Add(e.LogOnIdentity.Passphrase);
                    _fileSystemState.Save();
                }
                _knownIdentities.Add(e.LogOnIdentity);
            };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (e.Status.ErrorStatus == ErrorStatus.Canceled)
                {
                    return;
                }
                _statusChecker.CheckStatusAndShowMessage(e.Status.ErrorStatus, e.Status.FullName ?? e.OpenFileFullName, e.Status.InternalMessage);
            };

            return operationsController.DecryptAndLaunch(file);
        }

        private void HandleQueryOpenPassphraseEvent(object sender, FileOperationEventArgs e)
        {
            HandleQueryDecryptionPassphraseEvent(sender, e);
            if (e.Cancel)
            {
                return;
            }
            if (Resolve.UserSettings.LegacyConversionMode == LegacyConversionMode.NotDecided)
            {
                OnFirstLegacyOpen(e);
            }

            if (e.Cancel)
            {
                Resolve.UserSettings.LegacyConversionMode = LegacyConversionMode.RetainLegacyFiles;
                e.Cancel = false;
                return;
            }

            if (Resolve.UserSettings.LegacyConversionMode == LegacyConversionMode.NotDecided)
            {
                OnToggleLegacyConversion(e);
            }
        }

        private void HandleQueryDecryptionPassphraseEvent(object sender, FileOperationEventArgs e)
        {
            IdentityViewModel.AskForDecryptPassphrase.Execute(e.OpenFileFullName);
            if (IdentityViewModel.LogOnIdentity == LogOnIdentity.Empty)
            {
                e.Cancel = true;
                return;
            }
            e.LogOnIdentity = IdentityViewModel.LogOnIdentity;
        }

        private FileOperationContext EncryptFileWork(IDataStore file, IProgressContext progress)
        {
            FileOperationsController operationsController = New<IProgressContext, FileOperationsController>(progress);

            operationsController.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
            {
                using (FileLock lockedSave = e.SaveFileFullName.CreateUniqueFile())
                {
                    e.SaveFileFullName = lockedSave.DataStore.FullName;
                    lockedSave.DataStore.Delete();
                }
            };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (e.Status.ErrorStatus == ErrorStatus.FileAlreadyEncrypted)
                {
                    e.Status = new FileOperationContext(String.Empty, ErrorStatus.Success);
                    return;
                }
                if (_statusChecker.CheckStatusAndShowMessage(e.Status.ErrorStatus, e.Status.FullName, e.Status.InternalMessage))
                {
                    IDataStore encryptedInfo = New<IDataStore>(e.SaveFileFullName);
                    IDataStore decryptedInfo = New<IDataStore>(FileOperation.GetTemporaryDestinationName(e.OpenFileFullName));
                    ActiveFile activeFile = new ActiveFile(encryptedInfo, decryptedInfo, e.LogOnIdentity, ActiveFileStatus.NotDecrypted, e.CryptoId);
                    _fileSystemState.Add(activeFile);
                    _fileSystemState.Save();
                }
            };

            return operationsController.EncryptFile(file);
        }

        private FileOperationContext VerifyAndAddActiveWork(IDataStore fullName, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QueryDecryptionPassphrase += HandleQueryDecryptionPassphraseEvent;

            operationsController.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                if (!_fileSystemState.KnownPassphrases.Any(i => i.Thumbprint == e.LogOnIdentity.Passphrase.Thumbprint))
                {
                    _fileSystemState.KnownPassphrases.Add(e.LogOnIdentity.Passphrase);
                    _fileSystemState.Save();
                }
                _knownIdentities.Add(e.LogOnIdentity);
            };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (_statusChecker.CheckStatusAndShowMessage(e.Status.ErrorStatus, e.OpenFileFullName, e.Status.InternalMessage))
                {
                    IDataStore encryptedInfo = New<IDataStore>(e.OpenFileFullName);
                    IDataStore decryptedInfo = New<IDataStore>(FileOperation.GetTemporaryDestinationName(e.SaveFileFullName));
                    ActiveFile activeFile = new ActiveFile(encryptedInfo, decryptedInfo, e.LogOnIdentity, ActiveFileStatus.NotDecrypted, e.CryptoId);
                    _fileSystemState.Add(activeFile);
                    _fileSystemState.Save();
                }
            };

            return operationsController.VerifyEncrypted(fullName);
        }

        private void OpenFilesFromFolderAction(string folder)
        {
            FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs(new string[] { folder })
            {
                FileSelectionType = FileSelectionType.Open,
            };
            OnSelectingFiles(fileSelectionArgs);
            if (fileSelectionArgs.Cancel)
            {
                return;
            }
            OpenFilesAction(fileSelectionArgs.SelectedFiles);
        }

        private FileOperationContext DecryptFolderWork(IDataContainer folder, IProgressContext progress)
        {
            New<AxCryptFile>().DecryptFilesInsideFolderUniqueWithWipeOfOriginal(folder, _knownIdentities.DefaultEncryptionIdentity, _statusChecker, progress);
            return new FileOperationContext(String.Empty, ErrorStatus.Success);
        }

        private void AddRecentFilesAction(IEnumerable<string> files)
        {
            IEnumerable<IDataStore> fileInfos = files.Select(f => New<IDataStore>(f)).ToList();
            ProcessEncryptableFilesDroppedInRecentList(fileInfos.Where(fileInfo => Resolve.KnownIdentities.IsLoggedOn && fileInfo.Type() == FileInfoTypes.EncryptableFile));
            ProcessEncryptedFilesDroppedInRecentList(fileInfos.Where(fileInfo => fileInfo.Type() == FileInfoTypes.EncryptedFile));
        }

        private void ProcessEncryptedFilesDroppedInRecentList(IEnumerable<IDataStore> encryptedFiles)
        {
            _fileOperation.DoFiles(encryptedFiles, VerifyAndAddActiveWork, (status) => { });
        }

        private void ProcessEncryptableFilesDroppedInRecentList(IEnumerable<IDataStore> encryptableFiles)
        {
            _fileOperation.DoFiles(encryptableFiles, EncryptFileWork, (status) => { });
        }

        private void HandleSessionChanged(object sender, SessionNotificationEventArgs e)
        {
            switch (e.Notification.NotificationType)
            {
                case SessionNotificationType.LogOff:
                case SessionNotificationType.LogOn:
                case SessionNotificationType.SessionStart:
                    ((DelegateAction<string>)OpenFilesFromFolder).RaiseCanExecuteChanged();
                    break;
            }
        }
    }
}