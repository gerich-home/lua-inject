//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Samples.XFileExplorer
{
    /// <summary>
    /// Interaction logic for ContentView.xaml
    /// </summary>
    [Export("Microsoft.Samples.XFileExplorer.FileExplorerViewContract", typeof(UserControl))]
    [FileExplorerViewMetadata(Name = "Contents Pane", Docking = Dock.Right, DockId = 0)]
    public partial class ContentView : UserControl, IPartImportsSatisfiedNotification
    {
        [Import]
        public INavigationService Navigation { get; set; }

        [ImportMany("Microsoft.Samples.XFileExplorer.CustomMenuItemContract", AllowRecomposition=true)]
        public IEnumerable<MenuItem> CustomMenuItems { get; set; }

        [Import]
        public IIconReaderService IconReaderService { get; set; }

        private bool _initialized = false;
        FileSystemWatcher _watcher = null;
        SynchronizationContext syncContext = null;
        private bool isCopy = true;

        public ContentView()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_initialized)
            {
                syncContext = SynchronizationContext.Current;
                Navigation.CurrentPathChanged += new CurrentPathChangedHandler(NavigationService_CurrentPathChanged);
                NavigationService_CurrentPathChanged();
                _initialized = true;
            }
        }

        private void NavigationService_CurrentPathChanged()
        {
            if (Directory.Exists(Navigation.CurrentPath))
            {
                try
                {
                    _watcher = new FileSystemWatcher(Navigation.CurrentPath);
                    _watcher.Changed += new FileSystemEventHandler(UpdateContentView);
                    _watcher.Created += new FileSystemEventHandler(UpdateContentView);
                    _watcher.Deleted += new FileSystemEventHandler(UpdateContentView);
                    _watcher.Renamed += new RenamedEventHandler(UpdateContentView);
                    _watcher.EnableRaisingEvents = true;
                }
                catch (FileNotFoundException)
                {
                    // FileNotFoundException could be thrown if the path is restricted for access
                }
                finally
                {
                    UpdateContentView(this, null);
                }
            }
        }

        private void UpdateContentView(object sender, FileSystemEventArgs e)
        {
            // This has to be done on UI thread.
            syncContext.Send(
                delegate
                {
                    ContentGrid.DataContext = CreateContentDataTable(Navigation.CurrentPath);

                    //Scroll to the first item. Otherwise, the previous scroll will affect current view.
                    if (ContentGrid.Items.Count > 0)
                        ContentGrid.ScrollIntoView(ContentGrid.Items[0]);
                }, null
            );
        }

        private DataTable CreateContentDataTable(string CurrentFolder)
        {
            DataTable dataTable = new DataTable("FolderContents");

            dataTable.Columns.Add("Icon", typeof(object));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Date", typeof(string));
            dataTable.Columns.Add("Type", typeof(string));
            dataTable.Columns.Add("Size", typeof(string));

            try
            {
                string[] folders = Directory.GetDirectories(CurrentFolder);
                string[] files = Directory.GetFiles(CurrentFolder);

                var folderIcon = IconReaderService.GetFolderIconImage(true, true);
                foreach (var folder in folders)
                {
                    string name = folder.Substring(folder.LastIndexOf("\\") + 1);
                    DateTime modified = Directory.GetLastWriteTime(folder);
                    dataTable.Rows.Add(folderIcon, name, modified, "File Folder", "");
                }

                foreach (var file in files)
                {
                    string name = file.Substring(file.LastIndexOf("\\") + 1);
                    FileInfo fi = new FileInfo(file);
                    DateTime modified = fi.LastWriteTime;
                    long kbSize = fi.Length / 1024;
                    string size = ((kbSize == 0 && fi.Length > 0) ? "1" : kbSize.ToString("n0")) + " KB";
                    string type = fi.Extension.StartsWith(".") ? fi.Extension.Substring(1).ToUpper() + " File" : "File";
                    var icon = IconReaderService.GetFileIconImage(file, false);
                    dataTable.Rows.Add(icon, name, modified, type, size);
                }
            }
            catch (UnauthorizedAccessException uae)
            {
                MessageBox.Show(uae.Message);
            }

            return dataTable;
        }

        private void ContentGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            DataRowView drv = lv.SelectedItem as DataRowView;
            if (drv != null)
            {
                string name = drv.Row.Field<string>("Name");
                Navigation.SelectedItem = name;
            }
        }

        private void ContentGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView lv = sender as ListView;
            DataRowView drv = lv.SelectedItem as DataRowView;
            if (drv != null)
            {
                string name = drv.Row.Field<string>("Name");
                string fullpath = System.IO.Path.Combine(Navigation.CurrentPath, name);
                if (Directory.Exists(fullpath))
                {
                    Navigation.CurrentPath = fullpath;
                }
                else
                {
                    System.Diagnostics.Process.Start(fullpath);
                }
            }
        }

        private void ContentGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ContentGrid_MouseDoubleClick(sender, null);
        }

        private void CommandCopyCut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            System.Collections.Specialized.StringCollection sc = new System.Collections.Specialized.StringCollection();
            sc.Add(System.IO.Path.Combine(Navigation.CurrentPath, Navigation.SelectedItem));
            Clipboard.SetFileDropList(sc);

            isCopy = e.Command == ApplicationCommands.Copy;
        }

        private void CommandCopyCut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (Navigation.SelectedItem != null && Navigation.SelectedItem != "" && !Navigation.IsFolder());
        }

        private void CommandPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (var srcFile in Clipboard.GetFileDropList())
            {
                try
                {
                    string filename = System.IO.Path.GetFileName(srcFile);
                    string dstFile = System.IO.Path.Combine(Navigation.CurrentPath, filename);

                    if (srcFile.ToLower() == dstFile.ToLower())
                    {
                        MessageBox.Show("You cannot paste file to the same location", "File Paste", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }

                    if (File.Exists(dstFile))
                    {
                        var result = MessageBox.Show("The file \"" + filename + "\" is already existing in current folder.\nDo you want to overwrite this file?", "File Existing", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.No)
                            continue;
                        if (!isCopy) File.Delete(dstFile);
                    }
                    if (isCopy)
                        File.Copy(srcFile, dstFile, true);
                    else
                        File.Move(srcFile, dstFile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void CommandPaste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (Navigation.CurrentPath != null && Navigation.CurrentPath != "" && Clipboard.ContainsFileDropList());
        }

        private void CommandDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                File.Delete(System.IO.Path.Combine(Navigation.CurrentPath, Navigation.SelectedItem));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            contextMenu.Items.Remove(customMenuItem);
            if (CustomMenuItems.Count() > 0)
            {
                contextMenu.Items.Add(customMenuItem);
                foreach (var importedCustomMenuItem in CustomMenuItems)
                {
                    customMenuItem.Items.Add(importedCustomMenuItem);
                }
            }
        }

        #endregion
    }




}
