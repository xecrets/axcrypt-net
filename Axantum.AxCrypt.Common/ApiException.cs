﻿#region Coypright and License

/*
 * AxCrypt - Copyright 2015, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Abstractions;
using System;
using System.Net;

namespace Axantum.AxCrypt.Common
{
    public class ApiException : AxCryptException
    {
        public HttpStatusCode HttpStatusCode { get; private set; }

        public ApiException()
            : base()
        {
            HttpStatusCode = HttpStatusCode.OK;
        }

        public ApiException(string message)
            : this(message, ErrorStatus.ApiError)
        {
        }

        public ApiException(string message, ErrorStatus errorStatus)
            : base(message, errorStatus)
        {
            HttpStatusCode = HttpStatusCode.OK;
        }

        public ApiException(string message, HttpStatusCode httpStatusCode)
            : this(message, ErrorStatus.ApiHttpResponseError)
        {
            HttpStatusCode = httpStatusCode;
        }

        public ApiException(string message, Exception innerException)
            : this(message, ErrorStatus.CryptographicError, innerException)
        {
        }

        public ApiException(string message, ErrorStatus errorStatus, Exception innerException)
            : base(message, errorStatus, innerException)
        {
            HttpStatusCode = HttpStatusCode.OK;
        }
    }
}