// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.ComponentModel.Composition.Caching
{
    internal static class CacheStructureConstants
    {
        public static string CachingStubTypeNamePrefix = "CachingStub";
        public static string CachingStubGetCatalogIndexMethodName = "GetCatalogIndex";
        public static string CachingStubGetPartFactoryMethodName = "GetPartFactory";
        public static string CachingStubExportDefinitionFactoryFieldName = "ExportDefinitionFactory";
        public static string CachingStubImportDefinitionFactoryFieldName = "ImportDefinitionFactory";
        public static string CachingStubPartDefinitionFactoryFieldName = "PartDefinitionFactory";
        public static string CachingStubCreateExportDefinitionMethodName = "CreateExportDefinition";
        public static string CachingStubCreateImportDefinitionMethodName = "CreateImportDefinition";
        public static string CachingStubCreatePartDefinitionMethodName = "CreatePartDefinition";
        public static string CachingStubGetCatalogMetadata = "GetCatalogMetadata";
        public static string CachingStubGetCatalogIdentifier = "GetCatalogIdentifier";
        public static string PartDefinitionTableNamePrefix = "PartDefinitions";
        public static string ExportsDefinitionTableNamePrefix = "ExportsDefinitions";
        public static string ImportsDefinitionTableNamePrefix = "ImportsDefinitions";
        public static string EntryPointTypeName = "EntryPoint";
        public static string EntryPointGetRootStubMethodName = "GetRootStub";
    }
}
