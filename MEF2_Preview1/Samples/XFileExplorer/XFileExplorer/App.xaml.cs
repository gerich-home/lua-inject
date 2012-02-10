//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Windows;

namespace Microsoft.Samples.XFileExplorer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private CompositionContainer _container;

        [Import("Microsoft.Samples.XFileExplorer.MainWindowContract")]
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

            if (_container != null)
            {
                _container.Dispose();
            }
        }

        private bool Compose()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            catalog.Catalogs.Add(new DirectoryCatalog("AddIns"));

            _container = new CompositionContainer(catalog);
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(this);
            batch.AddExportedValue<CompositionContainer>(_container);

            try
            {
                _container.Compose(batch);
            }
            catch (CompositionException compositionException)
            {
                MessageBox.Show(compositionException.ToString());
                Shutdown(1);
            }
            return true;
        }
    }
}
