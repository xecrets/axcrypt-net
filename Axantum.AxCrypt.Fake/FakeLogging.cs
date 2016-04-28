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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Fake
{
    public class FakeLogging : ILogging
    {
        #region ILogging Members

        public event EventHandler<LoggingEventArgs> Logged;

        protected virtual void OnLogged(string message)
        {
            EventHandler<LoggingEventArgs> handler = Logged;
            if (handler != null)
            {
                handler(this, new LoggingEventArgs(message));
            }
        }

        public void SetLevel(LogLevel level)
        {
        }

        public bool IsFatalEnabled
        {
            get { return true; }
        }

        public bool IsErrorEnabled
        {
            get { return true; }
        }

        public bool IsWarningEnabled
        {
            get { return true; }
        }

        public bool IsInfoEnabled
        {
            get { return true; }
        }

        public bool IsDebugEnabled
        {
            get { return true; }
        }

        public void LogFatal(string message)
        {
            OnLogged(message);
        }

        public void LogError(string message)
        {
            OnLogged(message);
        }

        public void LogWarning(string message)
        {
            OnLogged(message);
        }

        public void LogInfo(string message)
        {
            OnLogged(message);
        }

        public void LogDebug(string message)
        {
            OnLogged(message);
        }

        #endregion ILogging Members

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}