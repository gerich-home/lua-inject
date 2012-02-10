// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.AttributedModel;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Reflection;
using Microsoft.Internal;
using System.Threading;

namespace System.ComponentModel.Composition.Caching.AttributedModel
{
    /// <summary>
    /// This cached catalog site is used by all catalogs that containe AttributedComposablePartDefinitions
    /// </summary>
    internal partial class AttributedComposablePartCatalogSite : ICachedComposablePartCatalogSite
    {
        private Func<Assembly> AssemblyLoader { get; set; }
        private bool WriteAssemblyForTypes { get; set; }

        public static AttributedComposablePartCatalogSite CreateForWriting()
        {
            return CreateForWriting(true);
        }

        public static AttributedComposablePartCatalogSite CreateForWriting(bool writeAssemblyForTypes)
        {
            return new AttributedComposablePartCatalogSite() { WriteAssemblyForTypes = writeAssemblyForTypes };
        }

        public static AttributedComposablePartCatalogSite CreateForReading()
        {
            return CreateForReading(null);
        }

        public static AttributedComposablePartCatalogSite CreateForReading(Func<Assembly> assemblyLoader)
        {
            return new AttributedComposablePartCatalogSite() { AssemblyLoader = assemblyLoader };
        }

        internal AttributedComposablePartCatalogSite()
        {
            this.AssemblyLoader = null;
            this.WriteAssemblyForTypes = true;
        }

        public IDictionary<string, object> CacheExportDefinition(ComposablePartDefinition owner, ExportDefinition exportDefinition)
        {
            return CompositionCacheServices.WriteExportDefinition(owner, exportDefinition);
        }

        public IDictionary<string, object> CacheImportDefinition(ComposablePartDefinition owner, ImportDefinition importDefinition)
        {
            ContractBasedImportDefinition contractBasedImport = importDefinition as ContractBasedImportDefinition;
            Assumes.NotNull(contractBasedImport);

            return CompositionCacheServices.WriteImportDefinition(owner, contractBasedImport);
        }

        public IDictionary<string, object> CachePartDefinition(ComposablePartDefinition partDefinition)
        {
            ComposablePartDefinition reflectionPartDefition = partDefinition as ComposablePartDefinition;
            Assumes.NotNull(reflectionPartDefition);

            IDictionary<string, object> cache =  CompositionCacheServices.WritePartDefinition(reflectionPartDefition, this.WriteAssemblyForTypes);
            return cache;
        }

        public ExportDefinition CreateExportDefinitionFromCache(ComposablePartDefinition owner, IDictionary<string, object> cache)
        {
            return CompositionCacheServices.ReadExportDefinition(owner, cache);
        }

        public ImportDefinition CreateImportDefinitionFromCache(ComposablePartDefinition owner, IDictionary<string, object> cache)
        {
            return CompositionCacheServices.ReadImportDefinition(owner, cache);
        }

        public ComposablePartDefinition CreatePartDefinitionFromCache(IDictionary<string, object> cache, Func<ComposablePartDefinition, IEnumerable<ImportDefinition>> importsCreator, Func<ComposablePartDefinition, IEnumerable<ExportDefinition>> exportsCreator)
        {
            return CompositionCacheServices.ReadPartDefinition(cache, importsCreator, exportsCreator, this.AssemblyLoader);
        }
    }
}
