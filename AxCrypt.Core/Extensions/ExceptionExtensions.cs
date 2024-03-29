﻿using AxCrypt.Abstractions;
using AxCrypt.Common;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using AxCrypt.Content;
using System;
using System.IO;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Extensions
{
    public static class ExceptionExtensions
    {
        public static bool IsFileOrDirectoryNotFound(this Exception ex)
        {
            if (ex is FileNotFoundException)
            {
                return true;
            }

            return IsDirectoryNotFound(ex);
        }

        private static bool IsDirectoryNotFound(this Exception ex)
        {
            string typeName = ex.GetType().FullName;
            if (typeName == "System.IO.DirectoryNotFoundException")
            {
                return true;
            }

            return false;
        }

        public static void ReportAndDisplay(this Exception ex)
        {
            New<IReport>().Exception(ex);
            AxCryptException aex = ex as AxCryptException;
            if (aex != null)
            {
                New<IStatusChecker>().CheckStatusAndShowMessage(aex.ErrorStatus, aex.DisplayContext, aex.Message);
                return;
            }
            New<IStatusChecker>().CheckStatusAndShowMessage(ErrorStatus.Exception, ex?.Message ?? "(null)", Texts.Exception.InvariantFormat("unknown"));
        }

        public static void RethrowFileOperation(this Exception ex, string displayContext)
        {
            if (ex == null)
            {
                throw new ArgumentNullException(nameof(ex));
            }

            AxCryptException aex = ex as AxCryptException;
            if (aex != null)
            {
                throw new FileOperationException(aex.Message, displayContext, aex.ErrorStatus, ex);
            }
            throw new FileOperationException(ex.Message, displayContext, ErrorStatus.Exception, ex);
        }

        public static async Task HandleApiExceptionAsync(this ApiException aex)
        {
            New<IReport>().Exception(aex);
            if (New<AxCryptOnlineState>().IsOffline)
            {
                return;
            }

            New<AxCryptOnlineState>().IsOffline = true;
            await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.WarningTitle, Texts.OfflineApiExceptionDialogText);
        }
    }
}