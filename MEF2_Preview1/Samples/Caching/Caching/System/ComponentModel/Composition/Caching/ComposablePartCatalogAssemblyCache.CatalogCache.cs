// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using Microsoft.Internal;
using System.IO;
using System.Reflection;
using System.Linq.Expressions;
using System.Globalization;

namespace System.ComponentModel.Composition.Caching
{
    internal partial class ComposablePartCatalogAssemblyCache : ComposablePartCatalogCache
    {
        private class CatalogCache : ComposablePartCatalog
        {
            private Type _stubType;
            private ICachedComposablePartCatalogSite _cachedCatalogSite;
            private IQueryable<ComposablePartDefinition> _parts;
            private IDictionary<string, IEnumerable<IntPtr>> _catalogIndex;
            private IDictionary<string, ComposablePartDefinition[]> _materializedIndex = new Dictionary<string, ComposablePartDefinition[]>();
            private Func<IntPtr, ComposablePartDefinition> _getPartFactory;
            private IDictionary<IntPtr, ComposablePartDefinition> _createdPartDefinitions = new Dictionary<IntPtr, ComposablePartDefinition>();

            private static Func<ComposablePartDefinition, IEnumerable<ImportDefinition>> _emptyImportCreator = delegate
            {
                return Enumerable.Empty<ImportDefinition>();
            };

            private static Func<ComposablePartDefinition, IEnumerable<ExportDefinition>> _emptyExportCreator = delegate
            {
                return Enumerable.Empty<ExportDefinition>();
            };


            public CatalogCache(Type stubType, ICachedComposablePartCatalogSite cachedCatalogSite)
            {
                Assumes.NotNull(stubType);

                cachedCatalogSite = cachedCatalogSite ?? new EmptyCachedComposablePartCatalogSite();
                this._stubType = stubType;
                this._cachedCatalogSite = cachedCatalogSite;

                // Get catalog index
                MethodInfo getIndexMethod = ComposablePartCatalogAssemblyCacheReader.GetCacheTypeMethod(this._stubType, CacheStructureConstants.CachingStubGetCatalogIndexMethodName, BindingFlags.Public | BindingFlags.Static);
                Func<IDictionary<string, IEnumerable<IntPtr>>> getIndex = (Func<IDictionary<string, IEnumerable<IntPtr>>>)Delegate.CreateDelegate(typeof(Func<IDictionary<string, IEnumerable<IntPtr>>>), getIndexMethod);
                this._catalogIndex = getIndex.Invoke();

                // Get part factory
                MethodInfo getPartFactoryMethod = ComposablePartCatalogAssemblyCacheReader.GetCacheTypeMethod(this._stubType, CacheStructureConstants.CachingStubGetPartFactoryMethodName, BindingFlags.Static | BindingFlags.Public);
                this._getPartFactory = (Func<IntPtr, ComposablePartDefinition>)Delegate.CreateDelegate(typeof(Func<IntPtr, ComposablePartDefinition>), getPartFactoryMethod);

                // get fields
                FieldInfo importDefinitionFactoryField = ComposablePartCatalogAssemblyCacheReader.GetCacheTypeField(this._stubType, CacheStructureConstants.CachingStubImportDefinitionFactoryFieldName, BindingFlags.Public | BindingFlags.Static);
                FieldInfo exportDefinitionFactoryField = ComposablePartCatalogAssemblyCacheReader.GetCacheTypeField(this._stubType, CacheStructureConstants.CachingStubExportDefinitionFactoryFieldName, BindingFlags.Public | BindingFlags.Static);
                FieldInfo partDefinitionFactoryField = ComposablePartCatalogAssemblyCacheReader.GetCacheTypeField(this._stubType, CacheStructureConstants.CachingStubPartDefinitionFactoryFieldName, BindingFlags.Public | BindingFlags.Static);

                // initialize the cache fields with the factory methods
                importDefinitionFactoryField.SetValue(null, new Func<ComposablePartDefinition, IDictionary<string, object>, ImportDefinition>(this._cachedCatalogSite.CreateImportDefinitionFromCache));
                exportDefinitionFactoryField.SetValue(null, new Func<ComposablePartDefinition, IDictionary<string, object>, ExportDefinition>(this._cachedCatalogSite.CreateExportDefinitionFromCache));
                partDefinitionFactoryField.SetValue(null, new Func<IDictionary<string, object>, Func<ComposablePartDefinition, IEnumerable<ImportDefinition>>, Func<ComposablePartDefinition, IEnumerable<ExportDefinition>>, ComposablePartDefinition>(this.InternalCreatePartDefinitionFromCache));
            }

