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
using System.ComponentModel;

namespace Designers
{
    [Export(typeof(HostSurfaceFactory))]
    [DesignerMetadata(Language = "None", ItemType = "Component", FileExtension = "no")]
    public class BasicComponentFactory : HostSurfaceFactory
    {
        public override HostSurface CreateNewCore(string name)
        {
            return new BasicComponent(this, name);
        }

        public override IEnumerable<Lazy<Action<HostSurface>, ICommandMetadataView>> GetCommands()
        {
            return Commands.Where(c => c.Metadata.Language == "None" && c.Metadata.ItemType == "Component");
        }
    }

    public class BasicComponent : HostSurface
    {
        public BasicComponent(HostSurfaceFactory hostSurfaceFactory, string name)
            : base(hostSurfaceFactory)
        {
            this.ServiceContainer.AddService(typeof(INameCreationService), new NameCreationService());
            this.BeginLoad(typeof(Component));
            ((Control)this.View).BackColor = Color.FloralWhite;
            this.Loader = null;
        }
    }
}
