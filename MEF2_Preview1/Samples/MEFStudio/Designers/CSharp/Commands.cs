//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contracts;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using Loader;

namespace Designers.CSharp
{
    public static class Commands
    {
        [Export(typeof(Action<HostSurface>))]
        [CommandMetadata(Language = "C#", ItemType = "Form", Category = "&Edit", Name = "&Cut")]
        public static void Cut(HostSurface hostSurface)
        {
            IMenuCommandService ims = hostSurface.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
	    if(ims!=null)
	            ims.GlobalInvoke(StandardCommands.Cut);
        }

        [Export(typeof(Action<HostSurface>))]
        [CommandMetadata(Language = "C#", ItemType = "Form", Category = "&Edit", Name = "Co&py")]
        public static void Copy(HostSurface hostSurface)
        {
            IMenuCommandService ims = hostSurface.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
	    if(ims!=null)
	            ims.GlobalInvoke(StandardCommands.Copy);
        }

        [Export(typeof(Action<HostSurface>))]
        [CommandMetadata(Language = "C#", ItemType = "Form", Category = "&Edit", Name = "&Paste")]
        public static void Paste(HostSurface hostSurface)
        {
            IMenuCommandService ims = hostSurface.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
	    if(ims!=null)
	            ims.GlobalInvoke(StandardCommands.Paste);
        }

    }
}
