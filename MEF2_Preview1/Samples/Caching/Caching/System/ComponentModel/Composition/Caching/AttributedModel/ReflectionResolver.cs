// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Linq;
using System.Reflection;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Caching
{
    internal static class ReflectionResolver
    {
        public static Assembly ResolveAssembly(string assemblyName, string codeBase)
        {
            AssemblyName name = (assemblyName != null) ? new AssemblyName(assemblyName) : new AssemblyName();
            name.CodeBase = codeBase;
            return Assembly.Load(name);
        }
        
        public static Module ResolveModule(Assembly assembly, int metadataToken)
        {
            Assumes.NotNull(assembly);
            Assumes.IsTrue(metadataToken != 0);

            // TODO: This likely cause all modules in the assembly to be loaded
            // perhaps we should load via file name (how will that work on SL)?
            foreach (Module module in assembly.GetModules())
            {
                if (module.MetadataToken == metadataToken)
                {
                    return module;
                }
            }

            Assumes.Fail("");
            return null;
        }

        public static MemberInfo ResolveMember(Module module, int metadataToken)
        {
            Assumes.NotNull(module);
            Assumes.IsTrue(metadataToken != 0);

            return module.ResolveMember(metadataToken);
        }

    }
}
