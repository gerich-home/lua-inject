//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Microsoft.Samples.XFileExplorer
{
    public interface IFileExplorerViewMetadata
    {
        string Name { get; }

        Dock Docking { get; }

        [DefaultValue(0)]
        int DockId { get; }

        [DefaultValue(false)]
        bool Hidden { get; }

    }

    [MetadataAttribute]
    public sealed class FileExplorerViewMetadata : Attribute
    {
        public string Name { get; set; }    // Name of the view to be displayed in View menu
        public Dock Docking { get; set; }   // Indicate which side the view will dock to
        public int DockId { get; set; }     // The order of the view will be docked
        public bool Hidden { get; set; }    // Indicate whether the view is shown or hidden on start-up

        public FileExplorerViewMetadata()
        {
            Name = "No Name Pane";
            DockId = 0;
            Hidden = false;
        }
    }
}
