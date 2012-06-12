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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestProgressStreamTest
    {
        [Test]
        public static void TestInvalidArguments()
        {
            Stream nullStream = null;
            ProgressContext nullProgress = null;

            ProgressStream progressStream;
            Assert.Throws<ArgumentNullException>(() => { progressStream = new ProgressStream(nullStream, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { progressStream = new ProgressStream(new MemoryStream(), nullProgress); });

            progressStream = new ProgressStream(new MemoryStream(), new ProgressContext());
            byte[] nullBuffer = null;
            Assert.Throws<ArgumentNullException>(() => { progressStream.Write(nullBuffer, 0, 0); });
            Assert.Throws<ArgumentNullException>(() => { progressStream.Read(nullBuffer, 0, 0); });
        }

        [Test]
        public static void TestProperties()
        {
            using (MemoryStream memoryStream = new MemoryStream(new byte[] { 0, 1, 2, 3, 5, 6, 7, 8, 9 }))
            {
                string kilroy = String.Empty;
                using (TestStream testStream = new TestStream(memoryStream, (string wasHere) => { kilroy = wasHere; }))
                {
                    using (ProgressStream progressStream = new ProgressStream(testStream, new ProgressContext()))
                    {
                        Assert.That(progressStream.CanRead, Is.True, "The underlying stream is readable.");
                        Assert.That(kilroy, Is.EqualTo("CanRead"), "ProgressStream should delegate to the underlying stream.");
                        Assert.That(progressStream.CanWrite, Is.True, "The underlying stream is writable.");
                        Assert.That(kilroy, Is.EqualTo("CanWrite"), "ProgressStream should delegate to the underlying stream.");
                    }
                }
            }
        }
    }
}