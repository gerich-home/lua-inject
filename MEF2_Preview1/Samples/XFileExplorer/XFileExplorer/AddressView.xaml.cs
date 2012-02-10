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
    /// Interaction logic for AddressView.xaml
    /// </summary>
    [Export("Microsoft.Samples.XFileExplorer.FileExplorerViewContract", typeof(UserControl))]
    [FileExplorerViewMetadata(Name = "Address Pane", Docking = Dock.Top)]
    public partial class AddressView : UserControl
    {
        [Import]
        public INavigationService Navigation = null;

        public AddressView()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Navigation.CurrentPathChanged += new CurrentPathChangedHandler(NavigationService_CurrentPathChanged);

            NavigationService_CurrentPathChanged();
        }

        private void NavigationService_CurrentPathChanged()
        {
            AddressBox.Text = Navigation.CurrentPath;
        }

        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            Navigation.CurrentPath = AddressBox.Text;

            if (Directory.Exists(AddressBox.Text))
            {
                bool bFound = false;
                foreach (string item in AddressBox.Items)
                {
                    if (item.ToString().ToLower() == AddressBox.Text.ToLower()) bFound = true;
                }
                if (!bFound) AddressBox.Items.Add(AddressBox.Text);
            }
        }

        private void Address_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AddressBox.SelectedItem != null)
            {
                AddressBox.Text = AddressBox.SelectedItem.ToString();
                Enter_Click(this, null);
            }
        }
    }
}
