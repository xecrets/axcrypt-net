﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Abstractions
{
    public static class Extensions
    {
        /// <summary>
        /// Extension for String.Format using InvariantCulture
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string Format(this string pattern, params object[] parameters)
        {
            string formatted = String.Format(CultureInfo.InvariantCulture, pattern, parameters);
            return formatted;
        }
    }
}