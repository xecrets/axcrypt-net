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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{

    /// <summary>
    /// Log On (enable default encryption key). If an email address is provided, an attempt is made to log on to the Internet
    /// server and synchronize keys. If that fails, a locally accessible key-pair is required.
    /// If no email address is provided, classic mode encryption is attempted, if this is enabled by the current feature policy.
    /// </summary>
    public class LogOnAccountViewModel : ViewModelBase
    {
        private UserSettings _userSettings;
        private int _nrOfTries = 0;
        private readonly int MAX_TRIES = 3;
        public event EventHandler TooManyTries;

        public LogOnAccountViewModel(UserSettings userSettings)
        {
            if (userSettings == null)
            {
                throw new ArgumentNullException("userSettings");
            }

            _userSettings = userSettings;

            InitializePropertyValues(_userSettings.UserEmail);
            BindPropertyChangedEvents();
        }

        private void InitializePropertyValues(string userEmail)
        {
            UserEmail = userEmail;
            Passphrase = String.Empty;
            ShowPassphrase = New<UserSettings>().DisplayDecryptPassphrase;
            ShowEmail = true;
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal(nameof(ShowPassphrase), (bool show) => New<UserSettings>().DisplayDecryptPassphrase = show);
            BindPropertyChangedInternal(nameof(ShowEmail), (bool show) => { if (!ShowEmail) UserEmail = String.Empty; });
            BindPropertyChangedInternal(nameof(UserEmail), async (string userEmail) => { if (await ValidateAsync(nameof(UserEmail))) { _userSettings.UserEmail = userEmail; } });
        }

        public bool ShowPassphrase { get { return GetProperty<bool>(nameof(ShowPassphrase)); } set { SetProperty(nameof(ShowPassphrase), value); } }

        public string Passphrase { get { return GetProperty<string>(nameof(Passphrase)); } set { SetProperty(nameof(Passphrase), value); } }

        public string UserEmail { get { return GetProperty<string>(nameof(UserEmail)); } set { SetProperty(nameof(UserEmail), value); } }

        public bool ShowEmail { get { return GetProperty<bool>(nameof(ShowEmail)); } private set { SetProperty(nameof(ShowEmail), value); } }

        protected override async Task<bool> ValidateAsync(string columnName)
        {
            switch (columnName)
            {
                case nameof(UserEmail):
                    if (!ShowEmail)
                    {
                        return true;
                    }
                    if (!UserEmail.IsValidEmailOrEmpty())
                    {
                        ValidationError = (int)ViewModel.ValidationError.InvalidEmail;
                        return false;
                    }
                    return true;

                case nameof(Passphrase):
                    return await ValidatePassphraseAsync();

                case nameof(ShowPassphrase):
                case nameof(ShowEmail):
                    return true;

                default:
                    throw new ArgumentException("Cannot validate property.", columnName);
            }
        }

        private async Task<bool> ValidatePassphraseAsync()
        {
            if (ShowEmail && UserEmail.Length > 0)
            {
                return await ValidatePassphraseForEmailAsync();
            }
            if (IsKnownPassphrase())
            {
                return true;
            }

            ValidationError = (int)ViewModel.ValidationError.WrongPassphrase;
            return false;
        }

        private async Task<bool> ValidatePassphraseForEmailAsync()
        {

            if (!UserEmail.IsValidEmail())
            {
                return true;
            }

            if (Passphrase.Length > 0 && await IsValidAccountLogOnAsync())
            {
                _nrOfTries = 0;
                return true;
            }

            _nrOfTries++;
            if (_nrOfTries == MAX_TRIES)
            {
                TooManyTries?.Invoke(this, null);
            }

            ValidationError = (int)ViewModel.ValidationError.WrongPassphrase;
            return false;
        }

        private async Task<bool> IsValidAccountLogOnAsync()
        {
            AccountStorage accountStorage = new AccountStorage(New<LogOnIdentity, IAccountService>(new LogOnIdentity(EmailAddress.Parse(UserEmail), new Passphrase(Passphrase))));

            return await accountStorage.IsIdentityValidAsync();
        }

        private bool IsKnownPassphrase()
        {
            SymmetricKeyThumbprint thumbprint = new Passphrase(Passphrase).Thumbprint;
            Passphrase knownPassphrase = New<FileSystemState>().KnownPassphrases.FirstOrDefault(id => id.Thumbprint == thumbprint);
            if (knownPassphrase != null)
            {
                return true;
            }
            return false;
        }
    }
}