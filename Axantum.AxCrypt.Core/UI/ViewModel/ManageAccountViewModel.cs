﻿#region Coypright and License

/*
 * AxCrypt - Copyright 2015, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    /// <summary>
    /// List the available UserKeys for the currently logged on user, and make it possible to change the password
    /// for them all.
    /// </summary>
    public class ManageAccountViewModel : ViewModelBase
    {
        public IEnumerable<string> AccountEmails { get { return GetProperty<IEnumerable<string>>("AccountEmails"); } private set { SetProperty("AccountEmails", value); } }

        public IAction ChangePassphrase { get; private set; }

        private UserAsymmetricKeysStore _keysStore;

        public ManageAccountViewModel(UserAsymmetricKeysStore keysStore)
        {
            _keysStore = keysStore;

            InitializePropertyValues();
            BindPropertyChangedEvents();
            SubscribeToModelEvents();
        }

        private void InitializePropertyValues()
        {
            AccountEmails = _keysStore.Keys.Select(key => key.UserEmail.ToString());

            ChangePassphrase = new DelegateAction<string>((password) => ChangePassphraseAction(password), (password) => _keysStore.HasStore);
        }

        private static void BindPropertyChangedEvents()
        {
        }

        private static void SubscribeToModelEvents()
        {
        }

        private object ChangePassphraseAction(string passphrase)
        {
            _keysStore.Save(new Passphrase(passphrase));

            return null;
        }
    }
}