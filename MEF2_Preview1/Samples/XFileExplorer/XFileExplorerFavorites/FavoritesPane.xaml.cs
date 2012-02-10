//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Samples.XFileExplorer;

namespace Microsoft.Samples.XFileExplorerFavorites
{
    [Export("Microsoft.Samples.XFileExplorer.FileExplorerViewContract", typeof(UserControl))]
    [ExportMetadata("Name", "Favorites Pane")]
    [ExportMetadata("Docking", Dock.Bottom)]
    public partial class FavoritesPane : UserControl
    {
        [ImportMany(typeof(IFavoriteItem), AllowRecomposition=true)]
        public ObservableCollection<IFavoriteItem> Favorites { get; set; }

        public FavoritesPane()
        {
            InitializeComponent();
            Favorites = new ObservableCollection<IFavoriteItem>();
            listBox.ItemsSource = Favorites;
        }

        private void FavoriteItemClicked(object sender, RoutedEventArgs e)
        {
            IFavoriteItem favItem = ((Button)sender).DataContext as IFavoriteItem;
            if (favItem.Command != null)
            {
                favItem.Command();
            }
        }
    }
}
