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

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Header;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Linq;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestV2HmacHeaderBlock
    {
        [Test]
        public static void TestBadConstructorArgument()
        {
            V2HmacHeaderBlock headerBlock = null;
            Assert.Throws<ArgumentNullException>(() => headerBlock = new V2HmacHeaderBlock(null));
            Assert.That(headerBlock, Is.Null);
        }

        [Test]
        public static void TestClone()
        {
            TypeMap.Register.Singleton<IRandomGenerator>(() => new FakeRandomGenerator());

            V2HmacHeaderBlock headerBlock = new V2HmacHeaderBlock();
            headerBlock.Hmac = new V2Hmac(Resolve.RandomGenerator.Generate(V2Hmac.RequiredLength));

            V2HmacHeaderBlock clone = (V2HmacHeaderBlock)headerBlock.Clone();

            Assert.That(clone.GetDataBlockBytes(), Is.EquivalentTo(headerBlock.GetDataBlockBytes()));
        }
    }
}