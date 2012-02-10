//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Controls;
using Microsoft.Samples.XFileExplorer;

namespace Microsoft.Samples.XFileExplorerComponents
{
    /// <summary>
    /// Interaction logic for PreviewViewTxt.xaml
    /// </summary>
    [Export(typeof(PreviewControl))]
    [ExportMetadata("Format", "TXT", IsMultiple = true)]
    [ExportMetadata("Format", "LOG", IsMultiple = true)]
    [ExportMetadata("Format", "INI", IsMultiple = true)]
    [ExportMetadata("Format", "BAT", IsMultiple = true)]
    [ExportMetadata("Format", "CMD", IsMultiple = true)]
    public partial class PreviewViewTxt : PreviewControl
    {
        [Import]
        public INavigationService Navigation = null;

        private int BUFFER_SIZE = 500;

        public PreviewViewTxt()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Update the preview of current selected item.
        /// This function is to be called by Preview View via structural casting.
        /// </summary>
        public override void UpdatePreview()
        {
            if (!Navigation.IsFolder())
            {
                try
                {
                    FileStream stream = new FileStream(System.IO.Path.Combine(Navigation.CurrentPath, Navigation.SelectedItem),
                        FileMode.Open, FileAccess.Read, FileShare.Read);
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        char[] buf = new char[BUFFER_SIZE];
                        int size = sr.Read(buf, 0, BUFFER_SIZE);
                        string content = new string(buf, 0, size);
                        TextPane.Text = content;
                    }
                }
                catch (IOException ex)
                {
                    TextPane.Text = ex.Message;
                }
            }
        }
    }
}

