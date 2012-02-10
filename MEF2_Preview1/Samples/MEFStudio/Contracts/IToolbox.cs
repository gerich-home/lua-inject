//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;

namespace Contracts
{
    public interface IToolbox
    {
        IDesignerHost DesignerHost { get; set; }
        IToolboxService ToolboxService { get; }
        void RefreshControls();
        UserControl View { get; }
    }
}
