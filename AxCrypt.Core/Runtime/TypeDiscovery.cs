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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Runtime
{
    public static class TypeDiscovery
    {
        public static IEnumerable<Type> Interface(Type interfaceToDiscover, IEnumerable<Assembly> extraAssemblies)
        {
            if (interfaceToDiscover == null)
            {
                throw new ArgumentNullException("interfaceToDiscover");
            }

            List<Type> interfaces = new List<Type>();
            foreach (Assembly assembly in new Assembly[] { interfaceToDiscover.GetTypeInfo().Assembly }.Concat(extraAssemblies))
            {
                try
                {
                    ScanAssemblyForNewInterfaces(interfaceToDiscover, assembly, interfaces);
                }
                catch (TypeLoadException tlex)
                {
                    New<IReport>().Exception(tlex);
                }
            }
            return interfaces;
        }

        private static void ScanAssemblyForNewInterfaces(Type interfaceToDiscover, Assembly assembly, IList<Type> interfaces)
        {
            IEnumerable<Type> types = from t in assembly.ExportedTypes where t.GetTypeInfo().ImplementedInterfaces.Contains(interfaceToDiscover) select t;

            foreach (Type t in types)
            {
                if (!interfaces.Any(i => i.FullName == t.FullName))
                {
                    interfaces.Add(t);
                }
            }
        }
    }
}