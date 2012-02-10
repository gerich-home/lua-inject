// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition.Caching
{
#if PUBLIC_CACHING_API
    public 
#else
    internal
#endif
        abstract class ComposablePartCatalogCache
    {
        /// <summary>
        /// Catalog metadata
        /// </summary>
        public abstract IDictionary<string, object> Metadata { get; }

        /// <summary>
        /// Returns the catalog that represents the cache. Note, that this is not the originally cached catalog, bur rather a representation
        /// of the cached part definitions.
        /// </summary>
        /// <param name="catalogSite"></param>
        /// <returns></returns>
        public abstract ComposablePartCatalog GetCacheCatalog(ICachedComposablePartCatalogSite catalogSite);

        /// <summary>
        /// The reader that has provided the cache
        /// </summary>
        public abstract ComposablePartCatalogCacheReader Reader { get; }
    }
}
