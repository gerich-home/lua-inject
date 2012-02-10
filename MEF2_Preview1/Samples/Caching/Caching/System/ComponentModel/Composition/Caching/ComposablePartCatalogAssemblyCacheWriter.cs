// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;
using Microsoft.Internal;
using System.Reflection;
using System.Globalization;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition.Caching
{
    internal class ComposablePartCatalogAssemblyCacheWriter : ComposablePartCatalogCacheWriter
    {
        private int _currentCatalogIdentifierCouner = 0;
        private ModuleBuilder _moduleBuilder;
        private AssemblyBuilder _assemblyBuilder;
        private GenerationServices _generationServices;
        private string _rootStubSuffix;
        private string _assemblyCacheFileName;

        public ComposablePartCatalogAssemblyCacheWriter(AssemblyName assemblyName, string cacheDirectory)
        {
            Requires.NotNull(assemblyName, "assemblyName");
            Requires.NotNullOrEmpty(assemblyName.Name, "assemblyName.Name");
            Requires.NotNullOrEmpty(cacheDirectory, "cacheDirectory");

            this._assemblyCacheFileName = assemblyName.Name + ".dll";

            this._assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save, cacheDirectory);
            this._moduleBuilder = this._assemblyBuilder.DefineDynamicModule(assemblyName.Name, this._assemblyCacheFileName);
            this._generationServices = new GenerationServices(this._moduleBuilder);
        }

        protected override object WriteCacheCore(IEnumerable<ComposablePartDefinition> partDefinitions, IDictionary<string, object> catalogMetadata, ICachedComposablePartCatalogSite catalogSite)
        {
            this.ThrowIfDisposed();
            catalogSite = catalogSite ?? new EmptyCachedComposablePartCatalogSite();

            CachingResult result = CachingResult.SucceededResult;
            int currentCatalogIdentifierCounter = this._currentCatalogIdentifierCouner++;
            string currentCatalogIdentifier = string.Format(CultureInfo.InvariantCulture, "{0}", currentCatalogIdentifierCounter);
            AssemblyCacheGenerator generator = new AssemblyCacheGenerator(this._moduleBuilder, this._generationServices, catalogSite, currentCatalogIdentifier);

            generator.BeginGeneration();
            foreach (ComposablePartDefinition partDefinition in partDefinitions)
            {
                result = result.MergeErrors(generator.CachePartDefinition(partDefinition).Errors);
            }
            generator.CacheCatalogMetadata(catalogMetadata);
            CachingResult<Type> stubGenerationResult = generator.EndGeneration();

            result = result.MergeErrors(stubGenerationResult.Errors);
            result.ThrowOnErrors();

            return currentCatalogIdentifier;
        }

        public override void WriteRootCacheToken(object catalogToken)
        {
            this.ThrowIfDisposed();
            Requires.NotNull(catalogToken, "catalogToken");

            string rootStubSuffix = catalogToken as string;
            if (rootStubSuffix == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Strings.InvalidCacheToken, catalogToken.ToString()), "catalogToken");
            }
            this._rootStubSuffix = rootStubSuffix;
        }

        private void GenerateEntryPoint()
        {
            // public static class EntryPoint
            // {
            //    public static string GetRootStub()
            //    {
            //       return <>;
            //    }
            // }

            TypeBuilder entryPointBuilder = this._moduleBuilder.DefineType(
                CacheStructureConstants.EntryPointTypeName,
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Abstract);

            MethodBuilder getRootStubMethod = entryPointBuilder.DefineMethod(
                CacheStructureConstants.EntryPointGetRootStubMethodName,
                MethodAttributes.Static | MethodAttributes.Public,
                typeof(string),
                Type.EmptyTypes);

            ILGenerator ilGenerator = getRootStubMethod.GetILGenerator();
            this._generationServices.LoadValue(ilGenerator, this._rootStubSuffix);
            ilGenerator.Emit(OpCodes.Ret);
            entryPointBuilder.CreateType();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // return if thsi has been already disposed of
                if (this._assemblyBuilder == null)
                {
                    return;
                }

                this.GenerateEntryPoint();
                this._assemblyBuilder.Save(this._assemblyCacheFileName);
                this._assemblyBuilder = null;
                this._moduleBuilder = null;
            }
        }

        private void ThrowIfDisposed()
        {
            if (this._assemblyBuilder == null)
            {
                throw new ObjectDisposedException(this.GetType().ToString());
            }
        }
    }
}
