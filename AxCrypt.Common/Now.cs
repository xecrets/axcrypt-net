﻿using AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Common
{
    public class Now : INow
    {
        public DateTime Utc
        {
            get
            {
                return DateTime.UtcNow;
            }
        }
    }
}
