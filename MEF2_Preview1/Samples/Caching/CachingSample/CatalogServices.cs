using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.ComponentModel.Composition.Hosting;

namespace CachingSample
{
    public static class CatalogServices
    {
        const string PARTS_PATH = @".\Parts\CachingSampleParts.dll";
        const string CACHE_PATH = @".\Parts\Cache.dll";
        const string CACHE_SHADOW_PATH = @".\Parts\CacheShadow\Cache.dll";


        public static ComposablePartCatalog GetCatalog()
        {
            ComposablePartCatalog catalog;
            if (!File.Exists(CACHE_PATH))
                catalog = CreateCacheCatalog();
            else
                catalog = LoadCacheCatalog();
            return catalog;
        }

        public static ComposablePartCatalog LoadCacheCatalog()
        {
            Console.WriteLine("Reading cache:");
            File.Copy(CACHE_PATH, CACHE_SHADOW_PATH, true);
            var catalog = ComposablePartCatalogCachingServices.ReadCatalogFromCache(CACHE_SHADOW_PATH);
            if (!catalog.IsCacheUpToDate)
            {
                Console.WriteLine("Updating cache");
                ComposablePartCatalogCachingServices.CacheCatalog(catalog, CACHE_PATH);
            }

            return (ComposablePartCatalog)catalog;
        }

        public static ComposablePartCatalog CreateCacheCatalog()
        {
            Console.WriteLine("Creating Cache:");
            var catalog = new CachedAssemblyCatalog(PARTS_PATH);
            ComposablePartCatalogCachingServices.CacheCatalog(catalog, CACHE_PATH);
            return catalog;
        }

    }
}
