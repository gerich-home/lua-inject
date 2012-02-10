// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using Microsoft.Internal;
using System.Globalization;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition.Caching
{
    internal class AssemblyCacheGenerator
    {
        private string _catalogIdentifier;
        private IDictionary<string, object> _catalogMetadata;
        private ModuleBuilder _moduleBuilder;
        private GenerationServices _generationServices;
        private ICachedComposablePartCatalogSite _cachedCatalogSite;
        private TypeBuilder _stubBuilder;
        private MethodBuilder _createImportDefinitionMethod;
        private MethodBuilder _createExportDefinitionMethod;
        private MethodBuilder _createPartDefinitionMethod;
        private MethodBuilder _getCatalogMetadata;
        private MethodBuilder _getCatalogIndex;
        private TypeBuilder _partsDefinitionBuilder;
        private TypeBuilder _exportsDefinitionBuilder;
        private TypeBuilder _importsDefinitionBuilder;
        private bool _isGenerationStarted = false;
        private bool _isGenerationCompleted = false;
        private int _partsCounter = 0;
        private IDictionary<string, List<MethodInfo>> _catalogIndex;

        private static ConstructorInfo _importsFactoryDelegateConstructor = typeof(Func<ComposablePartDefinition, IEnumerable<ImportDefinition>>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });
        private static ConstructorInfo _exportsFactoryDelegateConstructor = typeof(Func<ComposablePartDefinition, IEnumerable<ExportDefinition>>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });
        
        private static Type _stubExportDefinitionFactoryType = typeof(Func<ComposablePartDefinition, IDictionary<string, object>, ExportDefinition>);
        private static MethodInfo _stubExportDefinitionFactoryInvoke = _stubExportDefinitionFactoryType.GetMethod("Invoke");
        private static Type _stubImportDefinitionFactoryType = typeof(Func<ComposablePartDefinition, IDictionary<string, object>, ImportDefinition>);
        private static MethodInfo _stubImportDefinitionFactoryInvoke = _stubImportDefinitionFactoryType.GetMethod("Invoke");
        private static Type _stubPartDefinitionFactoryType = typeof(Func<IDictionary<string, object>, Func<ComposablePartDefinition, IEnumerable<ImportDefinition>>, Func<ComposablePartDefinition, IEnumerable<ExportDefinition>>, ComposablePartDefinition>);
        private static MethodInfo _stubPartDefinitionFactoryInvoke = _stubPartDefinitionFactoryType.GetMethod("Invoke");
        private static Type _catalogIndexDictionaryType = typeof(Dictionary<string, IEnumerable<IntPtr>>);
        private static ConstructorInfo _catalogIndexDictionaryConstructor = _catalogIndexDictionaryType.GetConstructor(Type.EmptyTypes);
        private static MethodInfo _catalogIndexDictionaryAddMethod = _catalogIndexDictionaryType.GetMethod("Add", new Type[] {typeof(string), typeof(IEnumerable<IntPtr>)});

        private static Type _importDefinitionType = typeof(ImportDefinition);
        private static Type _exportDefinitionType = typeof(ExportDefinition);

        private static Type _standardIDictionaryType = typeof(IDictionary<string, object>);
        private static Type _composablePartDefinitionType = typeof(ComposablePartDefinition);
        private static FieldInfo _IntPtr_Zero = typeof(IntPtr).GetField("Zero");


        public AssemblyCacheGenerator(ModuleBuilder moduleBuilder, GenerationServices generationServices, ICachedComposablePartCatalogSite cachedCatalogSite, string catalogIdentifier)
        {
            Assumes.NotNull(moduleBuilder);
            Assumes.NotNull(generationServices);
            Assumes.NotNull(cachedCatalogSite);
            Assumes.NotNull(catalogIdentifier);

            this._moduleBuilder = moduleBuilder;
            this._generationServices = generationServices;
            this._catalogIdentifier = catalogIdentifier ?? string.Empty;
            this._catalogMetadata = new Dictionary<string, object>();
            this._catalogIndex = new Dictionary<string, List<MethodInfo>>();
            this._cachedCatalogSite = cachedCatalogSite;
        }

        internal ModuleBuilder ModuleBuilder
        {
            get
            {
                return this._moduleBuilder;
            }
        }

        internal ICachedComposablePartCatalogSite CatalogSite
        {
            get
            {
                return this._cachedCatalogSite;
            }
        }

        internal string CatalogIdentifier
        {
            get
            {
                return this._catalogIdentifier;
            }
        }

        public void BeginGeneration()
        {
            Assumes.IsFalse(this._isGenerationStarted);
            Assumes.IsFalse(this._isGenerationCompleted);
            this._isGenerationStarted = true;

            this.GenerateCachingStub();
            this.GenerateTables();

            Assumes.NotNull(this._stubBuilder);
            Assumes.NotNull(this._createPartDefinitionMethod);
            Assumes.NotNull(this._createExportDefinitionMethod);
            Assumes.NotNull(this._createImportDefinitionMethod);

            Assumes.NotNull(this._partsDefinitionBuilder);
            Assumes.NotNull(this._exportsDefinitionBuilder);
            Assumes.NotNull(this._importsDefinitionBuilder);
        }

        public CachingResult<Type> EndGeneration()
        {
            Assumes.IsTrue(this._isGenerationStarted);
            Assumes.IsFalse(this._isGenerationCompleted);
            CachingResult result = CachingResult.SucceededResult;

            result = result.MergeResult(this.GenerateGetCatalogMetadata());
            result = result.MergeResult(this.GenerateGetCatalogIndex());
            Type stubType = this._stubBuilder.CreateType();
            this._partsDefinitionBuilder.CreateType();
            this._exportsDefinitionBuilder.CreateType();
            this._importsDefinitionBuilder.CreateType();

            this._isGenerationCompleted = true;
            return result.ToResult<Type>(stubType);
        }

        private void GenerateTables()
        {
            this._partsDefinitionBuilder = this._moduleBuilder.DefineType(
                string.Format(CultureInfo.InvariantCulture, "{0}{1}", CacheStructureConstants.PartDefinitionTableNamePrefix, this._catalogIdentifier),
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Abstract);

            this._exportsDefinitionBuilder = this._moduleBuilder.DefineType(
                string.Format(CultureInfo.InvariantCulture, "{0}{1}", CacheStructureConstants.ExportsDefinitionTableNamePrefix, this._catalogIdentifier),
                TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.Abstract);

            this._importsDefinitionBuilder = this._moduleBuilder.DefineType(
                string.Format(CultureInfo.InvariantCulture, "{0}{1}", CacheStructureConstants.ImportsDefinitionTableNamePrefix, this._catalogIdentifier),
                TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.Abstract);

        }

        private void GenerateCachingStub()
        {
            //
            // This stub is injected into all caches. The creation methods call into it to create actual instances of parts, exports and imports
            // The cacher reade injects the delegates into its fields.
            //
            //public class CachingStub
            //{
            //    // creates the export from the dictionary
            //    public static Func<ComposablePartDefinition, IDictionary<string, object>, ExportDefinition> ExportDefinitionFactory;
            //    // creates the import from the dictionary
            //    public static Func<ComposablePartDefinition, IDictionary<string, object>, ImportDefinition> ImportDefinitionFactory;
            //    // creates the part definition from the dictionary. 
            //    public static Func<IDictionary<string, object>, Func<ComposablePartDefinition, IEnumerable<ImportDefinition>>, Func<ComposablePartDefinition, IEnumerable<ExportDefinition>>, ComposablePartDefinition> PartDefinitionFactory;
            //
            //    internal static ExportDefinition CreateExportDefinition(ComposablePartDefinition owner, IDictionary<string, object> cache)
            //    {
            //        return CachingStub.ExportDefinitionFactory.Invoke(owner, cache);
            //    }
            //
            //    internal static ImportDefinition CreateImportDefinition(ComposablePartDefinition owner, IDictionary<string, object> cache)
            //    {
            //        return CachingStub.ImportDefinitionFactory.Invoke(owner, cache);
            //    }
            //
            //    internal static ComposablePartDefinition CreatePartDefinition(IDictionary<string, object> cache, IntPtr importsFactory, IntPtr exportsFactory)
            //    {
            //        return CachingStub.PartDefinitionFactory.Invoke(
            //             cache, 
            //             new Func<ComposablePartDefinition, IEnumerable<ImportDefinition>>(null, importsFactoryPtr),
            //             new Func<ComposablePartDefinition, IEnumerable<ExportDefinition>>(null, exportssFactoryPtr));
            //    }
            //    

            //    public static IDictionary<string, IEnemerable<IntPtr>> GetCatalogIndex()
            //    {
            //         Dictionary<string, IEnemerable<IntPtr>> index = new Dictionary<string, IEnemerable<IntPtr>>();
            //         index.Add(contractX, new IntPtr[] {partFactoryPtr1, partFactoryPtr2...}
            //   
            //    public static IDictionary<string, object> GetCatalogMetadata()
            //    {
            //       return <metadata>;
            //    }
            //
            //    public static string GetCatalogIdentifier()
            //    {
            //       return <id>;
            //    }
            //}

            // define type
            this._stubBuilder = this._moduleBuilder.DefineType(
                string.Format(CultureInfo.InvariantCulture, "{0}{1}", CacheStructureConstants.CachingStubTypeNamePrefix, this._catalogIdentifier),
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Abstract);

            // define fields
            FieldBuilder exportDefinitionFactoryFieldBuilder = this._stubBuilder.DefineField(
                CacheStructureConstants.CachingStubExportDefinitionFactoryFieldName,
                AssemblyCacheGenerator._stubExportDefinitionFactoryType,
                FieldAttributes.Static | FieldAttributes.Public
                );

            FieldBuilder importDefinitionFactoryFieldBuilder = this._stubBuilder.DefineField(
                CacheStructureConstants.CachingStubImportDefinitionFactoryFieldName,
                AssemblyCacheGenerator._stubImportDefinitionFactoryType,
                FieldAttributes.Static | FieldAttributes.Public
                );

            FieldBuilder partDefinitionFactoryFieldBuilder = this._stubBuilder.DefineField(
                CacheStructureConstants.CachingStubPartDefinitionFactoryFieldName,
                AssemblyCacheGenerator._stubPartDefinitionFactoryType,
                FieldAttributes.Static | FieldAttributes.Public
                );


            //
            // define calling method per field
            //
            ILGenerator ilGenerator = null;

            // static ExportDefinition CreateExportDefinition(ComposablePartDefinition owner, IDictionary<string, object> arg0)
            this._createExportDefinitionMethod = this._stubBuilder.DefineMethod(
                CacheStructureConstants.CachingStubCreateExportDefinitionMethodName,
                MethodAttributes.Static | MethodAttributes.Assembly,
                AssemblyCacheGenerator._exportDefinitionType,
                new Type[] { AssemblyCacheGenerator._composablePartDefinitionType, AssemblyCacheGenerator._standardIDictionaryType });
            ilGenerator = this._createExportDefinitionMethod.GetILGenerator();

            // CachingStub.ExportDefinitionFactory.Invoke(owner, cache)
            ilGenerator.Emit(OpCodes.Ldsfld, exportDefinitionFactoryFieldBuilder); // load the field
            ilGenerator.Emit(OpCodes.Ldarg_0); // load the part
            ilGenerator.Emit(OpCodes.Ldarg_1); // load the dictionary
            ilGenerator.EmitCall(OpCodes.Callvirt, AssemblyCacheGenerator._stubExportDefinitionFactoryInvoke, null);
            ilGenerator.Emit(OpCodes.Ret);


            // static ImportDefinition CreateImportDefinition(ComposablePartDefinition owner, IDictionary<string, object> arg0)
            this._createImportDefinitionMethod = this._stubBuilder.DefineMethod(
                CacheStructureConstants.CachingStubCreateImportDefinitionMethodName,
                MethodAttributes.Static | MethodAttributes.Assembly,
                AssemblyCacheGenerator._importDefinitionType,
                new Type[] { AssemblyCacheGenerator._composablePartDefinitionType, AssemblyCacheGenerator._standardIDictionaryType });
            ilGenerator = this._createImportDefinitionMethod.GetILGenerator();

            // CachingStub.ImportDefinitionFactory.Invoke(owner, cache)
            ilGenerator.Emit(OpCodes.Ldsfld, importDefinitionFactoryFieldBuilder); // load the field
            ilGenerator.Emit(OpCodes.Ldarg_0); // load the part
            ilGenerator.Emit(OpCodes.Ldarg_1); // load the dictionary
            ilGenerator.EmitCall(OpCodes.Callvirt, AssemblyCacheGenerator._stubImportDefinitionFactoryInvoke, null);
            ilGenerator.Emit(OpCodes.Ret);

            // ComposablePartDefinition CreatePartDefinition(IDictionary<string, object> arg0, IntPtr arg1, IntPtr arg2)
            this._createPartDefinitionMethod = this._stubBuilder.DefineMethod(
                CacheStructureConstants.CachingStubCreatePartDefinitionMethodName,
                MethodAttributes.Static | MethodAttributes.Assembly,
                AssemblyCacheGenerator._composablePartDefinitionType,
                new Type[] { AssemblyCacheGenerator._standardIDictionaryType, typeof(IntPtr), typeof(IntPtr) });
            ilGenerator = this._createPartDefinitionMethod.GetILGenerator();

            //        return CachingStub.PartDefinitionFactory.Invoke(
            //             cache, 
            //             new Func<ComposablePartDefinition, IEnumerable<ImportDefinition>>(null, importsFactoryPtr),
            //             new Func<ComposablePartDefinition, IEnumerable<ExportDefinition>>(null, exportssFactoryPtr));

            ilGenerator.Emit(OpCodes.Ldsfld, partDefinitionFactoryFieldBuilder); // load the field
            ilGenerator.Emit(OpCodes.Ldarg_0); // load the dictionary

            //
            // new Func<ComposablePartDefinition, IEnumerable<ImportDefinition>>(null, (importsFactoryPtr!=0)? importsFactory : null)
            //
            Label importsNotNull = ilGenerator.DefineLabel();
            Label importsDone = ilGenerator.DefineLabel();

            // if (importsfactoryPtr != null)
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Brtrue_S, importsNotNull);

            // else { importsFactory = null }
            ilGenerator.Emit(OpCodes.Ldnull);
            ilGenerator.Emit(OpCodes.Br_S, importsDone);

            // then { importsfactory = importsfactoryPtr }
            ilGenerator.MarkLabel(importsNotNull);
            this._generationServices.LoadValue(ilGenerator, null);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Newobj, AssemblyCacheGenerator._importsFactoryDelegateConstructor);

            ilGenerator.MarkLabel(importsDone);

            //
            // new Func<ComposablePartDefinition, IEnumerable<ExportDefinition>>(null, exportssFactoryPtr)
            //
            Label exportsNotNull = ilGenerator.DefineLabel();
            Label exportsDone = ilGenerator.DefineLabel();

            // if (exportsfactoryPtr != null)
            ilGenerator.Emit(OpCodes.Ldarg_2);
            ilGenerator.Emit(OpCodes.Brtrue_S, exportsNotNull);

            // else { exportsFactory = null }
            ilGenerator.Emit(OpCodes.Ldnull);
            ilGenerator.Emit(OpCodes.Br_S, exportsDone);

            // then { exportsfactory = exportsfactoryPtr }
            ilGenerator.MarkLabel(exportsNotNull);
            this._generationServices.LoadValue(ilGenerator, null);
            ilGenerator.Emit(OpCodes.Ldarg_2);
            ilGenerator.Emit(OpCodes.Newobj, AssemblyCacheGenerator._exportsFactoryDelegateConstructor);

            ilGenerator.MarkLabel(exportsDone);

            // Call the part definition factory
            ilGenerator.EmitCall(OpCodes.Callvirt, AssemblyCacheGenerator._stubPartDefinitionFactoryInvoke, null);
            ilGenerator.Emit(OpCodes.Ret);

            //
            // Define ComposablePartDefinition GetPartFactory (IntPtr funcPtr)
            //
            MethodBuilder getPartFactory = this._stubBuilder.DefineMethod(
                CacheStructureConstants.CachingStubGetPartFactoryMethodName,
                MethodAttributes.Static | MethodAttributes.Public,
                typeof(ComposablePartDefinition),
                new Type[] {typeof(IntPtr)});
            ilGenerator = getPartFactory.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.EmitCalli(OpCodes.Calli, CallingConventions.Standard, typeof(ComposablePartDefinition), Type.EmptyTypes, null);
            ilGenerator.Emit(OpCodes.Ret);

            //
            // Define the "GetCatalogMetadata" method. The body will be generated on completion
            // 
            this._getCatalogMetadata = this._stubBuilder.DefineMethod(
                CacheStructureConstants.CachingStubGetCatalogMetadata,
                MethodAttributes.Static | MethodAttributes.Public,
                AssemblyCacheGenerator._standardIDictionaryType,
                Type.EmptyTypes);

            //
            // Define the "GetCatalogIndex" method. The body will be generated on completion
            // 
            this._getCatalogIndex = this._stubBuilder.DefineMethod(
                CacheStructureConstants.CachingStubGetCatalogIndexMethodName,
                MethodAttributes.Static | MethodAttributes.Public,
                typeof(IDictionary<string, IEnumerable<IntPtr>>),
                Type.EmptyTypes);

            // 
            // Define the "GetCatalogIdentifier
            //
            MethodBuilder getCatalogIdentifier = this._stubBuilder.DefineMethod(
                CacheStructureConstants.CachingStubGetCatalogIdentifier,
                MethodAttributes.Static | MethodAttributes.Public,
                typeof(string),
                Type.EmptyTypes);
            ilGenerator = getCatalogIdentifier.GetILGenerator();

            this._generationServices.LoadValue(ilGenerator, this._catalogIdentifier);
            ilGenerator.Emit(OpCodes.Ret);
        }

        private CachingResult GenerateGetCatalogIndex()
        {
            Assumes.NotNull(this._getCatalogIndex);

            ILGenerator ilGenerator = this._getCatalogIndex.GetILGenerator();
            // Dictionary = new Dictionary();
            ilGenerator.Emit(OpCodes.Newobj, AssemblyCacheGenerator._catalogIndexDictionaryConstructor);
            foreach (KeyValuePair<string, List<MethodInfo>> kvp in this._catalogIndex)
            {
                string contractName = kvp.Key;
                List<MethodInfo> values = kvp.Value;

                // load the dictionary on the stack
                ilGenerator.Emit(OpCodes.Dup);
                // load the index on the stack
                ilGenerator.Emit(OpCodes.Ldstr, contractName);

                // create the array and load the value on the stack
                ilGenerator.Emit(OpCodes.Ldc_I4, values.Count);
                ilGenerator.Emit(OpCodes.Newarr, typeof(IntPtr));

                for (int i = 0; i < values.Count; i++)
                {
                    // load the array again
                    ilGenerator.Emit(OpCodes.Dup);
                    // load index
                    ilGenerator.Emit(OpCodes.Ldc_I4, i);
                    // load value - in thise case the function pointer
                    ilGenerator.Emit(OpCodes.Ldftn, values[i]);
                    // set the array element
                    ilGenerator.Emit(OpCodes.Stelem, typeof(IntPtr));
                }

                // at this point the array is on the stack, so we can call the "Add"
                ilGenerator.EmitCall(OpCodes.Call, AssemblyCacheGenerator._catalogIndexDictionaryAddMethod, null);
            }

            // at this point the dictionary is on the stack, just return it
            ilGenerator.Emit(OpCodes.Ret);

            return CachingResult.SucceededResult;
        }

        private CachingResult GenerateGetCatalogMetadata()
        {
            Assumes.NotNull(this._getCatalogMetadata);
            Assumes.NotNull(this._catalogMetadata);

            ILGenerator ilGenerator = this._getCatalogMetadata.GetILGenerator();
            CachingResult result = this._generationServices.LoadValue(ilGenerator, this._catalogMetadata);
            ilGenerator.Emit(OpCodes.Ret);

            return result;
        }


        public void CacheCatalogMetadata(IDictionary<string, object> catalogMetadata)
        {
            Assumes.NotNull(catalogMetadata);
            foreach (KeyValuePair<string, object> kvp in catalogMetadata)
            {
                this._catalogMetadata[kvp.Key] = kvp.Value;
            }
        }


        public CachingResult<MethodInfo> CachePartDefinition(ComposablePartDefinition partDefinition)
        {
            Assumes.NotNull(partDefinition);
            CachingResult result = CachingResult.SucceededResult;
            string methodName = string.Format(CultureInfo.InvariantCulture, "{0}", this._partsCounter);


            //Ftypeof
            // public static ComposablePartDefinition<>()
            // {
            //    // load dictionary
            //    return CachingStubX.CreatePartDefinition(<dictinary>, <importsFactory>, <exportsFactory>);
            // }

            // Generate the signature
            MethodBuilder partFactoryBuilder = this._partsDefinitionBuilder.DefineMethod(
                methodName,
                MethodAttributes.Static | MethodAttributes.Public,
                AssemblyCacheGenerator._composablePartDefinitionType,
                Type.EmptyTypes);
            ILGenerator ilGenerator = partFactoryBuilder.GetILGenerator();

            // Generate imports caching
            CachingResult<MethodInfo> importsFactoryResult = this.CachePartImportsOrExports<ImportDefinition>(
                partDefinition.ImportDefinitions,
                this._importsDefinitionBuilder,
                this._createImportDefinitionMethod,
                (import) => this._cachedCatalogSite.CacheImportDefinition(partDefinition, import),
                methodName);
            result = result.MergeErrors(importsFactoryResult.Errors);

            // Generate exports caching
            CachingResult<MethodInfo> exportsFactoryResult = this.CachePartImportsOrExports<ExportDefinition>(
                partDefinition.ExportDefinitions,
                this._exportsDefinitionBuilder,
                this._createExportDefinitionMethod,
                (export) => this._cachedCatalogSite.CacheExportDefinition(partDefinition, export),
                methodName);
            result = result.MergeErrors(exportsFactoryResult.Errors);


            // get the actual cache for the part definition
            IDictionary<string, object> cache = this._cachedCatalogSite.CachePartDefinition(partDefinition);

            //
            // now write the method
            //

            // load the cache dictionary on stack
            result = result.MergeResult(this._generationServices.LoadValue(ilGenerator, cache));

            // load the imports function pointer on stack
            MethodInfo importsFactory = importsFactoryResult.Value;
            if (importsFactory != null)
            {
                ilGenerator.Emit(OpCodes.Ldftn, importsFactory);
            }
            else
            {
                // load IntPtr.Zero
                ilGenerator.Emit(OpCodes.Ldsfld, AssemblyCacheGenerator._IntPtr_Zero);
            }

            // load the exports function pointer on stack
            MethodInfo exportsFactory = exportsFactoryResult.Value;
            if (exportsFactory != null)
            {
                ilGenerator.Emit(OpCodes.Ldftn, exportsFactory);
            }
            else
            {
                // load IntPtr.Zero
                ilGenerator.Emit(OpCodes.Ldsfld, AssemblyCacheGenerator._IntPtr_Zero);
            }

            // and then call into stub.CreatePartDefinition and return 
            ilGenerator.EmitCall(OpCodes.Call, this._createPartDefinitionMethod, null);
            ilGenerator.Emit(OpCodes.Ret);
            this._partsCounter++;

            this.UpdateCatalogIndex(partDefinition, partFactoryBuilder);

            return result.ToResult<MethodInfo>(partFactoryBuilder);
        }

        private CachingResult<MethodInfo> CachePartImportsOrExports<T>(IEnumerable<T> items, TypeBuilder definitionsTable, MethodBuilder stubFactoryMethod, Func<T, IDictionary<string, object>> cacheGenerator, string methodName)
             // in reality this is only ExportDefinition or ImportDefinition
        {
            Assumes.NotNull(items);
            Assumes.NotNull(definitionsTable);
            Assumes.NotNull(stubFactoryMethod);
            Assumes.NotNull(cacheGenerator);
            Assumes.NotNull(methodName);
            CachingResult result = CachingResult.SucceededResult;

            if (!items.Any())
            {
                return result.ToResult<MethodInfo>(null);
            }

            Type itemType = null;
            if (typeof(T) == AssemblyCacheGenerator._importDefinitionType)
            {
                itemType = AssemblyCacheGenerator._importDefinitionType;
            }
            else
            {
                itemType = AssemblyCacheGenerator._exportDefinitionType;
            }

            //
            // internal static IEnumerable<T> CreateTs(ComposablePartDefinition owner)
            // {
            //    T[] items = new ImportDefinition[<count>];
            //    
            //    IDictionary<string, object> dictionary0 = new Dictionary<string, object>();
            //    <populate the dictionary with the cache values>
            //    items[0] = CachingStubX.CreateTDefinition(dictinary0);
            //    ...
            //    IDictionary<string, object> dictionary<count-1> = new Dictionary<string, object>();
            //    <populate the dictionary with the cache values>
            //    items[<count-1>] = CachingStubX.CreateTDefinition(dictinary<count-1>);
            //    ...
            //    return items;
            // }

            // Generate the signature
            MethodBuilder itemsFactoryBuilder = definitionsTable.DefineMethod(
                methodName,
                MethodAttributes.Static | MethodAttributes.Assembly,
                typeof(IEnumerable<T>),
                new Type[] {AssemblyCacheGenerator._composablePartDefinitionType});
            ILGenerator ilGenerator = itemsFactoryBuilder.GetILGenerator();

            //
            // Generate array creation
            // 
            this._generationServices.LoadValue(ilGenerator, items.Count());
            ilGenerator.Emit(OpCodes.Newarr, itemType);
            // At this point the array is on the stack

            int index = 0;
            foreach (T item in items)
            {
                // get the cache
                IDictionary<string, object> cache = cacheGenerator(item);

                //
                //items[<index>] = stub.CreateTDefinition(<dictionary>)
                //
                ilGenerator.Emit(OpCodes.Dup); // this will load the array on the stack
                result = result.MergeResult(this._generationServices.LoadValue(ilGenerator, index));

                ilGenerator.Emit(OpCodes.Ldarg_0); // load the part definition
                result = result.MergeResult(this._generationServices.LoadValue(ilGenerator, cache)); // load the dictionary
                ilGenerator.EmitCall(OpCodes.Call, stubFactoryMethod, null);

                ilGenerator.Emit(OpCodes.Stelem, itemType);
                index++;
                // at this point the duplicate array has been popped from the stack
            }

            // just return - the stack already contains the array
            ilGenerator.Emit(OpCodes.Ret);

            return result.ToResult<MethodInfo>(itemsFactoryBuilder);
        }

        private void UpdateCatalogIndex(ComposablePartDefinition partDefinition, MethodInfo partDefinitionFactory)
        {
            Assumes.NotNull(partDefinition); 
            Assumes.NotNull(partDefinitionFactory);

            foreach (string contractName in partDefinition.ExportDefinitions.Select(export => export.ContractName).Distinct())
            {
                List<MethodInfo> contractPartFactories = null;
                if (!this._catalogIndex.TryGetValue(contractName, out contractPartFactories))
                {
                    contractPartFactories = new List<MethodInfo>();
                    this._catalogIndex.Add(contractName, contractPartFactories);
                }
                contractPartFactories.Add(partDefinitionFactory);
            }
        }
    }

}
