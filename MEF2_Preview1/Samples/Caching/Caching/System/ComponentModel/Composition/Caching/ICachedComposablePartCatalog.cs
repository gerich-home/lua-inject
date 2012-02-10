// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.ComponentModel.Composition.Caching
{
#if PUBLIC_CACHING_API
    public 
#else
    internal
#endif
        interface ICachedComposablePartCatalog
    {
        /// <summary>
        /// Caches the contents of the catalog using the specified  <see cref="ComposablePartCatalogCacheWriter"/>.
        /// </summary>
        /// <param name="writer">The cache writer.</param>
        /// <returns>The cache token corresponding to the written catalog cache.</returns>
        object CacheCatalog(ComposablePartCatalogCacheWriter writer);

        /// <summary>
        /// Specifies whether the cache the state of the catalog has changed since the last time it was read from the cache.
        /// </summary>
        bool IsCacheUpToDate { get; }
    }
}
