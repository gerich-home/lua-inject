//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Meflook;

namespace MeflookSample
{
    /// <summary>
    /// Interaction logic for MeflookShell.xaml
    /// </summary>
    [Export("MainWindow", typeof(Window))]
    public partial class MeflookShell : System.Windows.Window
    {
        [ImportMany("MeflookView")]
        private Lazy<UserControl, IShellViewMetadata>[] views = null;

        public MeflookShell()
        {   
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            foreach (var view in views.OrderBy(i => i.Metadata.Index))
            {
                dockPanel.Children.Add(view.Value);
            }
        }
    }
}
