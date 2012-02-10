// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using Microsoft.Internal;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.IO.Compression;

namespace System.ComponentModel.Composition.Caching
{
    internal class ComposablePartCatalogAssemblyCacheReader : ComposablePartCatalogCacheReader
    {
        private Assembly _cacheAssembly;
        private string _rootStubSuffix;

        public ComposablePartCatalogAssemblyCacheReader(Assembly cacheAssembly)
        {
            Requires.NotNull(cacheAssembly, "cacheAssembly");
            this._cacheAssembly = cacheAssembly;

            // read the entry point
            Type entryPointType = ComposablePartCatalogAssemblyCacheReader.GetCacheType(this._cacheAssembly, CacheStructureConstants.EntryPointTypeName);
            MethodInfo getRootStubMethod = ComposablePartCatalogAssemblyCacheReader.GetCacheTypeMethod(entryPointType, CacheStructureConstants.EntryPointGetRootStubMethodName, BindingFlags.Static | BindingFlags.Public);

            Func<string> getRootStub = (Func<string>)Delegate.CreateDelegate(typeof(Func<string>), getRootStubMethod);
            this._rootStubSuffix = getRootStub.Invoke();
        }

        protected override ComposablePartCatalogCache ReadCacheCore(object cacheToken)
        {
            string rootStubSuffix = cacheToken as string;
            if (rootStubSuffix == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Strings.InvalidCacheToken, cacheToken), "cacheToken");
            }

            Type stubType = ComposablePartCatalogAssemblyCacheReader.GetCacheType(this._cacheAssembly, string.Format(CultureInfo.InvariantCulture, "{0}{1}", CacheStructureConstants.CachingStubTypeNamePrefix, rootStubSuffix));
            return new ComposablePartCatalogAssemblyCache(stubType, this);
        }

        protected override object RootCacheToken
        {
            get
            {
                return this._rootStubSuffix;
            }
        }

        internal static Type GetCacheType(Assembly assembly, string name)
        {
            Type type = assembly.GetType(name);
            if (type == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Strings.CachingTypeNotFound, name));
            }
            return type;
        }

        internal static MethodInfo GetCacheTypeMethod(Type type, string name, BindingFlags bindingFlags)
        {
            MethodInfo method = type.GetMethod(name, bindingFlags);
            if (method == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Strings.CachingMethodNotFound, type.FullName, name));
            }
            return method;
        }

        internal static FieldInfo GetCacheTypeField(Type type, string name, BindingFlags bindingFlags)
        {
            FieldInfo field = type.GetField(name, bindingFlags);
            if (field == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Strings.CachingFieldNotFound, type.FullName, name));
            }
            return field;
        }
    }
}
