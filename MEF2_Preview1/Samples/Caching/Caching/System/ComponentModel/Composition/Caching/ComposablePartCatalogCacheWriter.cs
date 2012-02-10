// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Caching
{
#if PUBLIC_CACHING_API
    public 
#else
    internal
#endif
        abstract class ComposablePartCatalogCacheWriter : IDisposable
    {
        /// <summary>
        /// Writes part definitions and catalog metadata into the cache.
        /// </summary>
        /// <param name="catalogType">Catalog type.</param>
        /// <param name="partDefinitions">Parts definitions.</param>
        /// <param name="catalogMetadata">Catalog Metadata.</param>
        /// <param name="catalogSite">Catalog Site</param>
        /// <returns>Catalog cache token. This value can be cached and used to locate this cache wen reading from the cache.</returns>
        public object WriteCache(Type catalogType, IEnumerable<ComposablePartDefinition> partDefinitions, IDictionary<string, object> catalogMetadata, ICachedComposablePartCatalogSite catalogSite)
        {
            Requires.NotNull(catalogType, "catalogType");

            ConstructorInfo constructor = catalogType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(ComposablePartCatalogCache) },
                null);

            if (constructor == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Strings.CacheableCatalogMustHaveProperConstructor, catalogType.FullName, typeof(ComposablePartCatalogCache).FullName));
            }

            catalogMetadata = (catalogMetadata != null) ? new Dictionary<string, object>(catalogMetadata) : new Dictionary<string, object>();
            catalogMetadata[ComposablePartCatalogCacheReader.CatalogTypeMetadataKey] = catalogType;

            partDefinitions = partDefinitions ?? Enumerable.Empty<ComposablePartDefinition>();

            return this.WriteCacheCore(partDefinitions, catalogMetadata, catalogSite);
        }

        /// <summary>
        /// Writes part definitions and catalog metadata into the cache.
        /// </summary>
        /// <param name="partDefinitions">Parts definitions.</param>
        /// <param name="catalogMetadata">Catalog Metadata.</param>
        /// <param name="catalogSite">Catalog Site</param>
        /// <returns>Catalog cache token. This value can be cached and used to locate this cache wen reading from the cache.</returns>
        protected abstract object WriteCacheCore(IEnumerable<ComposablePartDefinition> partDefinitions, IDictionary<string, object> catalogMetadata, ICachedComposablePartCatalogSite catalogSite);

        /// <summary>
        /// Sets "root" catalog of the cache - that is one that the corresponding <see cref="ComposablePartCatalogCache"/> will open first when the 
        /// appropriate storage is read. This method may be called multiple times; the last set value is expected to be written to the cache when the
        /// <see cref="ComposablePartCatalogCacheWriter"/> is disposed.
        /// </summary>
        /// <param name="catalogToken">Value returned by a call to WriteCache.</param>
        public abstract void WriteRootCacheToken(object cacheToken);

        /// <summary>
        /// Releases all the resources allocated by the <see cref="ComposablePartCatalogCacheWriter"/> and completes the cache writing.
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
