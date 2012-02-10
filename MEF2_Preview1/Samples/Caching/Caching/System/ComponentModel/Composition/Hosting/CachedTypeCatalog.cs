// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.AttributedModel;
using System.ComponentModel.Composition.Caching;
using System.ComponentModel.Composition.Caching.AttributedModel;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Hosting
{
#if PUBLIC_CACHING_API
    public 
#else
    internal
#endif
        class CachedTypeCatalog : ComposablePartCatalog, ICachedComposablePartCatalog, ICompositionElement
    {
        private TypeCatalog _typeCatalog;
        private ComposablePartCatalog _cacheCatalog = null;
        private List<ComposablePartDefinition> _upToDateParts = null;
        private string _displayName = null;
        private volatile int _isDisposed = 0;

        public CachedTypeCatalog(params Type[] types)
        {
            this._typeCatalog = new TypeCatalog(types);
        }

        public CachedTypeCatalog(IEnumerable<Type> types)
        {
            this._typeCatalog = new TypeCatalog(types);
        }

        public override string ToString()
        {
            return this.GetDisplayName();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // NOTE : According to http://msdn.microsoft.com/en-us/library/4bw5ewxy.aspx, the warning is bogus when used with Interlocked API.
#pragma warning disable 420
                if (Interlocked.CompareExchange(ref this._isDisposed, 1, 0) == 0)
#pragma warning restore 420
                {
                    // This should force the creation of the display name as we can't build it after we are disposed
                    this.GetDisplayName();

                    if (this._typeCatalog != null)
                    {
                        this._typeCatalog.Dispose();
                    }

                    if (this._cacheCatalog != null)
                    {
                        this._cacheCatalog.Dispose();
                    }
                }
            }

            base.Dispose(disposing);
        }

        public CachedTypeCatalog(ComposablePartCatalogCache cache)
        {
            Requires.NotNull(cache, "cache");
            ComposablePartCatalog cacheCatalog = cache.GetCacheCatalog(AttributedComposablePartCatalogSite.CreateForReading());

            List<Type> notUpToDateTypes = new List<Type>();
            List<ComposablePartDefinition> upToDateParts = new List<ComposablePartDefinition>();
            foreach (ComposablePartDefinition partDefinition in cacheCatalog.Parts)
            {
                if (!partDefinition.IsPartDefinitionCacheUpToDate())
                {
                    notUpToDateTypes.Add(ReflectionModelServices.GetPartType(partDefinition).Value);
                }
                else
                {
                    upToDateParts.Add(partDefinition);
                }
            }

            if (notUpToDateTypes.Count == 0)
            {
                // everything is up to date, we can use the cached catalog
                this._cacheCatalog = cacheCatalog;
            }
            else
            {
                // some parts are not up-to-date, we will not query the catalog, but rather a combination of the type catalog and up-to-date parts
                this._upToDateParts = upToDateParts;
                this._typeCatalog = new TypeCatalog(notUpToDateTypes);
            }
        }
      
        public override IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                this.ThrowIfDisposed();

                return this.PartsInternal;
            }
        }

        private IQueryable<ComposablePartDefinition> PartsInternal
        {
            get
            {
                if (this._cacheCatalog != null)
                {
                    // catalog fully cached
                    return this._cacheCatalog.Parts;
                }
                else if (this._upToDateParts == null)
                {
                    // catalog is not cached at all
                    Assumes.NotNull(this._typeCatalog);
                    return this._typeCatalog.Parts;
                }
                else
                {
                    // we have a combination of cached and uncached parts
                    Assumes.NotNull(this._typeCatalog);
                    Assumes.NotNull(this._upToDateParts);
                    return this._typeCatalog.Parts.Concat(this._upToDateParts.AsQueryable());
                }
            }
        }

        public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
        {
            this.ThrowIfDisposed();
            Requires.NotNull(definition, "definition");

            if (this._cacheCatalog != null)
            {
                // catalog fully cached
                return this._cacheCatalog.GetExports(definition);
            }
            else if (this._upToDateParts == null)
            {
                // catalog is not cached at all
                Assumes.NotNull(this._typeCatalog);
                return this._typeCatalog.GetExports(definition);
            }
            else
            {
                // we have a combination of cached and uncached parts, we should get along with the Parts-based implementation
                return base.GetExports(definition);
            }            
        }

        public object CacheCatalog(ComposablePartCatalogCacheWriter writer)
        {
            this.ThrowIfDisposed();

            Requires.NotNull(writer, "writer");

            object cacheToken = writer.WriteCache(
                this.GetType(),
                this.Parts,
                null,
                AttributedComposablePartCatalogSite.CreateForWriting());

            writer.WriteRootCacheToken(cacheToken);
            return cacheToken;
        }

        public bool IsCacheUpToDate
        {
            get
            {
                this.ThrowIfDisposed();
                // Every time we were forced to create a type catalog, something is not cached
                return (this._typeCatalog == null);
            }
        }

        string ICompositionElement.DisplayName
        {
            get
            {
                return this.GetDisplayName();
            }
        }

        ICompositionElement ICompositionElement.Origin
        {
            get
            {
                return null;
            }
        }

        private string GetDisplayName()
        {
            // NOTE : we don't need to lock here as we don't much care how many times we build the display name
            // The Memorybarrier guarantees that teh operation of setting the string comes after the full initialization
            if (this._displayName == null)
            {
                string displayName = String.Format(CultureInfo.CurrentCulture,
                                         "{0} (Types=\"{1}\")",     // NOLOC
                                         this.GetType().Name,
                                         GetTypesDisplay());
                Thread.MemoryBarrier();
                this._displayName = displayName;
            }

            return this._displayName;
        }

        private string GetTypesDisplay()
        {
            int count = this.PartsInternal.Count();
            if (count == 0)
            {
                return Strings.CachedTypeCatalog_Empty;
            }

            const int displayCount = 2;
            StringBuilder builder = new StringBuilder();
            foreach (ComposablePartDefinition definition in this.PartsInternal.Take(displayCount))
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(ReflectionModelServices.GetPartType(definition).Value.FullName);
            }

            if (count > displayCount)
            {   // Add an elipse to indicate that there 
                // are more types than actually listed

                builder.Append(", ...");
            }

            return builder.ToString();
        }

        private void ThrowIfDisposed()
        {
            if (this._isDisposed == 1)
            {
                throw new ObjectDisposedException(this.GetType().ToString());
            }
        }
    }
}
