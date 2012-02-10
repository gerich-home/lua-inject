//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;
using System.Collections;
using System.ComponentModel.Composition;
using System.ComponentModel.Design.Serialization;

namespace Contracts
{
    public class HostSurface : DesignSurface
    {
        private IPropertyGrid _propertyGrid = null;
        private ISelectionService _selectionService;
        private HostSurfaceFactory _hostSurfaceFactory = null;
        private BasicDesignerLoader _loader;

        public HostSurface(HostSurfaceFactory hostSurfaceFactory)
            : base()
        {
            _hostSurfaceFactory = hostSurfaceFactory;
        }

        void selectionService_SelectionChanged(object sender, EventArgs e)
        {
            if (_selectionService == null)
                return;
            ICollection selectedComponents = _selectionService.GetSelectedComponents();
            object[] comps = new object[selectedComponents.Count];
            int i = 0;
            foreach (Object o in selectedComponents)
            {
                comps[i] = o;
                i++;
            }
            _propertyGrid.SetSelectedObjects(comps);
            
        }

        public IPropertyGrid PropertyGrid
        {
            set
            {
                _propertyGrid = value;
                _propertyGrid.SetSelectedObjects(new object[] {this.DesignerHost.RootComponent});
                _selectionService = (ISelectionService)(this.ServiceContainer.GetService(typeof(ISelectionService)));
                _selectionService.SelectionChanged += new EventHandler(selectionService_SelectionChanged);
            }
        }

        public HostSurfaceFactory HostSurfaceFactory
        {
            get
            {
                return _hostSurfaceFactory;
            }
            set
            {
                _hostSurfaceFactory = value;
            }
        }

        public IDesignerHost DesignerHost
        {
            get
            {
                return this.ServiceContainer.GetService(typeof(IDesignerHost)) as IDesignerHost;
            }
        }

        public BasicDesignerLoader Loader
        {
            get
            {
                return _loader;
            }
            set
            {
                _loader = value;
            }
        }

        public virtual string GetCode()
        {
            return String.Empty;
        }
    }
}
