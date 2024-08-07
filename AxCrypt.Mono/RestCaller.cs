﻿using System.Collections.Generic;
using System.Linq;

#region Coypright and License

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
using AxCrypt.Abstractions.Rest;
using AxCrypt.Common;
using AxCrypt.Core.Extensions;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Mono
{
    internal class RestCaller : IRestCaller
    {
        public RestCaller()
        {
        }

        #region IRestCaller Members

        public async Task<RestResponse> SendAsync(RestIdentity identity, RestRequest request)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (!New<IInternetState>().Connected)
            {
                throw new OfflineApiException(ExceptionMessage("Internet Unavailable", request));
            }

            try
            {
                switch (request.Method)
                {
                    case "GET":
                        if (request.Content.Text.Length > 0)
                        {
                            throw new ArgumentException("You can't send content with a GET request.", "request");
                        }
                        return await SendGetAsync(identity, request).Free();

                    case "PUT":
                        return await SendPutAsync(identity, request).Free();

                    case "POST":
                        return await SendPostAsync(identity, request).Free();

                    case "DELETE":
                        return await SendDeleteAsync(identity, request).Free();

                    default:
                        throw new NotSupportedException("The method '{0}' is not supported.".InvariantFormat(request.Method));
                }
            }
            catch (TaskCanceledException tcex)
            {
                throw new OfflineApiException(ExceptionMessage("Offline", request), tcex);
            }
            catch (WebException wex)
            {
                throw new OfflineApiException(ExceptionMessage("Offline", request), wex);
            }
            catch (HttpRequestException hrex)
            {
                New<IInternetState>().Clear();
                throw new OfflineApiException(ExceptionMessage("Offline", request), hrex);
            }
        }

        private static string ExceptionMessage(string message, RestRequest request)
        {
            return string.Format(CultureInfo.InvariantCulture, "{2} {1} {0}", request.Url, request.Method, message);
        }

        public string HtmlEncode(string value)
        {
            return WebUtility.HtmlEncode(value);
        }

        public string UrlEncode(string value)
        {
            return WebUtility.UrlEncode(value);
        }

        #endregion IRestCaller Members

        private static async Task<RestResponse> SendGetAsync(RestIdentity identity, RestRequest request)
        {
            string content = String.Empty;
            using (HttpClient client = CreateHttpClient())
            {
                PrepareClient(client, identity, request);

                HttpResponseMessage httpResponse = await client.GetAsync(request.Url.PathAndQuery).Free();
                content = await httpResponse.Content.ReadAsStringAsync().Free();
                if (content.Length > 0 && httpResponse.Content?.Headers?.ContentType?.MediaType != "application/json")
                {
                    throw new HttpRequestException("Expected Media Type 'application/json'.");
                }

                return new RestResponse(httpResponse.StatusCode, content);
            }
        }

        private static async Task<RestResponse> SendPutAsync(RestIdentity identity, RestRequest request)
        {
            string content = String.Empty;
            using (HttpClient client = CreateHttpClient())
            {
                PrepareClient(client, identity, request);

                StringContent httpContent = new StringContent(request.Content.Text, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = await client.PutAsync(request.Url.PathAndQuery, httpContent).Free();
                content = await httpResponse.Content.ReadAsStringAsync().Free();

                return new RestResponse(httpResponse.StatusCode, content);
            }
        }

        private static async Task<RestResponse> SendPostAsync(RestIdentity identity, RestRequest request)
        {
            string content = String.Empty;
            using (HttpClient client = CreateHttpClient())
            {
                PrepareClient(client, identity, request);

                StringContent httpContent = new StringContent(request.Content.Text, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = await client.PostAsync(request.Url.PathAndQuery, httpContent).Free();
                content = await httpResponse.Content.ReadAsStringAsync().Free();

                return new RestResponse(httpResponse.StatusCode, content);
            }
        }

        private static async Task<RestResponse> SendDeleteAsync(RestIdentity identity, RestRequest request)
        {
            string content = String.Empty;
            using (HttpClient client = CreateHttpClient())
            {
                PrepareClient(client, identity, request);

                HttpResponseMessage httpResponse = await client.DeleteAsync(request.Url.PathAndQuery).Free();
                content = await httpResponse.Content.ReadAsStringAsync().Free();

                return new RestResponse(httpResponse.StatusCode, content);
            }
        }

        private static void PrepareClient(HttpClient client, RestIdentity identity, RestRequest request)
        {
            client.BaseAddress = new Uri(request.Url.GetLeftPart(UriPartial.Authority));
            client.Timeout = request.Timeout > TimeSpan.Zero ? request.Timeout : TimeSpan.FromMilliseconds(-1);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            foreach (string key in request.Headers.Collection.Keys)
            {
                client.DefaultRequestHeaders.Add(key, request.Headers.Collection[key]);
            }

            if (identity.User.Length > 0)
            {
                string credentials = "{0}:{1}".InvariantFormat(identity.User, identity.Password.ToUtf8Base64());
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials)));
            }
        }

        private static HttpClient CreateHttpClient()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.Proxy = WebRequest.DefaultWebProxy;
            clientHandler.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
#if DEBUG
            clientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
#endif
            return new HttpClient(clientHandler);
        }
    }
}