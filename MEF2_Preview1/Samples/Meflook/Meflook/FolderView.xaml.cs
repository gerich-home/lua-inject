//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Meflook;

namespace MeflookSample
{
    /// <summary>
    /// Interaction logic for FolderTree.xaml
    /// </summary>
    [Export(typeof(FolderView))]
    public partial class FolderView : System.Windows.Controls.UserControl
    {
        [Import(typeof(IEmailService))]
        private IEmailService emailService = null;

        [Import(typeof(ISelectionService))]
        private ISelectionService selectionService = null;

        public FolderView()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                folderGrid.DataContext = emailService.GetFoldersDataProvider();
                TreeViewItem item = this.folderTree.ItemContainerGenerator.ContainerFromItem(this.folderTree.Items[0]) as TreeViewItem;
                item.IsExpanded = true;
                item.IsSelected = true;
            }
            catch { }
        }

        private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if ((e.NewValue as XmlElement) != null)
                selectionService.CurrentFolder = (e.NewValue as XmlElement)["Name"].InnerText;
            else
            {
                try
                {
                    IInputElement ie = Mouse.DirectlyOver;
                    if (ie == null)
                        return;
                    selectionService.CurrentFolder = (((ie as FrameworkElement).TemplatedParent as TreeViewItem).Header as XmlElement)["Name"].InnerText;
                }
                catch { }
            }

        }

    }
}
