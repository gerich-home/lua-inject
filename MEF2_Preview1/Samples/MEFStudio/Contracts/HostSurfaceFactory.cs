//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;
using System.ComponentModel.Composition;

namespace Contracts
{
    public abstract class HostSurfaceFactory
    {
        [Import]
        protected IPropertyGrid propertyGrid = null;

        [ImportMany(AllowRecomposition=true)]
        protected Lazy<Action<HostSurface>, ICommandMetadataView>[] Commands = null;

        public HostSurface CreateNew(string name)
        {
            HostSurface hostSurface = CreateNewCore(name);
            hostSurface.PropertyGrid = propertyGrid;
            return hostSurface;
        }

        public abstract HostSurface CreateNewCore(string name);

        public abstract IEnumerable<Lazy<Action<HostSurface>, ICommandMetadataView>> GetCommands();
    }
}
