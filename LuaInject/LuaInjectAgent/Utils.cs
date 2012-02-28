using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using EasyHook;

namespace LuaInjectAgent
{
    public class Utils
    {
        public static LocalHook SetupHook<T>(string function, string hookedModule, Func<T, T> hookProcedure, object callback = null)
        {
            var hook = LocalHook.Create(
                LocalHook.GetProcAddress(hookedModule, function),
                hookProcedure(LocalHook.GetProcDelegate<T>(hookedModule, function)) as Delegate,
                callback);
            
            hook.ThreadACL.SetExclusiveACL(new[] { 0 });

            return hook;
        }

        // TODO: add some typical error handling
        private static T DynamicDllInvoke<T>(string functionName, string library)
        {
            // create in-memory assembly, module and type
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("DynamicDllInvoke"),
                AssemblyBuilderAccess.Run);

            ModuleBuilder modBuilder = assemblyBuilder.DefineDynamicModule("DynamicDllModule");

            // note: without TypeBuilder, you can create global functions
            // on the module level, but you cannot create delegates to them
            TypeBuilder typeBuilder = modBuilder.DefineType(
                "DynamicDllInvokeType",
                TypeAttributes.Public | TypeAttributes.UnicodeClass);

            // get params from delegate dynamically (!), trick from Eric Lippert
            MethodInfo delegateMI = typeof(T).GetMethod("Invoke");
            Type[] delegateParams = (from param in delegateMI.GetParameters()
                                     select param.ParameterType).ToArray();

            // automatically create the correct signagure for PInvoke
            MethodBuilder methodBuilder = typeBuilder.DefinePInvokeMethod(
                functionName,
                library,
                MethodAttributes.Public |
                MethodAttributes.Static |
                MethodAttributes.PinvokeImpl,
                CallingConventions.Standard,
                delegateMI.ReturnType,        /* the return type */
                delegateParams,               /* array of parameters from delegate T */
                CallingConvention.Winapi,
                CharSet.Ansi);

            // needed according to MSDN
            methodBuilder.SetImplementationFlags(
                methodBuilder.GetMethodImplementationFlags() |
                MethodImplAttributes.PreserveSig);

            Type dynamicType = typeBuilder.CreateType();

            MethodInfo methodInfo = dynamicType.GetMethod(functionName);

            // create the delegate of type T, double casting is necessary
            return (T)(object)Delegate.CreateDelegate(
                typeof(T),
                methodInfo, true);
        }
    }
}
