﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.UI
{
    public interface IEmailParser
    {
        bool TryParse(string email, out string address);

        IEnumerable<string> Extract(string text);
    }
}