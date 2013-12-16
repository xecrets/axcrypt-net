﻿#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    /// <summary>
    /// Not using SetUpFixtureAttribute etc because MonoDevelop does not always honor.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "NUnit requires there to be a parameterless constructor.")]
    internal static class SetupAssembly
    {
        public static void AssemblySetup()
        {
            FactoryRegistry.Instance.Singleton<IRuntimeEnvironment>(() => new FakeRuntimeEnvironment());
            FactoryRegistry.Instance.Singleton<ILogging>(() => new FakeLogging());
            FactoryRegistry.Instance.Singleton<IUserSettings>(() => new UserSettings(UserSettings.DefaultPathInfo));
            FactoryRegistry.Instance.Singleton(() => new KnownKeys());
            FactoryRegistry.Instance.Singleton(() => new ProcessState());
            FactoryRegistry.Instance.Singleton<ISleep>(() => new FakeSleep());
            FactoryRegistry.Instance.Singleton<IUIThread>(() => new FakeUIThread());
            FactoryRegistry.Instance.Singleton<IProgressBackground>(() => new FakeProgressBackground());

            FactoryRegistry.Instance.Register<AxCryptFile>(() => new AxCryptFile());
            FactoryRegistry.Instance.Register<FileSystemState, FileSystemStateActions>((fileSystemState) => new FileSystemStateActions(fileSystemState));
            FactoryRegistry.Instance.Register<IDelayTimer>(() => new FakeDelayTimer());

            Instance.UserSettings.KeyWrapIterations = 1234;
            Instance.UserSettings.ThumbprintSalt = KeyWrapSalt.Zero;
            Instance.Log.SetLevel(LogLevel.Debug);
        }

        public static void AssemblyTeardown()
        {
            FakeRuntimeFileInfo.ClearFiles();
            FactoryRegistry.Instance.Clear();
        }

        internal static FakeRuntimeEnvironment FakeRuntimeEnvironment
        {
            get
            {
                return (FakeRuntimeEnvironment)OS.Current;
            }
        }
    }
}