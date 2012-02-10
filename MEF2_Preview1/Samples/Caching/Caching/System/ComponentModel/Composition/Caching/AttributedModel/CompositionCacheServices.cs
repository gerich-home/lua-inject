// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Internal;
using System.ComponentModel.Composition.ReflectionModel;
using System.Threading;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition.Caching.AttributedModel
{
    internal class CompositionCacheServices
    {
        public static IDictionary<string, object> WriteExportDefinition(ComposablePartDefinition owner, ExportDefinition exportDefinition)
        {
            Assumes.NotNull(owner);
            Assumes.NotNull(exportDefinition);

            LazyMemberInfo exportingMemberInfo = ReflectionModelServices.GetExportingMember(exportDefinition);

            IDictionary<string, object> cache = new Dictionary<string, object>();
            cache.WriteContractName(exportDefinition.ContractName);
            cache.WriteMetadata(exportDefinition.Metadata);
            cache.WriteValue(AttributedCacheServices.CacheKeys.MemberType, exportingMemberInfo.MemberType, MemberTypes.TypeInfo);
            cache.WriteLazyAccessors(exportingMemberInfo.GetAccessors(), ReflectionModelServices.GetPartType(owner));

            return cache;
        }

        public static ExportDefinition ReadExportDefinition(ComposablePartDefinition owner, IDictionary<string, object> cache)
        {
            Assumes.NotNull(owner);
            Assumes.NotNull(cache);

            LazyMemberInfo exportingMemberInfo = new LazyMemberInfo(
                cache.ReadValue<MemberTypes>(AttributedCacheServices.CacheKeys.MemberType, MemberTypes.TypeInfo),
                cache.ReadLazyAccessors(ReflectionModelServices.GetPartType(owner)));
                

            return ReflectionModelServices.CreateExportDefinition(
                exportingMemberInfo,
                cache.ReadContractName(),
                cache.ReadLazyMetadata(),
                owner as ICompositionElement);
        }


        public static IDictionary<string, object> WriteImportDefinition(ComposablePartDefinition owner, ContractBasedImportDefinition importDefinition)
        {
            Assumes.NotNull(owner);
            Assumes.NotNull(importDefinition);

            Lazy<Type> partType = ReflectionModelServices.GetPartType(owner);

            IDictionary<string, object> cache = new Dictionary<string, object>();
            cache.WriteContractName(importDefinition.ContractName);
            cache.WriteValue(AttributedCacheServices.CacheKeys.RequiredTypeIdentity, importDefinition.RequiredTypeIdentity, null);
            cache.WriteRequiredMetadata(importDefinition.RequiredMetadata);
            cache.WriteValue(AttributedCacheServices.CacheKeys.RequiredCreationPolicy, importDefinition.RequiredCreationPolicy, CreationPolicy.Any);
            cache.WriteValue(AttributedCacheServices.CacheKeys.Cardinality, importDefinition.Cardinality, ImportCardinality.ExactlyOne);
            if (ReflectionModelServices.IsImportingParameter(importDefinition))
            {
                cache.WriteValue(AttributedCacheServices.CacheKeys.ImportType, AttributedCacheServices.ImportTypes.Parameter);
                cache.WriteLazyParameter(
                        ReflectionModelServices.GetImportingParameter(importDefinition),
                        partType);
            }
            else
            {
                // don't write anything for import type - member assumed
                LazyMemberInfo importingMemberInfo = ReflectionModelServices.GetImportingMember(importDefinition);
                cache.WriteValue(AttributedCacheServices.CacheKeys.IsRecomposable, importDefinition.IsRecomposable, false);
                cache.WriteValue(AttributedCacheServices.CacheKeys.MemberType, importingMemberInfo.MemberType , MemberTypes.Property);
                cache.WriteLazyAccessors(
                        importingMemberInfo.GetAccessors(),
                        partType);
            }

            return cache;
        }

        public static ContractBasedImportDefinition ReadImportDefinition(ComposablePartDefinition owner, IDictionary<string, object> cache)
        {
            Assumes.NotNull(owner);
            Assumes.NotNull(cache);
  
            Lazy<Type> partType = ReflectionModelServices.GetPartType(owner);
            ICompositionElement origin = owner as ICompositionElement;
            if (cache.ReadValue<string>(AttributedCacheServices.CacheKeys.ImportType) == AttributedCacheServices.ImportTypes.Parameter)
            {
                return ReflectionModelServices.CreateImportDefinition(
                    cache.ReadLazyParameter(partType),
                    cache.ReadContractName(),
                    cache.ReadValue<string>(AttributedCacheServices.CacheKeys.RequiredTypeIdentity),
                    cache.ReadRequiredMetadata(),
                    cache.ReadValue<ImportCardinality>(AttributedCacheServices.CacheKeys.Cardinality, ImportCardinality.ExactlyOne),
                    cache.ReadValue<CreationPolicy>(AttributedCacheServices.CacheKeys.RequiredCreationPolicy, CreationPolicy.Any),
                    origin);
            }
            else
            {
                LazyMemberInfo importingMemberInfo = new LazyMemberInfo(
                    cache.ReadValue<MemberTypes>(AttributedCacheServices.CacheKeys.MemberType, MemberTypes.Property),
                    cache.ReadLazyAccessors(partType));

                return ReflectionModelServices.CreateImportDefinition(
                    importingMemberInfo,
                    cache.ReadContractName(),
                    cache.ReadValue<string>(AttributedCacheServices.CacheKeys.RequiredTypeIdentity),
                    cache.ReadRequiredMetadata(),
                    cache.ReadValue<ImportCardinality>(AttributedCacheServices.CacheKeys.Cardinality, ImportCardinality.ExactlyOne),
                    cache.ReadValue<bool>(AttributedCacheServices.CacheKeys.IsRecomposable, false),
                    cache.ReadValue<CreationPolicy>(AttributedCacheServices.CacheKeys.RequiredCreationPolicy, CreationPolicy.Any),
                    origin);
            }
        }

        public static IDictionary<string, object> WritePartDefinition(ComposablePartDefinition part, bool writeAssembly)
        {
            Assumes.NotNull(part);

            IDictionary<string, object> cache = new Dictionary<string, object>();
            Lazy<Type> partType = ReflectionModelServices.GetPartType(part);

            cache.WriteMetadata(part.Metadata);
            cache.WriteLazyTypeForPart(partType, writeAssembly);
            cache.WriteValue<bool>(AttributedCacheServices.CacheKeys.IsDisposalRequired, ReflectionModelServices.IsDisposalRequired(part), false);

            return cache;
        }

        public static ComposablePartDefinition ReadPartDefinition(IDictionary<string, object> cache, Func<ComposablePartDefinition, IEnumerable<ImportDefinition>> importsCreator, Func<ComposablePartDefinition, IEnumerable<ExportDefinition>> exportsCreator, Func<Assembly> assemblyLoader)
        {
            Assumes.NotNull(cache);

            Lazy<Type> partType = cache.ReadLazyTypeForPart(assemblyLoader);

            ComposablePartDefinition part = null;
            part = ReflectionModelServices.CreatePartDefinition(
                partType,
                cache.ReadValue<bool>(AttributedCacheServices.CacheKeys.IsDisposalRequired, false),
                LazyServices.MakeLazy(() => importsCreator(part)),
                LazyServices.MakeLazy(() => exportsCreator(part)),
                cache.ReadLazyMetadata(),
                null);

            return part;
        }
        
    }
}
