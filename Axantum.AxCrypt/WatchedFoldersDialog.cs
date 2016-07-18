﻿using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Forms;
using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt
{
    public partial class WatchedFoldersDialog : StyledMessageBase
    {
        private WatchedFoldersViewModel _viewModel;

        private IEnumerable<string> _additional;

        public WatchedFoldersDialog()
        {
            InitializeComponent();
        }

        public WatchedFoldersDialog(Form parent, IEnumerable<string> additional)
            : this()
        {
            InitializeStyle(parent);

            _additional = additional;
            _viewModel = New<WatchedFoldersViewModel>();
        }

        protected override void InitializeContentResources()
        {
            Text = Texts.DialogWatchedFoldersTitle;

            _watchedFolderColumnHeader.Text = Texts.ColumnFolderHeader;
            _watchedFoldersRemoveMenuItem.Text = "&" + Texts.MenuDecryptPermanentlyText;
            _watchedFoldersdecryptTemporarilyMenuItem.Text = "&" + Texts.MenuDecryptTemporarilyText;
            _watchedFoldersOpenExplorerHereMenuItem.Text = "&" + Texts.MenuOpenExplorerHereText;
        }

        private void WatchedFoldersDialog_Load(object sender, EventArgs e)
        {
            BindToViewModel();
            _viewModel.AddWatchedFolders.Execute(_additional);
            _additional = new string[0];
        }

        private void BindToViewModel()
        {
            _viewModel.BindPropertyChanged(nameof(WatchedFoldersViewModel.WatchedFolders), (IEnumerable<string> folders) => { UpdateWatchedFolders(folders); });

            _watchedFoldersListView.SelectedIndexChanged += (sender, e) => { _viewModel.SelectedWatchedFolders = _watchedFoldersListView.SelectedItems.Cast<ListViewItem>().Select(lvi => lvi.Text); };
            _watchedFoldersListView.MouseClick += (sender, e) => { if (e.Button == MouseButtons.Right) _watchedFoldersContextMenuStrip.Show((Control)sender, e.Location); };
            _watchedFoldersListView.DragOver += (sender, e) => { _viewModel.DragAndDropFiles = e.GetDragged(); e.Effect = GetEffectsForWatchedFolders(e); };
            _watchedFoldersListView.DragDrop += (sender, e) => { _viewModel.AddWatchedFolders.Execute(_viewModel.DragAndDropFiles); };
            _watchedFoldersOpenExplorerHereMenuItem.Click += (sender, e) => { _viewModel.OpenSelectedFolder.Execute(_viewModel.SelectedWatchedFolders.First()); };
            _watchedFoldersRemoveMenuItem.Click += (sender, e) => { _viewModel.RemoveWatchedFolders.Execute(_viewModel.SelectedWatchedFolders); };
        }

        private void UpdateWatchedFolders(IEnumerable<string> watchedFolders)
        {
            _watchedFoldersListView.BeginUpdate();
            try
            {
                _watchedFoldersListView.Items.Clear();
                foreach (string folder in watchedFolders)
                {
                    ListViewItem item = _watchedFoldersListView.Items.Add(folder);
                    item.Name = folder;
                }
            }
            finally
            {
                _watchedFoldersListView.EndUpdate();
            }
        }

        public DragDropEffects GetEffectsForWatchedFolders(DragEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (!_viewModel.DroppableAsWatchedFolder)
            {
                return DragDropEffects.None;
            }
            return (DragDropEffects.Link | DragDropEffects.Copy) & e.AllowedEffect;
        }
    }
}