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
using Designers;
using System.ComponentModel.Design.Serialization;

namespace MoreDesigners
{
    [Export(typeof(HostSurfaceFactory))]
    [DesignerMetadata(Language = "XML", ItemType = "Form", FileExtension="xml")]
    public class XMLFormFactory : HostSurfaceFactory
    {
        public override HostSurface CreateNewCore(string name)
        {
            return new XMLForm(this, name);
        }

        public override IEnumerable<Lazy<Action<HostSurface>, ICommandMetadataView>> GetCommands()
        {
            return Commands.Where(c => c.Metadata.Language == "XML" && c.Metadata.ItemType == "Form");
        }
    }

    public class XMLForm : HostSurface
    {
        private BasicHostLoader loader = null;

        public XMLForm(HostSurfaceFactory hostSurfaceFactory, string name)
            : base(hostSurfaceFactory)
        {
            this.ServiceContainer.AddService(typeof(INameCreationService), new NameCreationService());
            loader = new BasicHostLoader(typeof(Form), name);
            this.BeginLoad(loader);
            ((Control)this.View).BackColor = Color.White;
            this.Loader = loader;
        }

        public override string GetCode()
        {
            return loader.GetCode();
        }
    }
}
