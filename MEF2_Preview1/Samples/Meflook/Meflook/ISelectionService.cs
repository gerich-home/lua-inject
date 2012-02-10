//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;

namespace Meflook
{
    public delegate void CurrentFolderChangedHandler();
    public delegate void CurrentIndexChangedHandler();
    
    public interface ISelectionService
    {
        string CurrentFolder { get; set; } 
        event CurrentFolderChangedHandler CurrentFolderChanged;
        int CurrentIndex { get; set; }
        event CurrentIndexChangedHandler CurrentIndexChanged;
    }
}
