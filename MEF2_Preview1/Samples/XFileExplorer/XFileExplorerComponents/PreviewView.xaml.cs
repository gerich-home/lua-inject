//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Samples.XFileExplorer;

namespace Microsoft.Samples.XFileExplorerComponents
{
    /// <summary>
    /// Interaction logic for PreviewView.xaml
    /// </summary>
    [Export("Microsoft.Samples.XFileExplorer.FileExplorerViewContract", typeof(UserControl))]
    [ExportMetadata("Name", "Preview Pane")]
    [ExportMetadata("Docking", Dock.Right)]
    [ExportMetadata("DockId", 0)]
    public partial class PreviewView : UserControl
    {
        [Import]
        public INavigationService Navigation = null;

        [ImportMany(typeof(PreviewControl))]
        private Lazy<PreviewControl, IPreviewMetadata>[] Viewers { set; get; }

        private Dictionary<string, List<Lazy<PreviewControl, IPreviewMetadata>>> _viewerDic = null;

        private PreviewControl _currentViewer = null;

        private bool _initialized = false;

        public PreviewView()
        {
            InitializeComponent();
            _viewerDic = new Dictionary<string, List<Lazy<PreviewControl, IPreviewMetadata>>>();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_initialized)
            {
                foreach (var viewer in Viewers)
                {
                    foreach (string format in viewer.Metadata.Format)
                    {
                        string type = format.ToUpper();
                        if (!_viewerDic.ContainsKey(type))
                            _viewerDic[type] = new List<Lazy<PreviewControl, IPreviewMetadata>>();
                        _viewerDic[type].Add(viewer);
                    }
                }

                Navigation.SelectedItemChanged += new SelectedItemChangedHandler(NavigationService_SelectedItemChanged);

                NavigationService_SelectedItemChanged();

                _initialized = true;
            }
        }

        private void NavigationService_SelectedItemChanged()
        {
            if (!Navigation.IsFolder())
            {
                DirectoryInfo di = new DirectoryInfo(Navigation.CurrentPath);
                FileInfo fi = di.GetFiles().FirstOrDefault(i => i.Name == Navigation.SelectedItem);
                string type = fi.Extension.StartsWith(".") ? fi.Extension.Substring(1).ToUpper() : "";
                if (_viewerDic.ContainsKey(type))
                {
                    if (_currentViewer == null || _currentViewer != _viewerDic[type].First().Value)
                    {
                        _currentViewer = _viewerDic[type].First().Value;
                        PreviewPane.Children.Clear();
                        PreviewPane.Children.Add(_currentViewer);
                    }

                    _currentViewer.UpdatePreview();
                    return;
                }
            }

            PreviewPane.Children.Clear();
            PreviewPane.Children.Add(PreviewMessage);
            _currentViewer = null;
        }
    }
}
