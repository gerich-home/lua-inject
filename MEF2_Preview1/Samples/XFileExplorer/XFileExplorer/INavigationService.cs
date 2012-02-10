//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

//
// MEF uses structural matching which allows component authors to use shared contract
// without having to share binaries
//

using System;
using System.ComponentModel.Composition;

namespace Microsoft.Samples.XFileExplorer
{
    public delegate void CurrentPathChangedHandler();
    public delegate void SelectedItemChangedHandler();

    public interface INavigationService
    {
        event CurrentPathChangedHandler CurrentPathChanged;
        event SelectedItemChangedHandler SelectedItemChanged;
        string CurrentPath { get; set; }
        string SelectedItem { get; set; }
        bool IsFolder();
    }
}
