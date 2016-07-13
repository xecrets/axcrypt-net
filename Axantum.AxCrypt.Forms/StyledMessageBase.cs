﻿using Axantum.AxCrypt.Forms.Properties;
using Axantum.AxCrypt.Forms.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Forms
{
    public class StyledMessageBase : Form
    {
        public StyledMessageBase()
        {
            Load += (sender, e) => InitializeContentResources();
        }

        protected void InitializeStyle(Form owner)
        {
            new Styling(Resources.axcrypticon).Style(this);
            Owner = owner;
            StartPosition = FormStartPosition.CenterParent;

            Shown += StyledMessageBase_Shown;
            Move += StyledMessageBase_Move;
        }

        protected virtual void InitializeContentResources()
        {
        }

        private Point? _lastLocation;

        private void StyledMessageBase_Shown(object sender, EventArgs e)
        {
            _lastLocation = Location;
        }

        private void StyledMessageBase_Move(object sender, EventArgs e)
        {
            if (_lastLocation == null)
            {
                return;
            }
            Owner.Location = new Point(Owner.Location.X - (_lastLocation.Value.X - Location.X), Owner.Location.Y - (_lastLocation.Value.Y - Location.Y));
            _lastLocation = Location;
        }
    }
}