            private ComposablePartDefinition InternalCreatePartDefinitionFromCache(IDictionary<string, object> cache, Func<ComposablePartDefinition, IEnumerable<ImportDefinition>> importsCreator, Func<ComposablePartDefinition, IEnumerable<ExportDefinition>> exportsCreator)
            {
                return this._cachedCatalogSite.CreatePartDefinitionFromCache(
                    cache,
                    importsCreator ?? _emptyImportCreator,
                    exportsCreator ?? _emptyExportCreator);
            }

            public override IQueryable<ComposablePartDefinition> Parts
            {
                get
                {
                    if (this._parts == null)
                    {
                        // get the flattened index - we would get all parts that define any exports
                        IEnumerable<IntPtr> partDefinitionFuncPtrs = this._catalogIndex.Values.SelectMany(ptrs => ptrs).Distinct();
                        ComposablePartDefinition[] parts = partDefinitionFuncPtrs.Select(funcPtr => this.GetPartDefinition(funcPtr)).ToArray();

                        this._parts = parts.AsQueryable();
                    }
                    return this._parts;
                }
            }

            private ComposablePartDefinition GetPartDefinition(IntPtr funcPtr)
            {
                ComposablePartDefinition partDefinition = null;
                if (!this._createdPartDefinitions.TryGetValue(funcPtr, out partDefinition))
                {
                    partDefinition = this._getPartFactory.Invoke(funcPtr);
                    this._createdPartDefinitions.Add(funcPtr, partDefinition);
                }
                return partDefinition;
            }

            public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
            {
                Requires.NotNull(definition, "definition");
                ContractBasedImportDefinition contractBasedDefinition = definition as ContractBasedImportDefinition;
                if (contractBasedDefinition != null)
                {
                    return this.GetExports(contractBasedDefinition);
                }
                else
                {
                    return base.GetExports(definition);
                }
            }


            private IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ContractBasedImportDefinition definition)
            {
                ComposablePartDefinition[] matchingParts = this.GetPartsWithContract(definition.ContractName);
                if (matchingParts == null)
                {
                    return Enumerable.Empty<Tuple<ComposablePartDefinition, ExportDefinition>>();
                }
                else
                {
                    var exports = new List<Tuple<ComposablePartDefinition, ExportDefinition>>();
                    foreach (var part in matchingParts)
                    {
                        foreach (var export in part.ExportDefinitions)
                        {
                            if (definition.IsConstraintSatisfiedBy(export))
                            {
                                exports.Add(new Tuple<ComposablePartDefinition, ExportDefinition>(part, export));
                            }
                        }
                    }
                    return exports;
                }
            }

            private ComposablePartDefinition[] GetPartsWithContract(string contractName)
            {
                Assumes.NotNull(contractName);
                ComposablePartDefinition[] matchingParts = null;
                if (!this._materializedIndex.TryGetValue(contractName, out matchingParts))
                {
                    IEnumerable<IntPtr> matchingPartPtrs;
                    if (this._catalogIndex.TryGetValue(contractName, out matchingPartPtrs))
                    {
                        matchingParts = matchingPartPtrs.Select(ptr => this.GetPartDefinition(ptr)).ToArray();
                    }
                    this._materializedIndex.Add(contractName, matchingParts);
                }
                return matchingParts;
            }
        }
    }
}
