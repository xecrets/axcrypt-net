﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.IO
{
    public class WebCallerRequest
    {
        public WebContent Content { get; private set; }

        public WebHeaders Headers { get; private set; }

        public TimeSpan TimeOut { get; private set; }

        public string Method { get; private set; }

        public Uri Url { get; private set; }

        public WebCallerRequest()
        {
            Content = new WebContent();
            Headers = new WebHeaders();
            TimeOut = TimeSpan.FromSeconds(1);
        }

        public WebCallerRequest(string method, Uri url)
            : this()
        {
            Method = method;
            Url = url;
        }

        public WebCallerRequest(Uri url) : this("GET", url)
        {
        }
    }
}