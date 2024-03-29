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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Core.Runtime;
using System;
using System.Windows.Forms;

namespace AxCrypt.Desktop.Window
{
    public class UIThread : UIThreadBase
    {
        private Control _control;

        public UIThread(Control control) : base()
        {
            _control = control;
        }

        public override bool IsOn
        {
            get { return !_control.InvokeRequired; }
        }

        public override void Yield()
        {
            Application.DoEvents();
        }

        public override void ExitApplication()
        {
            Application.Exit();
        }

        public override void RestartApplication()
        {
            Application.Restart();
        }
    }
}