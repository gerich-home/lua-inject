//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Contracts;
using System.ComponentModel.Composition;

namespace ToolWindows
{
    [Export(typeof(IOutputWindow))]
    public partial class OutputWindow : UserControl, IOutputWindow
    {
        public OutputWindow()
        {
            InitializeComponent();
        }

        #region IOutputWindow Members

        public void Writeline(string text)
        {
            this.RichTextBox.Text += text + "\n";
        }

        public UserControl View
        {
            get
            {
                return this;
            }
        }

        #endregion
    }
}
