//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Contracts
{
    public interface IOutputWindow
    {
        void Writeline(string text);
        UserControl View { get; }
    }
}
