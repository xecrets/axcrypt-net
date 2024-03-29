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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Core.Session;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestWatchedFolderChangedEventArgsTest
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestConstructor()
        {
            using (WatchedFolder folder1 = new WatchedFolder(@"C:\Folder1\", IdentityPublicTag.Empty))
            {
                using (WatchedFolder folder2 = new WatchedFolder(@"C:\Folder2\", IdentityPublicTag.Empty))
                {
                    List<WatchedFolder> addedFolders = new List<WatchedFolder>();
                    addedFolders.Add(folder1);
                    List<WatchedFolder> removedFolders = new List<WatchedFolder>();
                    removedFolders.Add(folder2);

                    WatchedFolderChangedEventArgs e = new WatchedFolderChangedEventArgs(addedFolders, removedFolders);

                    List<WatchedFolder> eventArgsAddedFolders = new List<WatchedFolder>(e.Added);
                    List<WatchedFolder> eventArgsRemovedFolders = new List<WatchedFolder>(e.Removed);

                    Assert.That(eventArgsAddedFolders, Is.EquivalentTo(addedFolders));
                    Assert.That(eventArgsRemovedFolders, Is.EquivalentTo(removedFolders));
                }
            }
        }
    }
}