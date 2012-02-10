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
    public class FavoriteFileItem : IFavoriteItem
    {
        [Import]
        public IIconReaderService IconReader { get; set; }

        [Import]
        public INavigationService NavigationService { get; set; }

        private ImageSource icon;
        private string filePath;

        public FavoriteFileItem(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }
            this.filePath = filePath;
            ItemName = Path.GetFileName(filePath);
        }

        public ImageSource Icon
        {
            get
            {
                if (icon == null)
                {
                    icon = IconReader.GetFileIconImage(filePath, false);
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
                    NavigationService.CurrentPath = Path.GetDirectoryName(filePath);
                    NavigationService.SelectedItem = ItemName;
                };
            }
        }
    }
}
