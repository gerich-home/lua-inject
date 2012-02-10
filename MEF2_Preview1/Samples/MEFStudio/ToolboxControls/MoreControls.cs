//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Contracts;

namespace ToolboxControls
{
    public static class MoreControls
    {
	    private const string Winforms = "Windows Forms";

        [Export]
        [ToolboxItemMetadata(Category=Winforms)]
        private static Type t1 = typeof(System.Windows.Forms.BindingSource);

        [Export]
        [ToolboxItemMetadata(Category = Winforms)]
        private static Type t2 = typeof(System.Windows.Forms.BindingNavigator);

        [Export]
        [ToolboxItemMetadata(Category = "My Custom Controls")]
        private static Type t3 = typeof(System.Windows.Forms.FlowLayoutPanel);

    }
}
