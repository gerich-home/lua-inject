//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Samples.XFileExplorer
{
    /// <summary>
    /// Interaction logic for StatusViewItemCount.xaml
    /// </summary>
    [Export("Microsoft.Samples.XFileExplorer.StatusServiceContract", typeof(UserControl))]
    [ExportMetadata("Index", 0)]
    public partial class StatusViewItemCount : UserControl
    {
        [Import]
        public INavigationService Navigation = null;

        public StatusViewItemCount()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Navigation.CurrentPathChanged += new CurrentPathChangedHandler(NavigationService_CurrentPathChanged);
            Navigation.SelectedItemChanged += new SelectedItemChangedHandler(NavigationService_SelectedItemChanged);

            NavigationService_CurrentPathChanged();
            NavigationService_SelectedItemChanged();
        }

        private void NavigationService_CurrentPathChanged()
        {
            int itemsCount = 0;

            try
            {
                int foldersCount = Directory.GetDirectories(Navigation.CurrentPath).Length;
                int filesCount = Directory.GetFiles(Navigation.CurrentPath).Length;
                itemsCount = foldersCount + filesCount;
            }
            catch (UnauthorizedAccessException)
            {
                // UnauthorizedAccessException could be thrown if the path is restricted for access
            }

            ItemCountLabel.Content = itemsCount.ToString() + ((itemsCount > 1) ? " itmes" : " item");
        }

        private void NavigationService_SelectedItemChanged()
        {
            if (Navigation.SelectedItem != "")
                ItemCountLabel.Content = "";
        }
    }
}
