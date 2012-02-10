//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using Microsoft.Samples.XFileExplorer;

namespace Microsoft.Samples.XFileExplorerFavorites
{
    [Export(typeof(IFavoriteItem))]
    [PartNotDiscoverable]
    public class FavoriteDirectoryItem : IFavoriteItem
    {
        [Import]
        public IIconReaderService IconReader { get; set; }

        [Import]
        public INavigationService NavigationService { get; set; }

        private ImageSource icon;
        private string directoryPath;

        public FavoriteDirectoryItem(string directoryPath)
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException("directoryPath");
            }
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException(directoryPath);
            }
            this.directoryPath = directoryPath;
            ItemName = Path.GetFileName(directoryPath);
        }

        public ImageSource Icon
        {
            get
            {
                if (icon == null)
                {
                    icon = IconReader.GetFolderIconImage(false, false);
                }
                return icon;
            }
        }
        public string ItemName { get; private set; }
        public Action Command
        {
            get
            {
                return () =>
                {
                    Debug.Assert(NavigationService != null);
                    NavigationService.CurrentPath = directoryPath;
                };
            }
        }
    }
}
