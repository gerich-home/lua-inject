//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Drawing;
using Contracts;
using System.ComponentModel.Composition;
using Loader;
using System.ComponentModel.Design.Serialization;

namespace Designers
{
    [Export(typeof(HostSurfaceFactory))]
    [DesignerMetadata(Language = "None", ItemType = "Form", FileExtension = "no")]
    public class BasicFormFactory : HostSurfaceFactory
    {
        public override HostSurface CreateNewCore(string name)
        {
            return new BasicForm(this, name);
        }

        public override IEnumerable<Lazy<Action<HostSurface>, ICommandMetadataView>> GetCommands()
        {
            return Commands.Where(c => c.Metadata.Language == "None" && c.Metadata.ItemType == "Form");
        }
    }

    public class BasicForm : HostSurface
    {
        public BasicForm(HostSurfaceFactory hostSurfaceFactory, string name)
            : base(hostSurfaceFactory)
        {
            this.ServiceContainer.AddService(typeof(INameCreationService), new NameCreationService());
            this.BeginLoad(typeof(Form));
            ((Control)this.View).BackColor = Color.White;
            this.Loader = null;
        }
    }
}
