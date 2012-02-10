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
    /// Interaction logic for StatusViewFilesSize.xaml
    /// </summary>
    [Export("Microsoft.Samples.XFileExplorer.StatusServiceContract", typeof(UserControl))]
    [ExportMetadata("Index", 1)]
    public partial class StatusViewFilesSize : UserControl
    {
        [Import]
        public INavigationService Navigation = null;

        public StatusViewFilesSize()
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
            long totalFileSize = 0;

            try
            {
                DirectoryInfo di = new DirectoryInfo(Navigation.CurrentPath);
                FileInfo[] fis = di.GetFiles();
                foreach (var file in fis)
                {
                    totalFileSize += file.Length;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // UnauthorizedAccessException could be thrown is the path is restricted for access
            }
  
            FilesSizeLabel.Content = totalFileSize.ToString("n0") + " bytes";
        }

        private void NavigationService_SelectedItemChanged()
        {
            if (Navigation.IsFolder() && Navigation.SelectedItem != "")
            {
                FilesSizeLabel.Content = "";
            }
            else
            {
                FileInfo fi = new FileInfo(System.IO.Path.Combine(Navigation.CurrentPath, Navigation.SelectedItem));
                if (fi.Exists)
                {
                    FilesSizeLabel.Content = fi.Length.ToString("n0") + " bytes";
                }
            }
        }
    }
}
