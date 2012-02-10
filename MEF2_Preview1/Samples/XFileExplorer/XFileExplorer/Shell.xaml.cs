//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Samples.XFileExplorer
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    [Export("Microsoft.Samples.XFileExplorer.MainWindowContract", typeof(Window))]
    public partial class Shell : Window, IPartImportsSatisfiedNotification
    {
        [ImportMany("Microsoft.Samples.XFileExplorer.FileExplorerViewContract", AllowRecomposition=true)]
        private Lazy<UserControl, IFileExplorerViewMetadata>[] _views = null;

        public Shell()
        {
            InitializeComponent();
        }

        public void OnImportsSatisfied()
        {
            ReloadAllViews();
        }

        private void ReloadAllViews()
        {
            // Unload all views from UI
            ViewMenu.Items.Clear();
            TopPanel.Children.Clear();
            BottomPanel.Children.Clear();
            LeftPanel.Children.Clear();
            foreach (TabItem item in RightPanelTabs.Items)
                (item.Content as DockPanel).Children.Clear();
            RightPanelTabs.Items.Clear();

            // Reload all views
            foreach (var view in _views.OrderBy(i => i.Metadata.DockId))
            {
                //Get the instance of current view
                var childPane = view.Value;

                //Dock the view properly according to its metadata
                switch (view.Metadata.Docking)
                {
                    case Dock.Top:
                        TopPanel.Children.Add(childPane); break;
                    case Dock.Bottom:
                        BottomPanel.Children.Add(childPane); break;
                    case Dock.Left:
                        LeftPanel.Children.Add(childPane); break;
                    default:
                        TabItem item = RightPanelTabs.Items.Cast<TabItem>().FirstOrDefault(i => (int)i.Tag == view.Metadata.DockId);
                        if (item != null)
                        {
                            var grid = item.Content as Grid;
                            int column = grid.ColumnDefinitions.Count;
                            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(childPane.MinWidth, GridUnitType.Pixel) });
                            GridSplitter splitter = new GridSplitter() { Width = 2, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Stretch };
                            Grid.SetColumn(splitter, column - 1);
                            Grid.SetColumn(childPane, column);
                            grid.Children.Add(splitter);
                            grid.Children.Add(childPane);
                            
                        }
                        else
                        {
                            Grid grid = new Grid();
                            grid.ColumnDefinitions.Add(new ColumnDefinition());
                            Grid.SetColumn(childPane, 0);
                            grid.Children.Add(childPane);
                            item = new TabItem();
                            item.Header = view.Metadata.Name;
                            item.Content = grid;
                            item.Tag = view.Metadata.DockId;
                            RightPanelTabs.Items.Add(item);
                        }
                        break;
                }

                //Set the first Tab active if there is no one is active
                if (RightPanelTabs.SelectedIndex < 0 && RightPanelTabs.Items.Count > 0)
                    RightPanelTabs.SelectedIndex = 0;

                //Add the name of the view to the menu
                MenuItem mi = new MenuItem();
                mi.Header = view.Metadata.Name;
                mi.IsCheckable = true;
                mi.IsChecked = view.Metadata.Hidden ? false : true;
                mi.Checked += new RoutedEventHandler(ViewMenu_CheckChanged);
                mi.Unchecked += new RoutedEventHandler(ViewMenu_CheckChanged);
                ViewMenu.Items.Add(mi);

                //Set visibility of the view accordint to its metadata
                childPane.Visibility = view.Metadata.Hidden ? Visibility.Hidden : Visibility.Visible;
            }
        }

        private void ViewMenu_CheckChanged(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;

            UserControl view = _views.First(i => i.Metadata.Name == mi.Header.ToString()).Value;
            if (view != null)
                view.Visibility = mi.IsChecked ? Visibility.Visible : Visibility.Hidden;
        }

        private void HelpMenu_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Your feedback is always welcome!\n\nPlease post your questions or comments on http://www.codeplex.com/MEF/Thread/List.aspx.", "Contact Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
