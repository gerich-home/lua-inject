//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Meflook;

namespace MeflookSample 
{
    /// <summary>
    /// Interaction logic for LeftSpine.xaml
    /// </summary>
    [Export("MeflookView", typeof(UserControl)), ShellViewMetadata(1)]
    public partial class LeftSpine : System.Windows.Controls.UserControl
    {
        [Import(typeof(FolderView))] 
        private FolderView fv = null;

        public LeftSpine()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            dockPanel.Children.Add(fv);
        }

        public UserControl View
        {
            get { return this; }
        }

    }
}
