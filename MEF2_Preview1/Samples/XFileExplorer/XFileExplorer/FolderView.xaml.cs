//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Microsoft.Samples.XFileExplorer
{
    /// <summary>
    /// Interaction logic for FolderView.xaml
    /// </summary>
    [Export("Microsoft.Samples.XFileExplorer.FileExplorerViewContract", typeof(UserControl))]
    [FileExplorerViewMetadata(Name="Folder Pane", Docking=Dock.Left)]
    public partial class FolderView : UserControl
    {
        [Import]
        public INavigationService Navigation { get; set; }

        [Import]
        public IIconReaderService IconReaderService 
        {
            set
            {
                TreeViewItemToImageConverter.Instance.IconReaderService = value;
            }
        }

        private bool _bPathChangedByUI = false;

        public FolderView()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Navigation.CurrentPathChanged += new CurrentPathChangedHandler(NavigationService_CurrentPathChanged);

            object dummyNode = null;
            foreach (string drive in Directory.GetLogicalDrives())
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = drive;
                item.Tag = drive;
                item.FontWeight = FontWeights.Normal;
                item.Items.Add(dummyNode);
                item.Expanded += new RoutedEventHandler(Folder_Expanded);
                FolderTree.Items.Add(item);
            }

            SetPath(Environment.ExpandEnvironmentVariables("%SystemDrive%"));
        }
        
        private void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            object dummyNode = null;
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
                try
                {
                    foreach (string folder in Directory.GetDirectories(item.Tag.ToString()))
                    {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = folder.Substring(folder.LastIndexOf("\\") + 1);
                        subitem.Tag = folder;
                        subitem.FontWeight = FontWeights.Normal;
                        subitem.Items.Add(dummyNode);
                        subitem.Expanded += new RoutedEventHandler(Folder_Expanded);
                        item.Items.Add(subitem);
                    }
                }
                catch (UnauthorizedAccessException uae)
                {
                    MessageBox.Show(uae.Message);
                }
            }
        }

        private void FolderTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _bPathChangedByUI = true;

            TreeView tree = (TreeView)sender;
            TreeViewItem item = (TreeViewItem)tree.SelectedItem;
            Navigation.CurrentPath = item.Tag.ToString();
        }

        private void NavigationService_CurrentPathChanged()
        {
            if (!_bPathChangedByUI)
                SetPath(Navigation.CurrentPath);
            _bPathChangedByUI = false;
        }

        /// <summary>
        /// Expand and select the specified path in the folder tree
        /// </summary>
        /// <param name="path">The path to be expanded</param>
        private void SetPath(string path)
        {
            if (!Directory.Exists(path)) return;
            if (FolderTree.Items.Count == 0) return;
            
            ItemCollection items = FolderTree.Items;
            string fullPath = path.ToLower();
            fullPath = fullPath.Replace('/', '\\');
            fullPath = fullPath.TrimEnd(new char[] { '\\' });
            string[] folders = fullPath.Split(new char[] { '\\' });
            if (folders[0].EndsWith(":")) folders[0] += '\\';

            TreeViewItem folderItem = null;
            foreach (string folder in folders)
            {
                foreach (var item in items)
                {
                    folderItem = item as TreeViewItem;
                    if (folder == folderItem.Header.ToString().ToLower())
                    {
                        folderItem.IsExpanded = true;
                        items = folderItem.Items;
                        break;
                    }
                }
            }
            folderItem.IsSelected = true;
        }
    }

    #region TreeViewItemToImageConverter

    [ValueConversion(typeof(TreeViewItem), typeof(ImageSource))]
    public class TreeViewItemToImageConverter : IValueConverter
    {
        public static TreeViewItemToImageConverter Instance = new TreeViewItemToImageConverter();

        public IIconReaderService IconReaderService { set; get; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(IconReaderService != null);

            TreeViewItem item = value as TreeViewItem;
            string path = item.Tag as string;
            return IconReaderService.GetFolderIconImage(path, !item.IsExpanded, true);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    #endregion
}
