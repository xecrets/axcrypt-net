﻿#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Cache;

namespace Axantum.AxCrypt.Mono
{
    internal class WebCaller : IWebCaller
    {
        public WebCaller()
        {
        }

        #region IWebCaller Members

        public WebAnswer Send(string method, Uri url, LogOnIdentity identity, WebContent content, WebHeaders headers)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }

            switch (method)
            {
                case "GET":
                    if (content == WebContent.Empty)
                    {
                        throw new ArgumentException("You can't send content with a GET request.", "content");
                    }
                    return SendGet(url, headers);

                default:
                    throw new NotSupportedException("The method '{0}' is not supported.".InvariantFormat(method));
            }
        }

        private static WebAnswer SendGet(Uri url, WebHeaders headers)
        {
            string content = String.Empty;
            using (WebClient client = new WebClient())
            {
                client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                content = client.DownloadString(url);
            }
            return new WebAnswer(HttpStatusCode.OK, content);
        }

        #endregion IWebCaller Members
    }
}