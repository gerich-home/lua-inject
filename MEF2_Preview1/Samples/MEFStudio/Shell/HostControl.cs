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

namespace Shell
{
    public partial class HostControl : UserControl
    {
        private HostSurface hostSurface = null;

        public HostControl()
        {
            InitializeComponent();
        }

        public void InitializeHost(HostSurface hostSurface)
        {
            if (hostSurface == null)
                return;

            Control control = hostSurface.View as Control;
            control.Parent = this;
            control.Dock = DockStyle.Fill;
            control.Visible = true;
            this.hostSurface = hostSurface;
        }

        public HostSurface HostSurface
        {
            get
            {
                return hostSurface;
            }
        }
    }
}
