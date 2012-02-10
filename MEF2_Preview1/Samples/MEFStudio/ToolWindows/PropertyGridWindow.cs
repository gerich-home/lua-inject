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
    [Export(typeof(IPropertyGrid))]
    public partial class PropertyGridWindow : UserControl, IPropertyGrid
    {
        public PropertyGridWindow()
        {
            InitializeComponent();
        }

        #region IPropertyGrid Members

        public void SetSelectedObjects(object[] objects)
        {
            this.propertyGrid1.SelectedObjects = objects;
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
