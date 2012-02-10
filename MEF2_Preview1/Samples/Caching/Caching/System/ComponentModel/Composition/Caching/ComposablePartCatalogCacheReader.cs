// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Reflection;

namespace System.ComponentModel.Composition.Caching
{
#if PUBLIC_CACHING_API
    public 
#else
    internal
#endif
        abstract class ComposablePartCatalogCacheReader : IDisposable
    {
        internal static string CatalogTypeMetadataKey = "System.ComponentModel.Composition.Caching.CatalogType";

        /// <summary>
        /// Reads the catalog cache given the cache token.
        /// </summary>
        /// <param name="cacheToken">The cache token.</param>
        /// <returns>The catalog cache corresponding to the specified cache token.</returns>
        protected abstract ComposablePartCatalogCache ReadCacheCore(object cacheToken);

        /// <summary>
        /// Returns the cache token corresponding to the root catalog in the cache.
        /// </summary>
        protected abstract object RootCacheToken { get; }

        /// <summary>
        /// Reads the catalog from the cache given the specified cache token. This methos trows if thew catalog with the given
        /// token doesn't exist.
        /// </summary>
        /// <param name="cacheToken">The cache token.</param>
        /// <returns>The catalog corresponding to the specified cache token.</returns>
        public ComposablePartCatalog ReadCatalog(object cacheToken)
        {
            ComposablePartCatalogCache catalogCache = this.ReadCacheCore(cacheToken);
            Type catalogType = catalogCache.Metadata.GetValue<Type>(ComposablePartCatalogCacheReader.CatalogTypeMetadataKey);
            return (ComposablePartCatalog)Activator.CreateInstance(
                catalogType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new object[] { catalogCache },
                null);
                
        }

        /// <summary>
        /// Reads the root catalog from the cache
        /// </summary>
        /// <returns>The root catalog.</returns>
        public ComposablePartCatalog ReadRootCatalog()
        {
            return this.ReadCatalog(this.RootCacheToken);
        }

        /// <summary>
        /// Releases all the resources allocated by the <see cref="ComposablePartCatalogCacheReader"/> and completes the cache writing.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all the resources allocated by the <see cref="ComposablePartCatalogCacheWriter"/> and completes the cache writing.
        /// </summary>
        /// <param name="disposing">Is called from Dispose() if set to <c>true</c>, otherwise is called from the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
