// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using Microsoft.Internal;
using System.IO;
using System.Reflection;
using System.Globalization;

namespace System.ComponentModel.Composition.Caching
{
    internal partial class ComposablePartCatalogAssemblyCache : ComposablePartCatalogCache
    {
        private Type _stubType;
        private IDictionary<string, object> _metadata;
        private ComposablePartCatalogCacheReader _reader;

        public ComposablePartCatalogAssemblyCache(Type stubType, ComposablePartCatalogCacheReader reader)
        {
            Assumes.NotNull(stubType);
            Assumes.NotNull(reader);

            this._stubType = stubType;
            this._reader = reader;

            // Get catalog metadata
            MethodInfo getCatalogMetadataMethod = ComposablePartCatalogAssemblyCacheReader.GetCacheTypeMethod(this._stubType, CacheStructureConstants.CachingStubGetCatalogMetadata, BindingFlags.Public | BindingFlags.Static);
            Func<IDictionary<string, object>> getCatalogMetadata = (Func<IDictionary<string, object>>)Delegate.CreateDelegate(typeof(Func<IDictionary<string, object>>), getCatalogMetadataMethod);
            this._metadata = getCatalogMetadata.Invoke();
        }

        public override IDictionary<string, object> Metadata
        {
            get
            {
                return this._metadata;
            }
        }

        internal Type StubType
        {
            get
            {
                return this._stubType;
            }
        }

        public override ComposablePartCatalog GetCacheCatalog(ICachedComposablePartCatalogSite catalogSite)
        {
            Requires.NotNull(catalogSite, "catalogSite");
            return new CatalogCache(this._stubType, catalogSite);
        }

        public override ComposablePartCatalogCacheReader Reader
        {
            get
            {
                return this._reader;
            }
        }
    }

}
