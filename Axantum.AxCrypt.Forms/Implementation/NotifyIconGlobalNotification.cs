﻿using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Forms.Implementation
{
    public class NotifyIconGlobalNotification : IGlobalNotification
    {
        private NotifyIcon _notifyIcon;

        public NotifyIconGlobalNotification(NotifyIcon notifyIcon)
        {
            _notifyIcon = notifyIcon ?? throw new ArgumentNullException(nameof(notifyIcon));
        }

        public void ShowTransient(string title, string text)
        {
            _notifyIcon.ShowBalloonTip(50000, title, text, ToolTipIcon.Info);
        }
    }
}