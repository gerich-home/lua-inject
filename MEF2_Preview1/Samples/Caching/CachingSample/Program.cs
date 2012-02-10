using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Caching;
using System.ComponentModel.Composition.Primitives;
using CachingSampleContracts;


namespace CachingSample
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(CurrentDomain_AssemblyLoad);
            ComposablePartCatalog catalog = CatalogServices.GetCatalog();
            
            var container = new CompositionContainer(catalog);
            var pluginExports = container.GetExports<IPlugin, IPluginMetadata>();

            ShowPluginMetadata(pluginExports);
            CreatePluginInstances(pluginExports);
            Console.Read();
        }

        static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            if (!args.LoadedAssembly.GetName().Name.Contains("Proxies_"))
            {
                ConsoleColor CurrentColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(string.Format("Loading {0}.dll", args.LoadedAssembly.GetName().Name));
                Console.ForegroundColor = CurrentColor;
            }
        }

        private static void CreatePluginInstances(IEnumerable<Lazy<IPlugin, IPluginMetadata>> pluginExports)
        {
            Console.WriteLine("Creating plugin instances");
            foreach (var pluginExport in pluginExports)
            {
                var plugin = pluginExport.Value;
            }
        }

        private static void ShowPluginMetadata(IEnumerable<Lazy<IPlugin, IPluginMetadata>> pluginExports)
        {
            Console.WriteLine("Reading metadata for plugins:");
            foreach (var pluginExport in pluginExports)
            {
                Console.WriteLine(string.Format("\tName: {0}", pluginExport.Metadata.Name));
            }
        }

    }
}
