// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;

namespace System.ComponentModel.Composition.Caching.AttributedModel
{
    partial class AttributedCacheServices
    {
        internal static class CacheKeys
        {
            public const string ContractName = "ContractName";
            public const string Metadata = "Metadata";
            public const string Cardinality = "Cardinality";
            public const string IsPrerequisite = "IsPrerequisite";
            public const string IsRecomposable = "IsRecomposable";
            public const string RequiresFullyComposedExports = "RequiresFullyComposedExports";
            public const string RequiredTypeIdentity = "RequiredTypeIdentity";
            public const string RequiredMetadataKeys = "RequiredMetadataKeys";
            public const string RequiredMetadataTypes = "RequiredMetadataTypes";
            public const string RequiredCreationPolicy = "RequiredCreationPolicy";
            public const string PartType = "PartType";
            public const string PartConstructor = "PartConstructor";
            public const string Property = "Member";
            public const string Member = "Member";            
            public const string MetadataToken = "MetadataToken";
            public const string Parameter = "Parameter";
            public const string ParameterPosition = "ParameterPosition";
            public const string ParameterConstructor = "ParameterConstructor";  
            public const string Assembly = "Assembly";
            public const string AssemblyLocation = "Location";
            public const string AssemblyTimeStamp = "TimeStamp";
            public const string AssemblyFullName = "FullName";
            public const string Module = "Module";
            public const string SubordinateTokens = "SubordinateTokens";
            public const string SubordinateTimestamps = "SubordinateTimestamps";
            public const string Path = "Path";
            public const string SearchPattern = "SearchPattern";
            public const string MemberType = "MembertType";
            public const string ImportType = "ImportType";
            public const string Accessors = "Accessors";
            public const string IsDisposalRequired = "IsDisposalRequired";
        }

        internal static class ImportTypes
        {
            public const string Parameter = "Parameter";
            public const string Member = "Member";
        }
    }
}
