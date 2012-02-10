// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Hosting
{
    public partial class CompositionContainer : ExportProvider, ICompositionService, IDisposable
    {
        private readonly bool _isThreadSafe;
        private ImportEngine _importEngine;
        private ComposablePartExportProvider _partExportProvider;
        private AggregateExportProvider _aggregatingExportProvider;
        private ExportProvider _rootProvider;
        private CatalogExportProvider _catalogExportProvider;
        private readonly ReadOnlyCollection<ExportProvider> _providers;
        private volatile bool _isDisposed = false;
        private object _lock = new object();
        private static ReadOnlyCollection<ExportProvider> EmptyProviders = new ReadOnlyCollection<ExportProvider>(new ExportProvider[]{});

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositionContainer"/> class.
        /// </summary>
        public CompositionContainer()
            : this((ComposablePartCatalog)null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositionContainer"/> class 
        ///     with the specified export providers.
        /// </summary>
        /// <param name="providers">
        ///     A <see cref="Array"/> of <see cref="ExportProvider"/> objects which provide 
        ///     the <see cref="CompositionContainer"/> access to <see cref="Export"/> objects,
        ///     or <see langword="null"/> to set <see cref="Providers"/> to an empty
        ///     <see cref="ReadOnlyCollection{T}"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     <paramref name="providers"/> contains an element that is <see langword="null"/>.
        /// </exception>
        public CompositionContainer(params ExportProvider[] providers) : 
            this((ComposablePartCatalog)null, providers)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositionContainer"/> class 
        ///     with the specified catalog and export providers.
        /// </summary>
        /// <param name="providers">
        ///     A <see cref="Array"/> of <see cref="ExportProvider"/> objects which provide 
        ///     the <see cref="CompositionContainer"/> access to <see cref="Export"/> objects,
        ///     or <see langword="null"/> to set <see cref="Providers"/> to an empty 
        ///     <see cref="ReadOnlyCollection{T}"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     <paramref name="providers"/> contains an element that is <see langword="null"/>.
        /// </exception>
        public CompositionContainer(ComposablePartCatalog catalog, params ExportProvider[] providers): 
            this(catalog, false, providers)
        {
        }

        public CompositionContainer(ComposablePartCatalog catalog, bool isThreadSafe, params ExportProvider[] providers)
        {
            this._isThreadSafe = isThreadSafe;

            this._partExportProvider = new ComposablePartExportProvider(isThreadSafe);
            this._partExportProvider.SourceProvider = this;

            // the count of all aggregrated providers = 1 (Part EP) + 1 (Catalog EP, if present) + count of given providers;
            int reservedProvidersLength = 1 + ((catalog != null) ? 1 : 0);
            int aggregatedProvidersLength = reservedProvidersLength + ((providers != null) ? providers.Length : 0);

            if (aggregatedProvidersLength > 1)
            {
                ExportProvider[] aggregatedProviders = new ExportProvider[aggregatedProvidersLength];

                // Add the parts provider
                aggregatedProviders[0] = this._partExportProvider;

                // Add the catalog provider
                if (catalog != null)
                {
                    this._catalogExportProvider = new CatalogExportProvider(catalog, isThreadSafe);
                    this._catalogExportProvider.SourceProvider = this;
                    aggregatedProviders[1] = this._catalogExportProvider;
                }

                // Add the rest of providers
                if ((providers != null) && (providers.Length > 0))
                {
                    Array.Copy(providers, 0, aggregatedProviders, reservedProvidersLength, providers.Length);
                }

                // Create the aggregating provider and if any of the aggregatedProviders is null the constructor will throw an ArgumentExeption
                this._aggregatingExportProvider = new AggregateExportProvider(aggregatedProviders);
                this._rootProvider = this._aggregatingExportProvider;
            }
            else
            {
                Assumes.IsTrue(aggregatedProvidersLength == 1);
                this._rootProvider = this._partExportProvider;
            }

            this._rootProvider.ExportsChanged += this.OnExportsChangedInternal;
            this._rootProvider.ExportsChanging += this.OnExportsChangingInternal;

            this._providers = (providers != null) ? new ReadOnlyCollection<ExportProvider>((ExportProvider[])providers.Clone()) : EmptyProviders;
        }

        /// <summary>
        ///     Gets the catalog which provides the container access to exports produced
        ///     from composable parts.
        /// </summary>
        /// <value>
        ///     The <see cref="ComposablePartCatalog"/> which provides the 
        ///     <see cref="CompositionContainer"/> access to exports produced from
        ///     <see cref="ComposablePart"/> objects. The default is <see langword="null"/>.
        /// </value>
        /// <exception cref="ObjectDisposedException">
        ///     The <see cref="CompositionContainer"/> has been disposed of.
        /// </exception>
        public ComposablePartCatalog Catalog
        {
            get 
            {
                ThrowIfDisposed();
                return (this._catalogExportProvider != null) ? this._catalogExportProvider.Catalog : null;
            }
        }

        /// <summary>
        ///     Gets the export providers which provide the container access to additional exports.
        /// </summary>
        /// <value>
        ///     A <see cref="ReadOnlyCollection{T}"/> of <see cref="ExportProvider"/> objects
        ///     which provide the <see cref="CompositionContainer"/> access to additional
        ///     <see cref="Export"/> objects. The default is an empty 
        ///     <see cref="ReadOnlyCollection{T}"/>.
        /// </value>
        /// <exception cref="ObjectDisposedException">
        ///     The <see cref="CompositionContainer"/> has been disposed of.
        /// </exception>
        public ReadOnlyCollection<ExportProvider> Providers
        {
            get
            {
                this.ThrowIfDisposed();
                Contract.Ensures(Contract.Result<ReadOnlyCollection<ExportProvider>>() != null);

                return this._providers;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!this._isDisposed)
                {
                    ExportProvider rootProvider = null;
                    AggregateExportProvider aggregatingExportProvider = null;
                    ComposablePartExportProvider partExportProvider = null;
                    CatalogExportProvider catalogExportProvider = null;
                    ImportEngine importEngine = null;

                    lock (this._lock)
                    {
                        if (!this._isDisposed)
                        {
                            rootProvider = this._rootProvider;
                            this._rootProvider = null;

                            aggregatingExportProvider = this._aggregatingExportProvider;
                            this._aggregatingExportProvider = null;

                            partExportProvider = this._partExportProvider;
                            this._partExportProvider = null;

                            catalogExportProvider = this._catalogExportProvider;
                            this._catalogExportProvider = null;

                            importEngine = this._importEngine;
                            this._importEngine = null;

                            this._isDisposed = true;
                        }
                    }

                    if (rootProvider != null)
                    {
                        rootProvider.ExportsChanged -= this.OnExportsChangedInternal;
                        rootProvider.ExportsChanging -= this.OnExportsChangingInternal;
                    }

                    if (aggregatingExportProvider != null)
                    {
                        aggregatingExportProvider.Dispose();
                    }

                    if (catalogExportProvider != null)
                    {
                        catalogExportProvider.Dispose();
                    }

                    if (partExportProvider != null)
                    {
                        partExportProvider.Dispose();
                    }

                    if (importEngine != null)
                    {
                        importEngine.Dispose();
                    }
                }

            }
        }
  
        public void Compose(CompositionBatch batch)
        {
            Requires.NotNull(batch, "batch");

            this.ThrowIfDisposed();
            this._partExportProvider.Compose(batch);
        }

        /// <summary>
        ///     Releases the <see cref="Export"/> from the <see cref="CompositionContainer"/>. The behavior
        ///     may vary depending on the implementation of the <see cref="ExportProvider"/> that produced 
        ///     the <see cref="Export"/> instance. As a general rule non shared exports should be early 
        ///     released causing them to be detached from the container.
        ///
        ///     For example the <see cref="CatalogExportProvider"/> will only release 
        ///     an <see cref="Export"/> if it comes from a <see cref="ComposablePart"/> that was constructed
        ///     under a <see cref="CreationPolicy.NonShared" /> context. Release in this context means walking
        ///     the dependency chain of the <see cref="Export"/>s, detaching references from the container and 
        ///     calling Dispose on the <see cref="ComposablePart"/>s as needed. If the <see cref="Export"/> 
        ///     was constructed under a <see cref="CreationPolicy.Shared" /> context the 
        ///     <see cref="CatalogExportProvider"/> will do nothing as it may be in use by other requestors. 
        ///     Those will only be detached when the container is itself disposed.
        /// </summary>
        /// <param name="export"><see cref="Export"/> that needs to be released.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="export"/> is <see langword="null"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Performance", "CA1822")]
        public void ReleaseExport(Export export)
        {
            Requires.NotNull(export, "export");

            IDisposable dependency = export as IDisposable;

            if (dependency != null)
            {
                dependency.Dispose();
            }
        }

        /// <summary>
        ///     Releases the <see cref="Lazy{T}"/> from the <see cref="CompositionContainer"/>. The behavior
        ///     may vary depending on the implementation of the <see cref="ExportProvider"/> that produced 
        ///     the <see cref="Export"/> instance. As a general rule non shared exports should be early 
        ///     released causing them to be detached from the container.
        ///
        ///     For example the <see cref="CatalogExportProvider"/> will only release 
        ///     an <see cref="Lazy{T}"/> if it comes from a <see cref="ComposablePart"/> that was constructed
        ///     under a <see cref="CreationPolicy.NonShared" /> context. Release in this context means walking
        ///     the dependency chain of the <see cref="Export"/>s, detaching references from the container and 
        ///     calling Dispose on the <see cref="ComposablePart"/>s as needed. If the <see cref="Export"/> 
        ///     was constructed under a <see cref="CreationPolicy.Shared" /> context the 
        ///     <see cref="CatalogExportProvider"/> will do nothing as it may be in use by other requestors. 
        ///     Those will only be detached when the container is itself disposed.
        /// </summary>
        /// <param name="export"><see cref="Export"/> that needs to be released.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="export"/> is <see langword="null"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Performance", "CA1822")]
        public void ReleaseExport<T>(Lazy<T> export)
        {
            Requires.NotNull(export, "export");

            IDisposable dependency = export as IDisposable;

            if (dependency != null)
            {
                dependency.Dispose();
            }
        }

        /// <summary>
        ///     Releases a set of <see cref="Export"/>s from the <see cref="CompositionContainer"/>. 
        ///     See also <see cref="ReleaseExport"/>.
        /// </summary>
        /// <param name="exports"><see cref="Export"/>s that need to be released.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="exports"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="exports"/> contains an element that is <see langword="null"/>.
        /// </exception>
        public void ReleaseExports(IEnumerable<Export> exports)
        {
            Requires.NotNullOrNullElements(exports, "exports");

            foreach (Export export in exports)
            {
                this.ReleaseExport(export);
            }
        }

        /// <summary>
        ///     Releases a set of <see cref="Export"/>s from the <see cref="CompositionContainer"/>. 
        ///     See also <see cref="ReleaseExport"/>.
        /// </summary>
        /// <param name="exports"><see cref="Export"/>s that need to be released.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="exports"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="exports"/> contains an element that is <see langword="null"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public void ReleaseExports<T>(IEnumerable<Lazy<T>> exports)
        {
            Requires.NotNullOrNullElements(exports, "exports");

            foreach (Lazy<T> export in exports)
            {
                this.ReleaseExport(export);
            }
        }

        /// <summary>
        ///     Releases a set of <see cref="Export"/>s from the <see cref="CompositionContainer"/>. 
        ///     See also <see cref="ReleaseExport"/>.
        /// </summary>
        /// <param name="exports"><see cref="Export"/>s that need to be released.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="exports"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="exports"/> contains an element that is <see langword="null"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public void ReleaseExports<T, TMetadataView>(IEnumerable<Lazy<T, TMetadataView>> exports)
        {
            Requires.NotNullOrNullElements(exports, "exports");

            foreach (Lazy<T, TMetadataView> export in exports)
            {
                this.ReleaseExport(export);
            }
        }

        /// <summary>
        ///     Sets the imports of the specified composable part exactly once and they will not
        ///     ever be recomposed.
        /// </summary>
        /// <param name="part">
        ///     The <see cref="ComposablePart"/> to set the imports.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="part"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="CompositionException">
        ///     An error occurred during composition. <see cref="CompositionException.Errors"/> will
        ///     contain a collection of errors that occurred.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     The <see cref="ICompositionService"/> has been disposed of.
        /// </exception>
        public void SatisfyImportsOnce(ComposablePart part)
        {
            this.ThrowIfDisposed();

            if (this._importEngine == null)
            {
                ImportEngine importEngine = new ImportEngine(this, this._isThreadSafe);

                lock (this._lock)
                {
                    if (this._importEngine == null)
                    {
                        Thread.MemoryBarrier();
                        this._importEngine = importEngine;
                        importEngine = null;
                    }
                }

                if (importEngine != null)
                {
                    importEngine.Dispose();
                }
            }

            this._importEngine.SatisfyImportsOnce(part);
        }

        internal void OnExportsChangedInternal(object sender, ExportsChangeEventArgs e)
        {
            this.OnExportsChanged(e);
        }

        internal void OnExportsChangingInternal(object sender, ExportsChangeEventArgs e)
        {
            this.OnExportsChanging(e);
        }

        /// <summary>
        /// Returns all exports that match the conditions of the specified import.
        /// </summary>
        /// <param name="definition">The <see cref="ImportDefinition"/> that defines the conditions of the
        /// <see cref="Export"/> to get.</param>
        /// <returns></returns>
        /// <result>
        /// An <see cref="IEnumerable{T}"/> of <see cref="Export"/> objects that match
        /// the conditions defined by <see cref="ImportDefinition"/>, if found; otherwise, an
        /// empty <see cref="IEnumerable{T}"/>.
        /// </result>
        /// <remarks>
        /// 	<note type="inheritinfo">
        /// The implementers should not treat the cardinality-related mismatches as errors, and are not
        /// expected to throw exceptions in those cases.
        /// For instance, if the import requests exactly one export and the provider has no matching exports or more than one,
        /// it should return an empty <see cref="IEnumerable{T}"/> of <see cref="Export"/>.
        /// </note>
        /// </remarks>
        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            this.ThrowIfDisposed();

            IEnumerable<Export> exports = null;
            this._rootProvider.TryGetExports(definition, atomicComposition, out exports);

            return exports;
        }

        [DebuggerStepThrough]
        [ContractArgumentValidator]
        [SuppressMessage("Microsoft.Contracts", "CC1053", Justification = "Suppressing warning because this validator has no public contract")]
        private void ThrowIfDisposed()
        {
            if (this._isDisposed)
            {
                throw ExceptionBuilder.CreateObjectDisposed(this);
            }
        }
    }
}
