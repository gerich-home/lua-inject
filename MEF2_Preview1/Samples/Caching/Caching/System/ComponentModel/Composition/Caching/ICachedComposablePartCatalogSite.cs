// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;

namespace System.ComponentModel.Composition.Caching
{
#if PUBLIC_CACHING_API
    public 
#else
    internal
#endif
        interface ICachedComposablePartCatalogSite
    {
        // These methods are used during caching to cache the actual objects
        IDictionary<string, object> CacheExportDefinition(ComposablePartDefinition owner, ExportDefinition exportDefinition);
        IDictionary<string, object> CacheImportDefinition(ComposablePartDefinition owner, ImportDefinition importDefinition);
        IDictionary<string, object> CachePartDefinition(ComposablePartDefinition partDefinition);


        // These methods are used when running over an exisitng cache
        ExportDefinition CreateExportDefinitionFromCache(ComposablePartDefinition owner, IDictionary<string, object> cache);
        ImportDefinition CreateImportDefinitionFromCache(ComposablePartDefinition owner, IDictionary<string, object> cache);
        ComposablePartDefinition CreatePartDefinitionFromCache(IDictionary<string, object> cache, Func<ComposablePartDefinition, IEnumerable<ImportDefinition>> importsCreator, Func<ComposablePartDefinition, IEnumerable<ExportDefinition>> exportsCreator);
    }
}
