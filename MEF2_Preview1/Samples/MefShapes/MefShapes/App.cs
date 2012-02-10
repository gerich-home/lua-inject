//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Windows;
using Microsoft.Samples.MefShapes.Shapes.Library;
using Microsoft.ComponentModel.Composition.DynamicInstantiation;

namespace Microsoft.Samples.MefShapes
{
    public partial class App : Application
    {
        private CompositionContainer _container;

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [STAThreadAttribute()]
        public static void Main()
        {
            new App().Run();
        }

        [Import(typeof(MainWindow))]
        public new Window MainWindow
        {
            get { return base.MainWindow; }
            set { base.MainWindow = value; }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (Compose())
            {
                MainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (this._container != null)
            {
                this._container.Dispose();
            }
        }

        private bool Compose()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(IMefShapesGame).Assembly));
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(DefaultDimensions).Assembly));
            var partCreatorEP = new DynamicInstantiationExportProvider();

            this._container = new CompositionContainer(catalog, partCreatorEP);
            partCreatorEP.SourceProvider = this._container;

            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(this);
            batch.AddExportedValue<ICompositionService>(this._container);
            batch.AddExportedValue<AggregateCatalog>(catalog);

            try
            {
                this._container.Compose(batch);
            }
            catch (CompositionException compositionException)
            {
                MessageBox.Show(compositionException.ToString());
                Shutdown(1);
                return false;
            }
            return true;
        }
    }
}
