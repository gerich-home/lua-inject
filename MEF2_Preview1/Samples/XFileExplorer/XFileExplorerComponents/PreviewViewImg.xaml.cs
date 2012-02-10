//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Samples.XFileExplorer;

namespace Microsoft.Samples.XFileExplorerComponents
{
    /// <summary>
    /// Interaction logic for PreviewViewImg.xaml
    /// </summary>
    [Export(typeof(PreviewControl))]
    [ExportMetadata("Format", "BMP", IsMultiple = true)]
    [ExportMetadata("Format", "JPG", IsMultiple = true)]
    [ExportMetadata("Format", "JPGE", IsMultiple = true)]
    [ExportMetadata("Format", "PNG", IsMultiple = true)]
    [ExportMetadata("Format", "GIF", IsMultiple = true)]
    [ExportMetadata("Format", "ICO", IsMultiple = true)]
    [ExportMetadata("Format", "TIFF", IsMultiple = true)]
    [ExportMetadata("Format", "TIF", IsMultiple = true)]
    [ExportMetadata("Format", "WDP", IsMultiple = true)]
    public partial class PreviewViewImg : PreviewControl
    {
        [Import]
        public INavigationService Navigation = null;

        public PreviewViewImg()
        {
            InitializeComponent();
        }

        public override void UpdatePreview()
        {
            if (!Navigation.IsFolder())
            {
                // BitmapImage.UriSource must be in a BeginInit/EndInit block
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(System.IO.Path.Combine(Navigation.CurrentPath, Navigation.SelectedItem));
                bi.EndInit();
                ImagePane.Source = bi;
            }
        }
    }
}
