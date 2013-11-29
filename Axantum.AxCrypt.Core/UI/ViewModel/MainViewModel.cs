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

using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            LogonEnabled = true;

            OS.Current.SessionChanged += Current_SessionChanged;
        }

        void Current_SessionChanged(object sender, SessionEventArgs e)
        {
            foreach (SessionEvent sessionEvent in e.SessionEvents)
            {
                switch (sessionEvent.SessionEventType)
                {
                    case SessionEventType.ActiveFileChange:
                        break;
                    case SessionEventType.WatchedFolderAdded:
                        break;
                    case SessionEventType.WatchedFolderRemoved:
                        break;
                    case SessionEventType.LogOn:
                        Instance.UIThread.RunOnUIThread(() => SetLogOnState(Instance.KnownKeys.IsLoggedOn));
                        break;
                    case SessionEventType.LogOff:
                        Instance.UIThread.RunOnUIThread(() => SetLogOnState(Instance.KnownKeys.IsLoggedOn));
                        break;
                    case SessionEventType.ProcessExit:
                        break;
                    case SessionEventType.SessionChange:
                        break;
                    case SessionEventType.SessionStart:
                        break;
                    case SessionEventType.KnownKeyChange:
                        break;
                    case SessionEventType.WorkFolderChange:
                        break;
                    default:
                        break;
                }
            }
        }

        public bool LogonEnabled { get { return GetProperty<bool>("LogonEnabled"); } set { SetProperty("LogonEnabled", value); } }

        public bool EncryptFileEnabled { get { return GetProperty<bool>("EncryptFileEnabled"); } set { SetProperty("EncryptFileEnabled", value); } }

        public bool DecryptFileEnabled { get { return GetProperty<bool>("DecryptFileEnabled"); } set { SetProperty("DecryptFileEnabled", value); } }

        public bool OpenEncryptedEnabled { get { return GetProperty<bool>("OpenEncryptedEnabled"); } set { SetProperty("OpenEncryptedEnabled", value); } }

        public void LogOnLogOff()
        {
            LogOnLogOffInternal();
        }

        private void SetLogOnState(bool isLoggedOn)
        {
            LogonEnabled = !isLoggedOn;
            EncryptFileEnabled = isLoggedOn;
            DecryptFileEnabled = isLoggedOn;
            OpenEncryptedEnabled = isLoggedOn;
        }

        private void LogOnLogOffInternal()
        {
            if (Instance.KnownKeys.IsLoggedOn)
            {
                Instance.KnownKeys.Clear();
                return;
            }

            if (Instance.FileSystemState.Identities.Any(identity => true))
            {
                TryLogOnToExistingIdentity();
                return;
            }

            string passphrase = AskForNewEncryptionPassphrase(String.Empty);
            if (String.IsNullOrEmpty(passphrase))
            {
                return;
            }

            Instance.KnownKeys.DefaultEncryptionKey = Passphrase.Derive(passphrase);
        }

        private void TryLogOnToExistingIdentity()
        {
            string passphrase = AskForLogOnPassphrase(PassphraseIdentity.Empty);
            if (String.IsNullOrEmpty(passphrase))
            {
                return;
            }
        }

        public void EncryptFiles()
        {
            FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs()
            {
                FileSelectionType = FileSelectionType.Encrypt,
            };
            OnSelectingFiles(fileSelectionArgs);
            if (fileSelectionArgs.Cancel)
            {
                return;
            }
            EncryptFiles(fileSelectionArgs.SelectedFiles.Select(f => OS.Current.FileInfo(f)));
        }

        public void EncryptFiles(IEnumerable<IRuntimeFileInfo> files)
        {
            Instance.ParallelBackground.DoFiles(files, EncryptFile, (status) => { });
        }

        private FileOperationStatus EncryptFile(IRuntimeFileInfo file, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
            {
                FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs()
                {
                    FileSelectionType = FileSelectionType.SaveAs,
                    SelectedFiles = new string[] { e.SaveFileFullName },
                };
                OnSelectingFiles(fileSelectionArgs);
                if (fileSelectionArgs.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                e.SaveFileFullName = fileSelectionArgs.SelectedFiles[0];
            };

            operationsController.QueryEncryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                string passphrase = AskForLogOnPassphrase(PassphraseIdentity.Empty);
                if (String.IsNullOrEmpty(passphrase))
                {
                    e.Cancel = true;
                    return;
                }
                e.Passphrase = passphrase;
            };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (e.Status == FileOperationStatus.FileAlreadyEncrypted)
                {
                    e.Status = FileOperationStatus.Success;
                    return;
                }
                if (FactoryRegistry.Instance.Singleton<IStatusChecker>().CheckStatusAndShowMessage(e.Status, e.OpenFileFullName))
                {
                    IRuntimeFileInfo encryptedInfo = OS.Current.FileInfo(e.SaveFileFullName);
                    IRuntimeFileInfo decryptedInfo = OS.Current.FileInfo(FileOperation.GetTemporaryDestinationName(e.OpenFileFullName));
                    ActiveFile activeFile = new ActiveFile(encryptedInfo, decryptedInfo, e.Key, ActiveFileStatus.NotDecrypted);
                    Instance.FileSystemState.Add(activeFile);
                    Instance.FileSystemState.Save();
                }
            };

            return operationsController.EncryptFile(file);
        }

        public event EventHandler<FileSelectionEventArgs> SelectingFiles;

        protected virtual void OnSelectingFiles(FileSelectionEventArgs e)
        {
            EventHandler<FileSelectionEventArgs> handler = SelectingFiles;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<LogOnEventArgs> LoggingOn;

        protected virtual void OnLogggingOn(LogOnEventArgs e)
        {
            EventHandler<LogOnEventArgs> handler = LoggingOn;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private string AskForLogOnPassphrase(PassphraseIdentity identity)
        {
            string passphrase = AskForLogOnOrEncryptionPassphrase(identity);
            if (passphrase.Length == 0)
            {
                return String.Empty;
            }

            Instance.KnownKeys.DefaultEncryptionKey = Passphrase.Derive(passphrase);
            return passphrase;
        }

        private string AskForLogOnOrEncryptionPassphrase(PassphraseIdentity identity)
        {
            LogOnEventArgs logOnArgs = new LogOnEventArgs()
            {
                DisplayPassphrase = Instance.FileSystemState.Settings.DisplayEncryptPassphrase,
            };
            OnLogggingOn(logOnArgs);

            if (logOnArgs.CreateNew)
            {
                return AskForNewEncryptionPassphrase(logOnArgs.Passphrase);
            }

            if (logOnArgs.Cancel || logOnArgs.Passphrase.Length == 0)
            {
                return String.Empty;
            }

            if (logOnArgs.DisplayPassphrase != Instance.FileSystemState.Settings.DisplayEncryptPassphrase)
            {
                Instance.FileSystemState.Settings.DisplayEncryptPassphrase = logOnArgs.DisplayPassphrase;
                Instance.FileSystemState.Save();
            }

            return logOnArgs.Passphrase;
        }

        private string AskForNewEncryptionPassphrase(string defaultPassphrase)
        {
            LogOnEventArgs logOnArgs = new LogOnEventArgs()
            {
                CreateNew = true,
                DisplayPassphrase = Instance.FileSystemState.Settings.DisplayEncryptPassphrase,
                Passphrase = defaultPassphrase
            };
            OnLogggingOn(logOnArgs);

            if (logOnArgs.Cancel || logOnArgs.Passphrase.Length == 0)
            {
                return String.Empty;
            }
            
            if (logOnArgs.DisplayPassphrase != Instance.FileSystemState.Settings.DisplayEncryptPassphrase)
            {
                Instance.FileSystemState.Settings.DisplayEncryptPassphrase = logOnArgs.DisplayPassphrase;
                Instance.FileSystemState.Save();
            }

            Passphrase passphrase = new Passphrase(logOnArgs.Passphrase);
            PassphraseIdentity identity = Instance.FileSystemState.Identities.FirstOrDefault(i => i.Thumbprint == passphrase.DerivedPassphrase.Thumbprint);
            if (identity != null)
            {
                return logOnArgs.Passphrase;
            }

            identity = new PassphraseIdentity(logOnArgs.Passphrase, passphrase.DerivedPassphrase);
            Instance.FileSystemState.Identities.Add(identity);
            Instance.FileSystemState.Save();

            return logOnArgs.Passphrase;
        }
    }
}