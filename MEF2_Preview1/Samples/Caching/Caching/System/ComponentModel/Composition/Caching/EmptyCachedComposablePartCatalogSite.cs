// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Caching
{
    /// <summary>
    /// This is used by catalogs that don't actually have any parts, but only metadata.
    /// We use this type when "null" is passed for the site.
    /// </summary>
    internal class EmptyCachedComposablePartCatalogSite : ICachedComposablePartCatalogSite
    {
        public IDictionary<string, object> CacheExportDefinition(ComposablePartDefinition owner, ExportDefinition exportDefinition)
        {
            throw new NotSupportedException();
        }

        public IDictionary<string, object> CacheImportDefinition(ComposablePartDefinition owner, ImportDefinition importDefinition)
        {
            throw new NotSupportedException();
        }

        public IDictionary<string, object> CachePartDefinition(ComposablePartDefinition partDefinition)
        {
            throw new NotSupportedException();
        }

        public ExportDefinition CreateExportDefinitionFromCache(ComposablePartDefinition owner, IDictionary<string, object> cache)
        {
            throw new NotSupportedException();
        }

        public ImportDefinition CreateImportDefinitionFromCache(ComposablePartDefinition owner, IDictionary<string, object> cache)
        {
            throw new NotSupportedException();
        }

        public ComposablePartDefinition CreatePartDefinitionFromCache(IDictionary<string, object> cache, Func<ComposablePartDefinition, IEnumerable<ImportDefinition>> importsCreator, Func<ComposablePartDefinition, IEnumerable<ExportDefinition>> exportsCreator)
        {
            throw new NotSupportedException();
        }
    }
}
