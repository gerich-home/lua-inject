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

namespace MoreCommands
{
    public static class Commands
    {
        [Export(typeof(Action<HostSurface>))]
        [CommandMetadata(Language = "C#", ItemType = "Form", Category = "&Debug", Name = "&Run")]
        public static void Run(HostSurface hostSurface)
        {
            (hostSurface.Loader as CodeDomHostLoader).Run();
        }

        [Export(typeof(Action<HostSurface>))]
        [CommandMetadata(Language = "C#", ItemType = "Form", Category = "&Edit", Name = "&SelectAll")]
        public static void SelectAll(HostSurface hostSurface)
        {
            IMenuCommandService ims = hostSurface.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            ims.GlobalInvoke(StandardCommands.SelectAll);
        }
    }
}
