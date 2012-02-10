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
using System.ComponentModel.Composition;
using Contracts;

namespace ToolWindows
{
    [Export(typeof(ISolutionExplorer))]
    public partial class SolutionExplorer : UserControl, ISolutionExplorer
    {
        public SolutionExplorer()
        {
            InitializeComponent();
        }

        #region ISolutionExplorer Members

        public void AddFileNode(string text)
        {
            this.treeView1.Nodes[0].Nodes.Add(text);
            this.treeView1.ExpandAll();
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
