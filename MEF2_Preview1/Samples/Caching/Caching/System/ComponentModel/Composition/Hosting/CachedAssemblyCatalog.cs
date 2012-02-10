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
        class CachedAssemblyCatalog : ComposablePartCatalog, ICachedComposablePartCatalog, ICompositionElement
    {
        private ComposablePartCatalog _innerCatalog;
        private IDictionary<string, object> _cacheCatalogMetadata = null;
        private bool _isCached = false;
        private volatile int _isDisposed = 0;
        private Assembly _assemblyFromCache = null;
        private bool _useAssemblyIdentity = false;

        public CachedAssemblyCatalog(Assembly assembly)
        {
            this._innerCatalog = new AssemblyCatalog(assembly);
            this._isCached = false;
            this._useAssemblyIdentity = true;
        }

        public CachedAssemblyCatalog(string codeBase)
        {
            this._innerCatalog = new AssemblyCatalog(codeBase);
            this._isCached = false;
            this._useAssemblyIdentity = false;
        }

        public CachedAssemblyCatalog(ComposablePartCatalogCache cache)
        {
            Requires.NotNull(cache, "cache");

            this._useAssemblyIdentity = cache.Metadata.IsAssemblyIdentityStored();
            // if the assembly hasn't been changed since the caching, then load the cache
            if (cache.Metadata.IsAssemblyCacheUpToDate())
            {
                this._cacheCatalogMetadata = cache.Metadata;
                this._innerCatalog = cache.GetCacheCatalog(AttributedComposablePartCatalogSite.CreateForReading(() => this.Assembly));
                this._isCached = true;
            }
            else
            {
                // just load the assembly given the information we have
                this._innerCatalog = new AssemblyCatalog(cache.Metadata.ReadAssembly());
                this._isCached = false;
            }
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
                    this._innerCatalog.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        public override string ToString()
        {
            return ((ICompositionElement)this).DisplayName;
        }

        public Assembly Assembly
        {
            get
            {
                if (this._isCached)
                {
                    if (this._assemblyFromCache == null)
                    {
                        Assumes.NotNull(this._cacheCatalogMetadata);
                        Assembly assemblyFromCache = this._cacheCatalogMetadata.ReadAssembly();
                        Thread.MemoryBarrier();
                        this._assemblyFromCache = assemblyFromCache;
                    }

                    return this._assemblyFromCache;
                }
                else
                {
                    AssemblyCatalog assemblyCatalog = this._innerCatalog as AssemblyCatalog;
                    Assumes.NotNull(assemblyCatalog);
                    return assemblyCatalog.Assembly;
                }
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

            Requires.NotNull(definition, "definition");

            return this._innerCatalog.GetExports(definition);
        }

        public object CacheCatalog(ComposablePartCatalogCacheWriter writer)
        {
            this.ThrowIfDisposed();

            Requires.NotNull(writer, "writer");

            IDictionary<string, object> metadata;
            if (this._isCached)
            {
                Assumes.NotNull(this._cacheCatalogMetadata);
                metadata = this._cacheCatalogMetadata;
            }
            else
            {
                AssemblyCatalog assemblyCatalog = this._innerCatalog as AssemblyCatalog;
                Assumes.NotNull(assemblyCatalog);
                metadata = new Dictionary<string, object>();
                metadata.WriteAssembly(assemblyCatalog.Assembly, this._useAssemblyIdentity);
            }

            object cacheToken = writer.WriteCache(
                this.GetType(),
                this.Parts,
                metadata,
                AttributedComposablePartCatalogSite.CreateForWriting(false));

            writer.WriteRootCacheToken(cacheToken);
            return cacheToken;
        }

        public bool IsCacheUpToDate
        {
            get
            {
                this.ThrowIfDisposed();

                return this._isCached;
            }
        }

        /// <summary>
        ///     Gets the display name of the assembly catalog.
        /// </summary>
        /// <value>
        ///     A <see cref="String"/> containing a human-readable display name of the <see cref="AssemblyCatalog"/>.
        /// </value>
        string ICompositionElement.DisplayName
        {
            get 
            {
                string assemblyName = null;
                if (this._isCached)
                {
                    Assumes.NotNull(this._cacheCatalogMetadata);
                    assemblyName = this._cacheCatalogMetadata.GetAssemblyName();
                }
                else
                {
                    AssemblyCatalog assemblyCatalog = this._innerCatalog as AssemblyCatalog;
                    Assumes.NotNull(assemblyCatalog);
                    assemblyName = assemblyCatalog.Assembly.FullName;
                }

                return string.Format(CultureInfo.CurrentCulture,
                    "{0} (Assembly=\"{1}\")",   // NOLOC
                    this.GetType().Name,
                    assemblyName);
            }
        }

        /// <summary>
        ///     Gets the composition element from which the assembly catalog originated.
        /// </summary>
        /// <value>
        ///     This property always returns <see langword="null"/>.
        /// </value>
        ICompositionElement ICompositionElement.Origin
        {
            get
            {
                return null;
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
