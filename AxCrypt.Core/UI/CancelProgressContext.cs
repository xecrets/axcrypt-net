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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AxCrypt.Core.UI
{
    public class CancelProgressContext : IProgressContext
    {
        private IProgressContext _progress;

        private long _totalCount;

        private long _currentCount;

        private bool _canceledExceptionThrown;

        public CancelProgressContext(IProgressContext progress)
        {
            _progress = progress;
        }

        public void AddCount(long count)
        {
            Interlocked.Add(ref _currentCount, count);
            _progress.AddCount(count);

            ThrowIfCancelled();
        }

        private void ThrowIfCancelled()
        {
            if (!Cancel || _canceledExceptionThrown)
            {
                return;
            }

            _progress.AddCount(_totalCount - _currentCount);
            _currentCount = _totalCount;
            _canceledExceptionThrown = true;
            throw new OperationCanceledException("Operation canceled on request.");
        }

        public void NotifyLevelStart()
        {
            ThrowIfCancelled();
            _progress.NotifyLevelStart();
        }

        public void NotifyLevelFinished()
        {
            ThrowIfCancelled();
            _progress.NotifyLevelFinished();
        }

        public bool Cancel
        {
            get
            {
                return _progress.Cancel;
            }
            set
            {
                _progress.Cancel = value;
            }
        }

        public void RemoveCount(long totalCount, long progressCount)
        {
            _progress.RemoveCount(totalCount, progressCount);
        }

        public void AddTotal(long count)
        {
            ThrowIfCancelled();

            Interlocked.Add(ref _totalCount, count);
            _progress.AddTotal(count);
        }

        public bool AllItemsConfirmed
        {
            get
            {
                return _progress.AllItemsConfirmed;
            }
            set
            {
                _progress.AllItemsConfirmed = value;
            }
        }

        public ProgressTotals Totals
        {
            get
            {
                return _progress.Totals;
            }
        }

        public event EventHandler<ProgressEventArgs> Progressing
        {
            add
            {
                _progress.Progressing += value;
            }
            remove
            {
                _progress.Progressing -= value;
            }
        }

        public Task EnterSingleThread()
        {
            return _progress.EnterSingleThread();
        }

        public void LeaveSingleThread()
        {
            _progress.LeaveSingleThread();
        }

        public string Display
        {
            get
            {
                return _progress.Display;
            }

            set
            {
                _progress.Display = value;
            }
        }
    }
}