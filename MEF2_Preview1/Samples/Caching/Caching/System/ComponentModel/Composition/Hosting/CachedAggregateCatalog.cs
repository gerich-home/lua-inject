// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Caching;
using System.ComponentModel.Composition.Caching.AttributedModel;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Hosting
{
#if PUBLIC_CACHING_API
    public 
#else
    internal
#endif
        class CachedAggregateCatalog : ComposablePartCatalog, ICachedComposablePartCatalog, INotifyComposablePartCatalogChanged
    {
        private AggregateCatalog _innerCatalog;
        private volatile int _isDisposed = 0;
        private volatile bool _isChanged = false;
        private bool _readFromCache = false;

        public CachedAggregateCatalog()
        {
            this._innerCatalog = new AggregateCatalog();
            this._innerCatalog.Changed += OnInnerCatalogsChanged;
            this._innerCatalog.Changing += OnInnerCatalogsChanging;
        }

        public CachedAggregateCatalog(IEnumerable<ComposablePartCatalog> catalogs)
        {
            this._innerCatalog = new AggregateCatalog(catalogs);
            this._innerCatalog.Changed += OnInnerCatalogsChanged;
            this._innerCatalog.Changing += OnInnerCatalogsChanging;
        }

        public CachedAggregateCatalog(ComposablePartCatalogCache cache)
        {
            Requires.NotNull(cache, "cache");

            IEnumerable<object> subordinateCacheTokens = cache.Metadata.ReadEnumerable<object>(AttributedCacheServices.CacheKeys.SubordinateTokens);
            List<ComposablePartCatalog> subordinateCatalogs = new List<ComposablePartCatalog>();
            foreach (object subordinateCacheToken in subordinateCacheTokens)
            {
                subordinateCatalogs.Add(cache.Reader.ReadCatalog(subordinateCacheToken));
            }
            this._innerCatalog = new AggregateCatalog(subordinateCatalogs);
            this._readFromCache = true;
            this._innerCatalog.Changed += OnInnerCatalogsChanged;
            this._innerCatalog.Changing += OnInnerCatalogsChanging;
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
                    this._innerCatalog.Changed -= this.OnInnerCatalogsChanged;
                    this._innerCatalog.Changing -= this.OnInnerCatalogsChanging;
                    this._innerCatalog.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        public event EventHandler<ComposablePartCatalogChangeEventArgs> Changed;
        public event EventHandler<ComposablePartCatalogChangeEventArgs> Changing;

        private void OnInnerCatalogsChanged(object sender, ComposablePartCatalogChangeEventArgs e)
        {
            this._isChanged = true;
            this.OnChanged(e);
        }

        protected virtual void OnChanged(ComposablePartCatalogChangeEventArgs e)
        {
            EventHandler<ComposablePartCatalogChangeEventArgs> changedEvent = this.Changed;
            if (changedEvent != null)
            {
                changedEvent(this, e);
            }
        }

        private void OnInnerCatalogsChanging(object sender, ComposablePartCatalogChangeEventArgs e)
        {
            this.OnChanging(e);
        }

        protected virtual void OnChanging(ComposablePartCatalogChangeEventArgs e)
        {
            EventHandler<ComposablePartCatalogChangeEventArgs> changingEvent = this.Changing;
            if (changingEvent != null)
            {
                changingEvent(this, e);
            }
        }

        public ICollection<ComposablePartCatalog> Catalogs
        {
            get
            {
                this.ThrowIfDisposed();

                return this._innerCatalog.Catalogs;
            }
        }

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                this.ThrowIfDisposed();

                return this._innerCatalog.Parts;
            }
        }

        public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
        {
            this.ThrowIfDisposed();

            return this._innerCatalog.GetExports(definition);
        }

        public object CacheCatalog(ComposablePartCatalogCacheWriter writer)
        {
            ThrowIfDisposed();

            Requires.NotNull(writer, "writer");

            List<object> subordinateCacheTokens = new List<object>();
            foreach (ComposablePartCatalog catalog in this.Catalogs)
            {
                ICachedComposablePartCatalog cachedCatalog = catalog as ICachedComposablePartCatalog;
                if (cachedCatalog == null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Strings.CatalogIsNotCacheable, catalog.GetType()));
                }
                object subordinateCacheToken = cachedCatalog.CacheCatalog(writer);
                subordinateCacheTokens.Add(subordinateCacheToken);
            }

            IDictionary<string, object> metadata = new Dictionary<string, object>();
            metadata.WriteEnumerable(AttributedCacheServices.CacheKeys.SubordinateTokens, subordinateCacheTokens);

            object cacheToken = writer.WriteCache(
                this.GetType(),
                null,
                metadata,
                null);

            writer.WriteRootCacheToken(cacheToken);
            return cacheToken;
        }

        public bool IsCacheUpToDate
        {
            get
            {
                this.ThrowIfDisposed();

                if (!this._readFromCache)
                {
                    return false;
                }

                if (this._isChanged)
                {
                    return false;
                }

                foreach (ComposablePartCatalog catalog in this.Catalogs)
                {
                    ICachedComposablePartCatalog cachedCatalog = catalog as ICachedComposablePartCatalog;
                    if ((cachedCatalog == null) || (!cachedCatalog.IsCacheUpToDate))
                    {
                        return false;
                    }
                }
                return true;
            }
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
