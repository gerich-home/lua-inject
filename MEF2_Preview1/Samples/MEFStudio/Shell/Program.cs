//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using System.Reflection;

namespace Shell
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Compose();
        }

        static void Compose()
        {
            DirectoryCatalog directoryCatalog = new DirectoryCatalog(".");
            AssemblyCatalog assemblyCatalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            AggregateCatalog aggregateCatalog = new AggregateCatalog();
	        aggregateCatalog.Catalogs.Add(directoryCatalog);
	        aggregateCatalog.Catalogs.Add(assemblyCatalog);

            CompositionContainer container = new CompositionContainer(aggregateCatalog);
            
            RefreshCatalog refreshCatalog = new RefreshCatalog(directoryCatalog);
            container.ComposeParts(refreshCatalog);
            
            Application.Run(container.GetExportedValue<MainShell>());
        }
    }

    public class RefreshCatalog
    {
        DirectoryCatalog directoryCatalog = null;

        public RefreshCatalog(DirectoryCatalog dc)
        {
            this.directoryCatalog = dc;
        }

        [Export("RefreshExtensions", typeof(Action))]
        public void RefreshExtensions()
        {
            this.directoryCatalog.Refresh();
        }
    }
}
