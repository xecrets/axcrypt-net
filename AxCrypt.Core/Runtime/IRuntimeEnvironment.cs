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

using System;
using System.Threading;

namespace AxCrypt.Core.Runtime
{
    public interface IRuntimeEnvironment
    {
        bool IsLittleEndian { get; }

        string AxCryptExtension { get; }

        Platform Platform { get; }

        int StreamBufferSize { get; }

        ITiming StartTiming();

        bool CanTrackProcess { get; }

        string EnvironmentVariable(string name);

        int MaxConcurrency { get; }

        bool IsFirstInstanceReady(TimeSpan timeout);

        void FirstInstanceIsReady();

        bool IsFirstInstance { get; set; }

        void ExitApplication(int exitCode);

        void DebugMode(bool enable);

        SynchronizationContext SynchronizationContext { get; }

        string AppPath { get; set; }

        void RunApp(string arguments);
    }
}