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

using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class ImportPublicKeysViewModel : ViewModelBase
    {
        private IDataStore _store;

        private IStringSerializer _serializer;

        public ImportPublicKeysViewModel(IDataStore store, IStringSerializer serializer)
        {
            _store = store;
            _serializer = serializer;

            InitializePropertyValues();
            BindPropertyChangedEvents();
            SubscribeToModelEvents();
        }

        public IAction ImportFiles { get; private set; }

        public IEnumerable<string> FailedFiles { get { return GetProperty<IEnumerable<string>>("FailedFiles"); } set { SetProperty("FailedFiles", value); } }

        private void InitializePropertyValues()
        {
            ImportFiles = new DelegateAction<IEnumerable<string>>((files) => ImportFilesAction(files));
        }

        private static void BindPropertyChangedEvents()
        {
        }

        private static void SubscribeToModelEvents()
        {
        }

        private void ImportFilesAction(IEnumerable<string> files)
        {
            List<string> failed = new List<string>();
            using (KnownPublicKeys knownPublicKeys = KnownPublicKeys.Load(_store, _serializer))
            {
                foreach (string file in files)
                {
                    IDataStore publicKeyData = TypeMap.Resolve.New<IDataStore>(file);
                    UserPublicKey publicKey = null;
                    try
                    {
                        publicKey = _serializer.Deserialize<UserPublicKey>(publicKeyData);
                    }
                    catch (JsonException)
                    {
                    }
                    if (publicKey == null)
                    {
                        failed.Add(file);
                        continue;
                    }
                    knownPublicKeys.AddOrReplace(publicKey);
                }
            }
            FailedFiles = failed;
        }
    }
}