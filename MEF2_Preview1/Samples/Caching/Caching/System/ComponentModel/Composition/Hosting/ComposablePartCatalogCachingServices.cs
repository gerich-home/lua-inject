// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Caching;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using Microsoft.Internal;
using System.Reflection;

namespace System.ComponentModel.Composition.Hosting
{
#if PUBLIC_CACHING_API
    public 
#else
    internal
#endif
        static class ComposablePartCatalogCachingServices
    {
        public static bool IsCacheUpToDate(ICachedComposablePartCatalog catalog)
        {
            Requires.NotNull(catalog, "catalog");
            return catalog.IsCacheUpToDate;
        }

        public static void CacheCatalog(ICachedComposablePartCatalog catalog, string assemblyPath)
        {
            Requires.NotNull(catalog, "catalog");
            Requires.NotNull(assemblyPath, "assemblyPath");

            assemblyPath = Path.GetFullPath(assemblyPath);

            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = Path.GetFileNameWithoutExtension(assemblyPath);
            assemblyName.CodeBase = assemblyPath;

            CacheCatalog(catalog, assemblyName, Path.GetDirectoryName(assemblyPath));
        }

        public static void CacheCatalog(ICachedComposablePartCatalog catalog, AssemblyName assemblyName, string cacheStorageDirectory)
        {
            Requires.NotNull(catalog, "catalog");
            Requires.NotNull(assemblyName, "assemblyName");
            Requires.NotNull(cacheStorageDirectory, "cacheStorageDirectory");

            ComposablePartCatalogAssemblyCacheWriter writer = new ComposablePartCatalogAssemblyCacheWriter(assemblyName, cacheStorageDirectory);
            catalog.CacheCatalog(writer);
            writer.Dispose();
        }

        public static ICachedComposablePartCatalog ReadCatalogFromCache(string assemblyPath)
        {
            Requires.NotNull(assemblyPath, "assemblyPath");

            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = Path.GetFileNameWithoutExtension(assemblyPath);
            assemblyName.CodeBase = assemblyPath;
            return ReadCatalogFromCache(assemblyName);
        }

        public static ICachedComposablePartCatalog ReadCatalogFromCache(AssemblyName assemblyName)
        {
            Requires.NotNull(assemblyName, "assemblyName");

            Assembly assembly = Assembly.Load(assemblyName);

            using (ComposablePartCatalogAssemblyCacheReader reader = new ComposablePartCatalogAssemblyCacheReader(assembly))
            {
                return (ICachedComposablePartCatalog)reader.ReadRootCatalog();
            }
        }
    }
}
