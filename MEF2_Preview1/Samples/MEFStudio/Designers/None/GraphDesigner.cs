//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;
using System.Drawing;
using Contracts;
using System.ComponentModel.Composition;
using Loader;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;

namespace Designers
{
    [Export(typeof(HostSurfaceFactory))]
    [DesignerMetadata(Language = "None", ItemType = "GraphDesigner", FileExtension = "no")]
    public class GraphDesignerFactory : HostSurfaceFactory
    {
        public override HostSurface CreateNewCore(string name)
        {
            return new GraphDesigner(this, name);
        }

        public override IEnumerable<Lazy<Action<HostSurface>, ICommandMetadataView>> GetCommands()
        {
            return Commands.Where(c => c.Metadata.Language == "None" && c.Metadata.ItemType == "GraphDesigner");
        }
    }

    public class GraphDesigner : HostSurface
    {
        public GraphDesigner(HostSurfaceFactory hostSurfaceFactory, string name)
            : base(hostSurfaceFactory)
        {
            this.ServiceContainer.AddService(typeof(INameCreationService), new NameCreationService());
            this.BeginLoad(typeof(MyTopLevelComponent));
            ((Control)this.View).BackColor = Color.White;
            this.Loader = null;
        }
    }
}
