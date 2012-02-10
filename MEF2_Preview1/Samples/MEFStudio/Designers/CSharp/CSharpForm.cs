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

namespace Designers
{
    [Export(typeof(HostSurfaceFactory))]
    [DesignerMetadata(Language = "C#", ItemType = "Form", FileExtension="cs")]
    public class CSharpFormFactory : HostSurfaceFactory
    {
        public override HostSurface CreateNewCore(string name)
        {
            return new CSharpForm(this, name);
        }

        // ...

        public override IEnumerable<Lazy<Action<HostSurface>, ICommandMetadataView>> GetCommands()
        {
            return Commands.Where(c => c.Metadata.Language == "C#" && c.Metadata.ItemType == "Form");
        }
    }

    public class CSharpForm : HostSurface
    {
        private CodeDomHostLoader loader = null;

        public CSharpForm(HostSurfaceFactory hostSurfaceFactory, string name)
            : base(hostSurfaceFactory)
        {
            this.ServiceContainer.AddService(typeof(IMenuCommandService), new MenuCommandService(this));
            loader = new CodeDomHostLoader(name);
            this.BeginLoad(loader);
            ((Control)this.View).BackColor = Color.White;
            this.Loader = loader;
        }

        public override string GetCode()
        {
            return loader.GetCode("C#");
        }
    }
}
